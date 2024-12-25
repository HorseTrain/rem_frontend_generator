using System.Text;
using rem_frontend_generator.language;

namespace rem_frontend_generator.generators
{
    public class rem_generator
    {
        public StringBuilder header_file            { get; set; }
        public StringBuilder cpp_file               { get; set; }
        public Stack<bool>   cf_runtime             { get; set; }
        Dictionary<string, string> generic_remap    { get; set; }

        public rem_generator()
        {

        }

        const string runtime_operand_type = "ir_operand";
        const string compile_time_operand_type = "uint64_t";

        bool in_cf_runtime()
        {
            foreach (bool b in cf_runtime)
            {
                if (b)
                    return true;
            }

            return false;
        }

        string get_explicit_rem_type(variable_type source)
        {
            switch (source)
            {
                case generic_runtime_variable_type grvt: return grvt.name;
                case runtime_variable rv: return rv.size.ToString();
                case generic_declaration gd: return gd.name;
                default: throw new Exception();
            }
        }

        string get_rem_type(variable_type source, bool interpreted)
        {
            if (source is compile_time_type cto && cto.is_void)
            {
                return "void";
            }

            if (interpreted)
            {
                if (!source.is_runtime())
                {
                    return compile_time_operand_type;
                }
                else
                {
                    switch (source)
                    {
                        case generic_runtime_variable_type grt: return grt.name;
                        case runtime_variable rvs: return $"u{rvs.size}_t";
                        case generic_declaration gd:
                        {
                            if (generic_remap.ContainsKey(gd.name))
                            {
                                return generic_remap[gd.name];
                            }

                            return gd.name;
                        }; break;
                        default: throw new Exception();
                    }
                }
            }
            else
            {
                return source.is_runtime() ? runtime_operand_type : compile_time_operand_type;
            }
        }

        string get_name_with_interpreted(string source, bool interpreted)
        {
            if (interpreted)
            {
                return $"{source}_interpreter";
            }
            else
            {
                return $"{source}_jit";
            }
        }

        string get_default_argument(bool interpreted)
        {
            if (interpreted)
            {
                return "ctx";
            }
            else
            {
                return "ctx";
            }
        }

        string get_default_parameter(bool interpreted)
        {
            if (interpreted)
            {
                return "interpreter_data* ctx";
            }
            else
            {
                return "ssa_emit_context* ctx";
            }
        }

        string get_operation(string name, bool runtime, out bool is_signed)
        {
            is_signed = false;

            if (runtime)
            {
                switch (name)
                {
                    case "+": return "ir_add"; 
                    case "-": return "ir_subtract";
                    case "&": return "ir_bitwise_and";
                    case "|": return "ir_bitwise_or";
                    case "clt": return "ir_compare_less_signed";
                    case "<": return "ir_compare_less_unsigned";
                    case "==": return "ir_compare_equal";
                    case "^": return "ir_bitwise_exclusive_or";
                    case "~": return "ir_bitwise_not";
                    case ">=": return "ir_compare_greater_equal_unsigned";
                    default: throw new Exception();
                }
            }
            else
            {
                switch (name)
                {
                    case "sar": is_signed = true; return ">>";
                    case "clt": is_signed = true; return "<";
                    case "cgt": is_signed = true; return ">";
                    case "clte": is_signed = true; return "<=";
                    case "cgte": is_signed = true; return ">=";

                    default:
                        return name;
                }
            }
        }

        string sign_compile_time(string source)
        {
            return $"(int64_t){source}";
        }

        string convert_to_runtime(string source, variable_type type, bool interpreted)
        {
            if (interpreted)
                throw new Exception();

            return $"ir_operand::create_con({source}, {get_explicit_rem_type(type)})";
        }

        string convert_to_other_runtime(string source, variable_type type, bool interpreted)
        {
            if (interpreted)
                throw new Exception();

            return $"ir_operand::copy_new_raw_size({source}, {get_explicit_rem_type(type)})";
        }

        string generate_object(i_ast_object data, bool interpreted, bool is_command = false)
        {
            switch (data)
            {
                case scope sc:
                {
                    string result = "{\n";

                    foreach (i_ast_object command in sc.commands)
                    {
                        result += string_tools.tab_string(generate_object(command, interpreted, true)) + "\n";
                    }

                    result += "}";

                    return result;
                };

                case return_statement rs:
                {
                    string result = "return";

                    if (rs.to_return == null)
                    {
                        return $"{result};";
                    }
                    else
                    {
                        string value =  generate_object(rs.to_return, interpreted);

                        if (!interpreted && rs.to_runtime_conversion_needed())
                        {
                            value = convert_to_runtime(value, rs.return_type, interpreted);
                        }
                        
                        return $"{result} {value};";
                    }
                };

                case variable_declaration vd:
                {
                    string result = $"{get_rem_type(vd.type, interpreted)} {vd.variable_name}";

                    if (vd.default_value != null)
                    {
                        string default_value = generate_object(vd.default_value, interpreted);

                        if (!interpreted && vd.to_runtime_conversion_needed())
                        {
                            default_value = convert_to_runtime(default_value, vd.type, interpreted);
                        }

                        result = $"{result} = {default_value};";
                    }
                    else
                    {
                        result = $"{result};";
                    }

                    return result;
                }; 

                case cast ca:
                {
                    string result = generate_object(ca.value, interpreted);

                    if (interpreted)
                    {
                        return $"({get_rem_type(ca.new_type, interpreted)}){result}";
                    }
                    else
                    {
                        if (!ca.value.is_runtime())
                        {
                            return convert_to_runtime(result, ca.new_type, interpreted);
                        }
                        else
                        {
                            return convert_to_other_runtime(result, ca.new_type, interpreted);
                        }
                    }
                }

                case unary_operation uo:
                {
                    string result = generate_object(uo.value, interpreted);

                    if (uo.operation == "()")
                    {
                        if (!interpreted && uo.is_runtime())
                        {
                            return generate_object(uo.value, interpreted);
                        }
                        else
                        {
                            return $"({result})";
                        }
                    }

                    if (uo.is_runtime() && !interpreted)
                    {
                        if (!uo.value.is_runtime()) result = convert_to_runtime(result, uo.get_type(), false);

                        return $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)}, {get_operation(uo.operation, true, out _)}, {result})";
                    }
                    else
                    {
                        string raw_operation = get_operation(uo.operation, false, out bool is_signed);

                        if (is_signed)
                        {
                            raw_operation = sign_compile_time(raw_operation);
                        }

                        return $"{raw_operation} {result}";
                    }
                }; break;

                case binary_operation bo:
                {
                    string left = generate_object(bo.left, interpreted);
                    string right = generate_object(bo.right, interpreted);

                    if (bo.is_runtime() && !interpreted)
                    {
                        if (!bo.left.is_runtime()) left = convert_to_runtime(left, bo.get_type(), false);
                        if (!bo.right.is_runtime()) right = convert_to_runtime(right, bo.get_type(), false);

                        return $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)}, {get_operation(bo.operation, true, out _)}, {left}, {right})";
                    }
                    else
                    {
                        string raw_operation = get_operation(bo.operation, false, out bool is_signed);
                        
                        if (is_signed)
                        {
                            left = sign_compile_time(left);
                            right = sign_compile_time(right);
                        }

                        return $"{left} {raw_operation} {right}";
                    }
                };

                case if_statment ifs:
                {
                    if (!interpreted && ifs.condition.is_runtime())
                    {
                        cf_runtime.Push(true);

                        throw new Exception();

                        cf_runtime.Pop();
                    }
                    else
                    {
                        string result = $"if ({generate_object(ifs.condition, interpreted)})\n{generate_object(ifs.yes, interpreted, true)}";

                        if (ifs.no != null)
                        {
                            result += $"\nelse{(ifs.no is if_statment ? " " : "\n")}{generate_object(ifs.no, interpreted, true)}";
                        }

                        return result;
                    }
                }; 

                case function_call fc:
                {
                    string result = get_name_with_interpreted(fc.function_name, interpreted);

                    if (interpreted)
                    {
                        if (fc.generics.Count != 0)
                        {
                            result += "<";

                            for (int i = 0; i < fc.generics.Count; ++i)
                            {
                                var g = fc.generics[i];

                                result += get_rem_type(g, interpreted);

                                if (i != fc.generics.Count - 1)
                                {
                                    result += ",";
                                }
                            }

                            result += ">";
                        }

                        result += $"({get_default_argument(interpreted)}";
                    }
                    else
                    {
                        result += $"({get_default_argument(interpreted)}";

                        if (fc.generics.Count != 0)
                        {
                            foreach (var v in fc.generics)
                            {
                                result += $",{get_explicit_rem_type(v)}";
                            }
                        }
                    }

                    int argument_index = 0;

                    foreach (var a in fc.function_arguments)
                    {
                        string raw_argument = generate_object(a, interpreted, false);

                        if (!a.is_runtime() && fc.function_reference.parameters[argument_index].type.is_runtime() && !interpreted)
                        {
                            raw_argument = convert_to_runtime(raw_argument, fc.function_reference.parameters[argument_index].type, interpreted);
                        }

                        result += $",{raw_argument}";

                        argument_index++;
                    }

                    result += ")";

                    if (is_command)
                    {
                        result += ";";
                    }

                    return result;
                }; 

                case l_value_set lvs:
                {
                    if (!interpreted && in_cf_runtime())
                    {
                        throw new Exception();
                    }
                    else
                    {
                        string value = generate_object(lvs.r_value, interpreted);

                        if (!interpreted && lvs.to_runtime_conversion_needed())
                        {
                            value = convert_to_runtime(value, lvs.l_value.get_type(),interpreted);
                        }
                        
                        return $"{generate_object(lvs.l_value,interpreted)} = {value};";
                    }
                };

                case number nu:
                {
                    return nu.value.ToString() + "ULL";
                };

                case generic_declaration gd:
                {
                    string test = generate_object(gd.new_type.test, interpreted);

                    if (interpreted)
                    {
                        string result = "";

                        generic_remap.Add(gd.name, "");

                        foreach (generic_declaration_case c in gd.new_type.conditions)
                        {
                            generic_remap[gd.name] = get_rem_type(c.type, interpreted);

                            result += $"if ({test} == {generate_object(c.condition, interpreted)})\n{generate_object(gd.body, interpreted)}\n";
                        }

                        generic_remap.Remove(gd.name);

                        return result;
                    }
                    else
                    {
                        string result = $"{compile_time_operand_type} {gd.name} = ";

                        foreach (generic_declaration_case c in gd.new_type.conditions)
                        {
                            result += $"{test} == {generate_object(c.condition, interpreted, false)} ? {get_explicit_rem_type(c.type)} : ";
                        }

                        result += "throw 0;\n";

                        result += generate_object(gd.body, interpreted);

                        return result;
                    }
                }; 

                case object_reference or:
                {
                    switch (or.reference)
                    {
                        case variable_declaration vd: return vd.variable_name;
                        default: throw new Exception();
                    }
                }

                default: throw new Exception();
            }
        }

        string generate_function_header(function source_function,bool implament_body, bool interpreted, out bool is_external)
        {
            string variable_type = get_rem_type(source_function.return_type, interpreted);
            string function_name = get_name_with_interpreted(source_function.function_name, interpreted);

            string result = $"{variable_type} {function_name}";

            result += $"({get_default_parameter(interpreted)}";

            if (source_function.generics.Count != 0)
            {
                if (interpreted)
                {
                    string template_data = "template <";

                    foreach (generic_runtime_variable_type gd in source_function.generics)
                    {
                        template_data += $"typename {gd.name}";

                        if (gd != source_function.generics.Last())
                            template_data += ",";
                    }

                    template_data += ">";

                    result = $"{template_data}\n{result}";
                }
                else
                {
                    foreach (generic_runtime_variable_type gd in source_function.generics)
                    {
                        result += $",{compile_time_operand_type} {gd.name}";
                    }
                }
            }

            foreach (variable_declaration parameter in source_function.parameters)
            {
                result += $", {get_rem_type(parameter.type, interpreted)} {parameter.variable_name}";
            }

            result += ")";

            is_external = source_function.is_external;

            if (!is_external && implament_body)
            {
                result += $"\n{generate_object(source_function.function_body, interpreted)}";
            }
            else
            {
                result += ";";
            }

            return result;
            
        }   

        static string copy_data(char src, int count)
        {
            string result = "";

            for (int i = 0; i < count; ++i)
            {
                result += src;
            }

            return result;
        }

        public void generate_files(source_file sf)
        {
            cf_runtime = new Stack<bool>();
            generic_remap = new Dictionary<string, string>();

            header_file = new StringBuilder(@"#include <inttypes.h>
#include ""emulator/ssa_emit_context.h""
#include ""aarch64_context_offsets.h""
#include ""aarch64_process.h""

enum branch_type
{
    no_branch       = 0,
    short_branch    = 1 << 0,
    long_branch     = 1 << 1
};

struct interpreter_data
{
    aarch64_process*    process_context;
    void*               register_data;
    uint64_t            current_pc;
    int                 branch_type;
};

void init_aarch64_decoder(aarch64_process* process);

");
            cpp_file = new StringBuilder(@"#include ""aarch64_impl.h""
#include ""string.h""

static void append_table(aarch64_process* process, std::string encoding, void* emit, void* interperate)
{
	uint32_t instruction = 0;
	uint32_t mask = 0;

	for (int i = 0; i < 32; ++i)
	{
		char working = encoding[i];

		int shift = 31 - i;

		if (working == '1')
		{
			instruction |= 1UL << shift;
		}

		if (working != '-')
		{
			mask |= 1UL << shift;
		}
	}

	fixed_length_decoder<uint32_t>::insert_entry(&process->decoder, instruction, mask, emit, interperate);
}

");

            string table_create_function = $"void init_aarch64_decoder(aarch64_process* process)\n{{\n";

            foreach (function f in sf.functions.Values)
            {
                if (f.fixed_length_operand_data.Count == 0)
                    continue;

                string instruction = "";
                
                Dictionary<string, int> offsets =new Dictionary<string, int>();

                foreach (var op in f.fixed_length_operand_data)
                {
                    if (op.is_encoding)
                    {
                        instruction += op.data;
                    }
                    else
                    {
                        instruction += copy_data('-', op.size);

                        offsets.Add(op.data, 32 - instruction.Length);
                    }
                }

                if (instruction.Length != 32)
                {
                    throw new Exception();
                }

                table_create_function += $"\tappend_table(process, \"{instruction}\", (void*)emit_{get_name_with_interpreted(f.function_name, false)}, (void*)call_{get_name_with_interpreted(f.function_name, true)});\n";

                {
                    cpp_file.Append($"static void call_{get_name_with_interpreted(f.function_name, true)}({get_default_parameter(true)}, uint32_t instruction)\n{{\n");

                    string function_call = get_name_with_interpreted(f.function_name, true) + $"({get_default_argument(true)}";

                    foreach (var op in f.fixed_length_operand_data)
                    {
                        if (op.is_encoding)
                            continue;

                        cpp_file.Append($"\tint {op.data} = (instruction >> {offsets[op.data]}) & {(1 << op.size) - 1};\n");

                        function_call += $", {op.data}";
                    }

                    function_call += ")";

                    cpp_file.Append($"\t{function_call};\n");

                    cpp_file.Append("}\n\n");
                }

                {
                    cpp_file.Append($"static void emit_{get_name_with_interpreted(f.function_name, false)}({get_default_parameter(false)}, uint32_t instruction)\n{{\n");

                    string function_call = get_name_with_interpreted(f.function_name, false) + $"({get_default_argument(false)}";

                    foreach (var op in f.fixed_length_operand_data)
                    {
                        if (op.is_encoding)
                            continue;

                        cpp_file.Append($"\tint {op.data} = (instruction >> {offsets[op.data]}) & {(1 << op.size) - 1};\n");

                        function_call += $", {op.data}";
                    }

                    function_call += ")";

                    cpp_file.Append($"\t{function_call};\n");

                    cpp_file.Append("}\n\n");
                }
            }

            table_create_function += "}";

            cpp_file.Append(table_create_function + "\n\n");

            header_file.Append("//INTERPRETER\n");

            generate_files(sf, true);

            header_file.Append("\n//JIT\n");

            generate_files(sf, false);
        }

        void generate_files(source_file sf, bool interpreted)
        {
            string external_functions = "";

            foreach (function f in sf.functions.Values)
            {
                string function_header = generate_function_header(f, false, interpreted, out bool is_external);

                if (!is_external)
                    continue;

                external_functions += function_header + "//THIS FUNCTION IS USER DEFINED\n";
            }

            foreach (function f in sf.functions.Values)
            {
                string function_header = generate_function_header(f, false, interpreted, out bool is_external);             

                if (is_external)
                    continue;

                string function_body = generate_function_header(f, true, interpreted, out bool _);

                header_file.Append(function_header + "\n");
                cpp_file.Append(function_body + "\n\n");
            }

            header_file.Append(external_functions);
        }

        public void store_to(string path, string name)
        {
            File.WriteAllText($"{path}/{name}.h", header_file.ToString());
            File.WriteAllText($"{path}/{name}.cpp", cpp_file.ToString());
        }
    }
}
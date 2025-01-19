using System.Reflection.Emit;
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
                case i_generic_name ign:
                {
                    if (generic_remap.ContainsKey(ign.name))
                    {
                        return generic_remap[ign.name];
                    }

                    return ign.name;
                }

                case runtime_variable rv: return rv.size.ToString();
                case compile_time_type: return compile_time_operand_type;
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
                        case i_generic_name ign:
                        {
                            if (generic_remap.ContainsKey(ign.name))
                            {
                                return generic_remap[ign.name];
                            }

                            return ign.name;
                        }

                        case runtime_variable rvs: return $"u{rvs.size}_t";
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

        string get_operation(string name, bool runtime, out bool is_signed, bool is_unary = false)
        {
            is_signed = false;

            if (runtime)
            {
                switch (name)
                {
                    case "+": return "ir_add"; 
                    case "-": 
                     if (is_unary)
                        return "ir_negate";
                    
                    return "ir_subtract";
                    case "&": return "ir_bitwise_and";
                    case "|": return "ir_bitwise_or";
                    case "clt": return "ir_compare_less_signed";
                    case "<": return "ir_compare_less_unsigned";
                    case "==": return "ir_compare_equal";
                    case "^": return "ir_bitwise_exclusive_or";
                    case "~": return "ir_bitwise_not";
                    case ">=": return "ir_compare_greater_equal_unsigned";
                    case "<<": return "ir_shift_left";
                    case ">>": return "ir_shift_right_unsigned";
                    case "ror": return "ir_rotate_right";
                    case "sar": return "ir_shift_right_signed";
                    case "sdiv": return "ir_divide_signed";
                    case "/": return "ir_divide_unsigned";
                    case "&&": return "ir_bitwise_and";
                    case "*": return "ir_multiply";
                    case "umulh": return "ir_multiply_hi_unsigned";
                    case "smulh": return "ir_multiply_hi_signed";
                    case "!": return "ir_logical_not";
                    case "!=": return "ir_compare_not_equal";
                    case ">": return "ir_compare_greater_unsigned";
                    case "cgt": return "ir_compare_greater_signed";
                    case "cgte": return "ir_compare_greater_equal_signed";
                    case "fadd": return "ir_floating_point_add"; 
                    case "fsub": return "ir_floating_point_subtract"; 
                    case "fmul": return "ir_floating_point_multiply"; 
                    case "fdiv": return "ir_floating_point_divide"; 
                    case "fmax": return "ir_floating_point_select_max";
                    case "fmin": return "ir_floating_point_select_min";
                    case "fclt": return "ir_floating_point_compare_less";
                    case "fclte": return "ir_floating_point_compare_less_equal";
                    case "feq": return "ir_floating_point_compare_equal";
                    case "fcgt": return "ir_floating_point_compare_greater";
                    case "fcgte": return "ir_floating_point_compare_greater_equal";
                    case "fsqrt": return "ir_floating_point_square_root";
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
                    case "sdiv": is_signed = true; return "/";

                    case "fadd": 
                    case "fsub":
                    case "fmul": 
                    case "fdiv": 
                    case "fmin":
                    case "fmax":
                    case "feq":
                    case "fneq":
                    case "fclt":
                    case "fcgte":
                    case "fcgt": 
                    case "fsqrt":
                    case "fclte":
                        return "undefined";

                    default:
                        return name;
                }
            }
        }

        string sign_compile_time(string source, variable_type to_type = null)
        {
            string result = $"sign_extend({source})";

            if (to_type != null)
            {
                result = $"({get_rem_type(to_type, true)}){result}";
            }

            return result;
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

            return $"copy_new_raw_size({get_default_argument(interpreted)}, {source}, {get_explicit_rem_type(type)})";
        }

        string bitwise_to_float(string source, variable_type type, bool interpreted)
        {
            return $"get_float<{get_rem_type(type, interpreted)}>({source})";
        }

        string generate_object(i_ast_object data, bool interpreted, bool is_command = false)
        {
            switch (data)
            {
                case loop lo:
                {
                    if (lo.loop_count.is_runtime())
                    {
                        throw new Exception();
                    }
                    else
                    {
                        return $"for ({generate_object(lo.loop_index, interpreted).Replace(";", " = 0;")} {lo.loop_index.variable_name} < {generate_object(lo.loop_count, interpreted)}; {lo.loop_index.variable_name}++)\n{generate_object(lo.body, interpreted)}";
                    }
                }; 

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
                        else if (!interpreted && rs.to_return.get_type() != rs.return_type)
                        {
                            value = convert_to_other_runtime(value, rs.return_type, interpreted);
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
                        else if (!interpreted && vd.is_runtime() && !variable_type.types_compatible(vd.default_value.get_type(),vd.type))
                        {
                            default_value = convert_to_other_runtime(default_value, vd.type, interpreted);
                        }

                        if (!interpreted && vd.force_non_constant)
                        {
                            default_value = $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)}, ir_move, {default_value})";
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

                        return $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)}, {get_operation(uo.operation, true, out _, true)}, {result})";
                    }
                    else
                    {
                        string raw_operation = get_operation(uo.operation, false, out bool is_signed);

                        if (raw_operation == "undefined")
                        {
                            return "undefined_value()";
                        }

                        if (is_signed)
                        {
                            raw_operation = sign_compile_time(raw_operation);
                        }

                        return $"{raw_operation}{result}";
                    }
                }; 

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

                        left = $"({get_rem_type(bo.get_type(), interpreted)}){left}";
                        right = $"({get_rem_type(bo.get_type(), interpreted)}){right}";
                        
                        if (is_signed)
                        {
                            left = sign_compile_time(left);
                            right = sign_compile_time(right);
                        }

                        string result;

                        if (raw_operation == "ror")
                        {
                            result = $"rotate_right({left},{right})";
                        }
                        else if (raw_operation == "smulh")
                        {
                            result = $"multiply_hi({left},{right}, true)";
                        }
                        else if (raw_operation == "umulh")
                        {
                            result = $"multiply_hi({left},{right}, false)";
                        }
                        else  if (raw_operation == "undefined")
                        {
                            result = "undefined_value()";
                        }
                        else
                        {
                            result = $"{left} {raw_operation} {right}";
                        }

                        if (is_signed)
                        {
                            result = $"({get_rem_type(bo.get_type(), interpreted)})({result})";
                        }

                        while (true)
                        {
                            int length = result.Length;

                            string to_replace = $"({get_rem_type(bo.get_type(), interpreted)})";

                            result = result.Replace($"{to_replace}{to_replace}", to_replace);

                            if (length == result.Length)
                                break;
                        }

                        return $"({result})";
                    }
                };

                case if_statment ifs:
                {
                    if (!interpreted && ifs.condition.is_runtime())
                    {
                        cf_runtime.Push(true);

                        string result = "{";

                        string context = $"{get_default_argument(interpreted)}->ir";

                        result += @$"
    ir_operand end = ir_operation_block::create_label({context});
    ir_operand yes = ir_operation_block::create_label({context});

    ir_operand condition = {generate_object(ifs.condition,interpreted)};

    ir_operation_block::jump_if({context},yes, condition);
{(ifs.no != null ? string_tools.tab_string(generate_object(ifs.no, interpreted, true)) : "\t/* TODO if statements without a no should not have this*/")}
    
    ir_operation_block::jump({context},end);
    ir_operation_block::mark_label({context}, yes);

{string_tools.tab_string(generate_object(ifs.yes, interpreted, true))}

    ir_operation_block::mark_label({context}, end);
";

                        cf_runtime.Pop();

                        return result + "}";
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

                    if (fc.is_reference_call)
                    {
                        string cast = $"({get_rem_type(fc.return_type, interpreted)}(*)(void*";

                        foreach (var i in fc.parameter_types)
                        {
                            cast += $",{get_rem_type(i, interpreted)}";
                        }

                        cast += "))";

                        result = $"({cast}{fc.function_name})";
                    }

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

                        if (!a.is_runtime() && fc.parameter_types[argument_index].is_runtime() && !interpreted)
                        {
                            raw_argument = convert_to_runtime(raw_argument, fc.parameter_types[argument_index], interpreted);
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
                    if (!lvs.l_value.is_runtime() && lvs.r_value.is_runtime())
                    {
                        throw new Exception();
                    }

                    string value = generate_object(lvs.r_value, interpreted);

                    if (!interpreted && lvs.to_runtime_conversion_needed())
                    {
                        value = convert_to_runtime(value, lvs.l_value.get_type(),interpreted);
                    }
                    else if (!interpreted && lvs.l_value.is_runtime() && !variable_type.types_compatible(lvs.l_value.get_type(),lvs.r_value.get_type()))
                    {
                        value = convert_to_other_runtime(value, lvs.l_value.get_type(), interpreted);
                    }

                    if (!interpreted && lvs.force_runtime)
                    {   
                        return $"ssa_emit_context::move({get_default_argument(interpreted)},{generate_object(lvs.l_value,interpreted)},{value});";
                    }

                    return $"{generate_object(lvs.l_value,interpreted)} = {value};";
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

                        result += "0;\n";

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

                case sign_extend se:
                {
                    if (interpreted || !se.is_runtime())
                    {
                        string result = generate_object(se.value, interpreted);

                        return sign_compile_time(result, se.new_type);
                    }
                    else if (!interpreted && se.is_runtime())
                    {
                        return $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)},ir_sign_extend,{generate_object(se.value, interpreted)}, {get_explicit_rem_type(se.new_type)})";
                    }
                    else
                    {
                        throw new Exception();
                    }
                };

                case physical_read pr:
                {
                    if (interpreted)
                    {
                        return $"*({get_rem_type(pr.get_type(), interpreted)}*){generate_object(pr.address, interpreted)}";
                    }
                    else
                    {
                        return $"ssa_emit_context::emit_ssa({get_default_argument(interpreted)}, ir_load, {generate_object(pr.address, interpreted)}, {get_explicit_rem_type(pr.get_type())})";
                    }
                }

                case physical_write pw:
                {
                    if (interpreted)
                    {
                        return $"*({get_rem_type(pw.value.get_type(), interpreted)}*){generate_object(pw.address, interpreted)} = {generate_object(pw.value, interpreted)};";
                    } 
                    else
                    {
                        return $"ssa_emit_context::store({get_default_argument(interpreted)}, {generate_object(pw.address, interpreted)}, {generate_object(pw.value, interpreted)});";
                    }
                }

                case element_insert ei:
                {
                    if (interpreted)
                    {
                        return $"uint128_t::insert(&{generate_object(ei.source, interpreted)}, {generate_object(ei.index, interpreted)}, {generate_object(ei.size, interpreted)}, {generate_object(ei.value, interpreted)});";
                    }
                    else
                    {
                        return $"ssa_emit_context::vector_insert({get_default_argument(interpreted)},{generate_object(ei.source, interpreted)}, {generate_object(ei.index, interpreted)}, {generate_object(ei.size, interpreted)}, {generate_object(ei.value, interpreted)});";
                    }
                }; 

                case element_extract ee:
                {
                    if (interpreted)
                    {
                        return $"uint128_t::extract({generate_object(ee.source, interpreted)}, {generate_object(ee.index, interpreted)}, {generate_object(ee.size, interpreted)})";
                    }
                    else
                    {
                        return $"ssa_emit_context::vector_extract({get_default_argument(interpreted)},{generate_object(ee.source, interpreted)}, {generate_object(ee.index, interpreted)}, {generate_object(ee.size, interpreted)})";
                    }
                }

                case vector_default vd:
                {
                    if (interpreted)
                    {
                        switch (vd.default_type)
                        {
                            case vector_default_type.zeros: return "0";
                            case vector_default_type.ones: return "uint128_t(-1, -1)";
                            default: throw new Exception();
                        }
                    }
                    else
                    {
                        switch (vd.default_type)
                        {
                            case vector_default_type.zeros: return $"ssa_emit_context::vector_zero({get_default_argument(interpreted)})";
                            case vector_default_type.ones: return $"ssa_emit_context::vector_one({get_default_argument(interpreted)})";
                            default: throw new Exception();
                        }
                    }
                }; 

                case identifier id:
                {
                    if (id.is_external)
                    {
                        return $"(uint64_t){id.data}";
                    }
                    else if (id.is_internal)
                    {
                        string result = $"(uint64_t){id.data}";

                        if (interpreted)
                        {
                            result += "_interpreter";
                        }
                        else
                        {
                            result += "_jit";
                        }

                        return result;
                    }

                    throw new Exception();
                }

                case floating_point_conversion fpc:
                {
                    if (interpreted)
                    {
                        return $"convert_to_float<{get_rem_type(fpc.new_type,interpreted)}, {get_rem_type(fpc.source.get_type(), interpreted)}>({generate_object(fpc.source, interpreted)}, {(fpc.is_signed ? "1" : "0")})";
                    }
                    else
                    {
                        if (!fpc.is_runtime())
                        {
                            throw new Exception();
                        }

                        if (fpc.to_float)
                        {
                            return $"ssa_emit_context::convert_to_float({get_default_argument(interpreted)},{generate_object(fpc.source, interpreted)},{get_explicit_rem_type(fpc.new_type)},{get_explicit_rem_type(fpc.source.get_type())}, {(fpc.is_signed ? "1" : "0")})";
                        }
                        else
                        {
                            return $"ssa_emit_context::convert_to_integer({get_default_argument(interpreted)},{generate_object(fpc.source, interpreted)},{get_explicit_rem_type(fpc.new_type)},{get_explicit_rem_type(fpc.source.get_type())}, {(fpc.is_signed ? "1" : "0")})";
                        }
                    }
                }; 

                default: throw new Exception(data.GetType().ToString());
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
#include ""emulator/guest_process.h""
#include ""aarch64_soft_float.h""
#include ""emulator/atomic.h""
#include ""emulator/uint128_t.h""
#include ""fallbacks.h""

struct interpreter_data
{
    guest_process*      process_context;
    void*               register_data;
    uint64_t            current_pc;
    int                 branch_type;
    uint32_t            current_instruction;
};

void init_aarch64_decoder(guest_process* process);

enum sys_registers
{
    nzcv_n,
    nzcv_z,
    nzcv_c,
    nzcv_v,
    fpcr,
    fpsr,
    exclusive_value,
    exclusive_address,
    thread_local_0,
    thread_local_1
};

template <typename D, typename S>
uint64_t convert_to_float(uint64_t source, bool is_signed)
{
    int des_size = sizeof(D) * 8;
    int src_size = sizeof(S) * 8;

    double temp;

    if (is_signed)
    {
        if (src_size == 32)
        {
            temp = (int32_t)source;
        }
        else
        {
            temp = (int64_t)source;
        }
    }
    else
    {
        temp = (S)source;
    }

    if (des_size == 32)
    {
        float temp_32 = temp;

        return *(uint32_t*)&temp_32;
    }

    return *(uint64_t*)&temp;
}

template <typename T>
double get_float(uint64_t source)
{
	switch (sizeof(T))
	{
		case 4: return convert<float, uint32_t>(source);
		case 8: return convert<double, uint64_t>(source);
		default: throw_error();
	}
}

template <typename T>
uint64_t get_int(double source)
{
    switch (sizeof(T))
    {
        case 4: return convert<uint32_t, float>(source);
        case 8: return convert<uint64_t, double>(source);
    }
}

static uint64_t undefined_value()
{
    throw_error();
}

");
            cpp_file = new StringBuilder(@"#include ""aarch64_impl.h""
#include ""string.h""
#include ""tools/big_number.h""

static void append_table(guest_process* process, std::string encoding, void* emit, void* interperate, void* decoder_helper,std::string name)
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

	fixed_length_decoder<uint32_t>::insert_entry(&process->decoder, instruction, mask, emit, interperate, decoder_helper, name);
}

template <typename T>
int64_t sign_extend(T src) 
{
    switch (sizeof(T))
    {
        case 1: return (int8_t)src;
        case 2: return (int16_t)src;
        case 4: return (int32_t)src;
    }

    return src;
}

template <typename T>
T rotate_right(T src, int ammount)
{
	int INT_BITS = sizeof(T) * 8;

	return (src >> ammount)|(src << (INT_BITS - ammount));
}

static ir_operand copy_new_raw_size(ssa_emit_context* ctx, ir_operand source, uint64_t new_size)
{
	bool div = new_size >= int128;
	bool siv = ir_operand::is_vector(&source);

	if (div == siv)
	{
		if (new_size >= ir_operand::get_raw_size(&source))
		{
			source = ir_operand::copy_new_raw_size(source, new_size);

			return source;
		} 
		else
		{
			source = ir_operand::copy_new_raw_size(source, new_size);

			return ssa_emit_context::emit_ssa(ctx, ir_move, source);
		}
	}
	else if (div && !siv)
	{	
		ir_operand result = ssa_emit_context::emit_ssa(ctx, ir_vector_zero, int128);

		ir_operation_block::emitds(ctx->ir, ir_vector_insert, result, result, source, ir_operand::create_con(0), ir_operand::create_con(8 << ir_operand::get_raw_size(&source)));

		return result;
	}
	else
	{
		ir_operand result = ssa_emit_context::create_local(ctx, new_size);
		
		ir_operation_block::emitds(ctx->ir, ir_vector_extract, result, source, ir_operand::create_con(0), ir_operand::create_con(8 << new_size));

		return result;
	}
}

template <typename R>
R intrinsic_unary_interpreter(interpreter_data* ctx, uint64_t instruction, R source)
{

}

template <typename R>
R intrinsic_binary_interpreter(interpreter_data* ctx, uint64_t instruction, R source_0, R source_1)
{

}

template <typename R>
R intrinsic_binary_imm_interpreter(interpreter_data* ctx, uint64_t instruction, R source_0, uint64_t source_1)
{

}

template <typename R>
R intrinsic_ternary_interpreter(interpreter_data* ctx, uint64_t instruction, R source_0, R source_1, R source_2)
{

}

template <typename R>
R intrinsic_ternary_imm_interpreter(interpreter_data* ctx, uint64_t instruction, R source_0, R source_1, uint64_t source_2)
{

}

");

            string table_create_function = $"void init_aarch64_decoder(guest_process* process)\n{{\n";

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

                bool needs_help = false;

                foreach (instruction_operand operand in f.fixed_length_operand_data)
                {
                    if (operand.extra_rule == null)
                        continue;

                    needs_help = true;
                }

                table_create_function += $"\tappend_table(process, \"{instruction}\", (void*)emit_{get_name_with_interpreted(f.function_name, false)}, (void*)call_{get_name_with_interpreted(f.function_name, true)},{(needs_help ? $"(void*)help_decode_{f.function_name}" : "nullptr")}, \"{f.function_name}\");\n";

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

                if (needs_help)
                {
                    cpp_file.Append($"static bool help_decode_{f.function_name}(uint32_t instruction)\n{{\n");

                    foreach (instruction_operand operand in f.fixed_length_operand_data)
                    {
                        if (operand.extra_rule == null)
                            continue;

                        switch (operand.extra_rule)
                        {
                            case "!=":
                            case "==":
                            {
                                cpp_file.Append($"\tif (((instruction >> {offsets[operand.data]}) & {(1 << operand.size) - 1}) == {operand.extra_number}) return false;\n");
                            }; break;

                            default: throw new Exception();
                        }
                    }

                    cpp_file.Append("\treturn true;\n}\n\n");
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
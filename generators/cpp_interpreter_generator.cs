using System.Text;
using rem_frontend_generator.language;

namespace rem_frontend_generator.generators
{
    public class cpp_interpreter_generator
    {
        public StringBuilder    header_file                         { get; set; }
        public StringBuilder    cpp_file                            { get; set; }
        
        Dictionary<generic_declaration, string> working_generics    { get; set; }

        const string context_declaration = "void* ctx";
        const string context_reference = "ctx";

        string generate_type(variable_type source)
        {
            switch (source)
            {
                case generic_runtime_variable_type grt: return grt.name;
                case runtime_variable rv:               return $"u{rv.size}_t";
                case generic_declaration gd:            return working_generics[gd];
                case compile_time_type ctt:
                {
                    if (ctt.is_void)
                        return "void";

                    return $"uint64_t";
                };
                default: throw new Exception();
            }
        }

        string generate_function_call(function_call fc)
        {
            string result = fc.function_name;

            if (fc.generics.Count != 0)
            {
                result += "<";

                foreach (variable_type v in fc.generics)
                {
                    result += generate_type(v);

                    if (v != fc.generics.Last())
                    {
                        result += ",";
                    }
                }

                result += ">";
            }

            result += $"({context_reference}";

            foreach (expression e in fc.function_arguments)
            {
                result += $", {generate_ast(e)}";
            }

            return result + ")";
        }

        string generate_variable_declaration(variable_declaration vd)
        {
            string type = generate_type(vd.type);
            string name = vd.variable_name;

            return $"{type} {name}{(vd.default_value == null ? "" : $" = {generate_ast(vd.default_value)}")};";
        }

        string generate_if_statement(if_statment statment)
        {
            string result = $"if ({generate_ast(statment.condition)})\n{generate_ast(statment.yes)}";

            if (statment.no != null)
            {
                result += $"\nelse \n{generate_ast(statment.no)}";
            }

            return result;
        }

        string get_object_name(i_ast_object source)
        {
            switch (source)
            {
                case variable_declaration vd: return vd.variable_name;
                default: throw new Exception();
            }
        }

        string translate_operaiton(string source, out bool signed)
        {
            signed = true;

            switch (source)
            {
                case "clt": return "<";
                case "cgt": return ">";
                case "clte": return "<=";
                case "cgte": return ">=";
                case "sdiv": return "/";
                case "sar": return ">>";

                default:
                {
                    signed = false;

                    return source;
                };
            }
        }

        string generate_binary_operation(binary_operation bo)
        {
            string operation = translate_operaiton(bo.operation, out bool signed);

            string l = $"{(signed ? "(int64_t)" : "")}{generate_ast(bo.left)}";
            string r = $"{(signed ? "(int64_t)" : "")}{generate_ast(bo.right)}";

            return $"{l} {operation} {r}";
        }

        string generate_return_statement(return_statement rs)
        {
            if (rs.to_return == null)
            {
                return "return;";
            }
            else
            {
                return $"return {generate_ast(rs.to_return)};";
            }
        }

        string generate_generic_declaration(generic_declaration gd)
        {
            if (!working_generics.ContainsKey(gd))
            {
                working_generics.Add(gd, null);
            }

            runtime_type_switch sizes = gd.new_type;

            string result = "";

            foreach (generic_declaration_case condition in sizes.conditions)
            {
                working_generics[gd] = generate_type(condition.type);
                
                result += $"if ({generate_ast(sizes.test)} == {generate_ast(condition.condition)})\n{generate_ast(gd.body)}";

                if (condition != sizes.conditions.Last())
                    result += "\n";
            }
            
            return result;
        }

        string generate_l_value_set(l_value_set set)
        {   
            return $"{generate_ast(set.l_value)} = {generate_ast(set.r_value)};";
        }   

        string generate_ast(i_ast_object to_generate)
        {
            switch (to_generate)
            {
                case scope s: return generate_scope(s);
                case variable_declaration vd: return generate_variable_declaration(vd);
                case if_statment i_statement: return generate_if_statement(i_statement);
                case object_reference or: return get_object_name(or.reference);
                case number n: return n.value.ToString() + "ULL";
                case binary_operation bo: return generate_binary_operation(bo);
                case return_statement rs: return generate_return_statement(rs);
                case generic_declaration gd: return generate_generic_declaration(gd);
                case function_call fc: return generate_function_call(fc);
                case l_value_set lvs: return generate_l_value_set(lvs);

                default: return $"/*{to_generate.GetType()}*/";
            }
        }

        string generate_scope(scope source)
        {
            string result = "{\n";

            foreach (i_ast_object line in source.commands)
            {
                result += string_tools.tab_string(generate_ast(line)) ;

                if (line is function_call)
                {
                    result += ";";
                }

                result += "\n";
            }

            return result + "}";
        }

        string generate_function_header(function f, bool input_template, bool create_body)
        {
            string result = $"{generate_type(f.return_type)} {f.function_name}({context_declaration}";

            foreach (variable_declaration parameter in f.parameters)
            {
                result += $", {generate_type(parameter.type)} {parameter.variable_name}";
            }

            result += ")";

            if (f.generics.Count != 0 && input_template)
            {
                string template_creation = "template <";

                foreach (generic_runtime_variable_type generic in f.generics)
                {
                    template_creation += $"typename {generic.name}";

                    if (generic != f.generics.Last())
                    {
                        template_creation += ",";
                    }
                }

                template_creation += ">";

                result = $"{template_creation}\n{result}";
            }

            if (create_body)
            {   
                result = $"{result}\n{generate_scope(f.function_body)}";
            }

            return result;
        }

        public void generate(source_file source)
        {
            header_file = new StringBuilder();
            cpp_file = new StringBuilder();

            working_generics = new Dictionary<generic_declaration, string>();

            header_file.AppendLine("#include <inttypes.h>");
            cpp_file.AppendLine("#include \"interpreter.h\"");

            foreach (function f in source.functions.Values)
            {
                if (f.is_external)
                {
                    header_file.AppendLine(generate_function_header(f, true, false) + ";");
                }
                else
                {
                    cpp_file.AppendLine(generate_function_header(f, true, true) + "\n");
                }
            }
        }
    }
}
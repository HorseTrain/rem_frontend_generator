using System.Text;
using rem_frontend_generator.language;

namespace rem_frontend_generator.generators
{
    public class rem_jit_generator 
    {   
        public StringBuilder    header_file { get; set; }
        public StringBuilder    cpp_file    { get; set; }

        const string context_declaration = "arm_emit_context* ctx";
        const string context_reference = "ctx";

        static string generate_type(variable_type source)
        {
            switch (source)
            {
                case generic_runtime_variable_type:
                case runtime_variable:
                    return $"ir_operand";

                case compile_time_type ctt:
                {
                    if (ctt.is_void)
                        return "void";

                    return $"uint64_t";
                };
                default: throw new Exception();
            }
        }

        static string generate_function_header(function f)
        {
            string result = $"{generate_type(f.return_type)} {f.function_name}({context_declaration}";

            foreach (variable_declaration parameter in f.parameters)
            {
                result += $", {generate_type(parameter.type)} {parameter.variable_name}";
            }

            result += ")";

            return result;
        }

        public void generate(source_file source)
        {
            header_file = new StringBuilder();
            cpp_file = new StringBuilder();

            header_file.AppendLine("#include \"arm_emit_context.h\"");
            cpp_file.AppendLine("#include \"jit.h\"");

            foreach (function f in source.functions.Values)
            {
                if (f.is_external)
                {
                    header_file.AppendLine(generate_function_header(f) + ";");
                }
                else
                {
                    cpp_file.AppendLine(generate_function_header(f) + ";");
                }
            }
        }
    }
}
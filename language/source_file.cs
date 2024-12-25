namespace rem_frontend_generator.language
{
    public class source_file : scope
    {
        public Dictionary<string, function>     functions   { get; set; }

        public static string get_function_key(function source)
        {
            string result = $"function {source.function_name} <";

            foreach (var generic in source.generics)
            {
                result += "G";

                if (generic != source.generics.Last())
                {
                    result += ",";
                }
            }

            result += $">";

            foreach (var parameter in source.parameters)
            {
                result += " A";
            }

            return result;
        }

        public static string get_function_key(function_call call)
        {
            string result = $"function {call.function_name} <";

            foreach (var generic in call.generics)
            {
                result += "G";

                if (generic != call.generics.Last())
                {
                    result += ",";
                }
            }

            result += $">";

            foreach (var argument in call.function_arguments)
            {
                result += " A";
            }

            return result;
        }

        public source_file() : base (null)
        {
            functions = new Dictionary<string, function>();
        }
    }
}
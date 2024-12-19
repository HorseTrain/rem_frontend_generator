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
                result += generic.name;

                if (generic != source.generics.Last())
                {
                    result += ",";
                }
            }

            result += $"> ";

            foreach (var parameter in source.parameters)
            {
                result += parameter.type.get_type_key() + " ";
            }

            return result;
        }

        public source_file() : base (null)
        {
            functions = new Dictionary<string, function>();
        }
    }
}
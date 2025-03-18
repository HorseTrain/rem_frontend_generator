namespace rem_frontend_generator.language
{
    public static class string_tools
    {
        public static string tab_string(string source)
        {
            string result = "";

            string[] lines = source.Split('\n');
            
            for (int i = 0; i < lines.Length; ++i)
            {
                result += $"\t{lines[i]}";

                if (i != lines.Length - 1)
                    result += "\n";
            }

            return result;
        }
    }
}
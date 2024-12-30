namespace rem_frontend_generator.language
{
    public class variable_declaration : i_ast_object
    {
        public string           variable_name       { get; set; }
        public variable_type    type                { get; set; }
        public expression       default_value       { get; set; }
        public bool             is_parameter        { get; set; }
        public bool             force_non_constant  { get; set; }

        public variable_declaration(variable_type type, string variable_name, bool is_parameter)
        {
            this.type = type;
            this.variable_name = variable_name;
            this.is_parameter = is_parameter;
        }

        public bool is_runtime()
        {
            bool result = type.is_runtime();

            if (!result && default_value.is_runtime())
            {
                throw new Exception();
            }

            return result;
        }

        public bool to_runtime_conversion_needed()
        {
            return type.is_runtime() && !default_value.is_runtime();
        }
    }
}
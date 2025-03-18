namespace rem_frontend_generator.language
{
    public class generic_runtime_variable_type : variable_type, i_generic_name
    {
        public string   name    { get; set; }

        public generic_runtime_variable_type(string name) 
        {
            this.name = name;
        }

        public override string get_type_key()
        {
            return $"(generic: {name})";
        }
    }
}
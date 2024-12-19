namespace rem_frontend_generator.language
{
    public class generic_declaration_case : variable_type
    {
        public expression       condition   { get; set; }
        public variable_type    type        { get; set; }
    }

    public class runtime_type_switch : i_ast_object
    {
        public expression                       test        { get; set; }
        public List<generic_declaration_case>   conditions  { get; set; }

        public runtime_type_switch()
        {
            conditions = new List<generic_declaration_case>();
        }
    }
}
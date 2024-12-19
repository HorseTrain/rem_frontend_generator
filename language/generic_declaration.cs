namespace rem_frontend_generator.language
{
    public class generic_declaration : variable_type
    {
        public string               name        { get; set; }
        public runtime_type_switch  new_type    { get; set; }
        public scope                body        { get; set; }
        
        public generic_declaration(scope parent_scope, string name, runtime_type_switch new_type, scope body)
        {
            this.name = name;
            this.new_type = new_type;
            this.body = body;

            parent_scope.add_object_to_scope(name, this);
        }
    }
}
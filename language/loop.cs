namespace rem_frontend_generator.language
{
    public class loop : scope
    {
        public  expression              loop_count      { get; set; }
        public  variable_declaration    loop_index      { get; set; }
        public  scope                   body            { get; set; }

        public loop(expression count, scope parent_scope) : base (parent_scope)
        {
            this.loop_count = count;
            this.body = parent_scope;
        }
        
    }
}
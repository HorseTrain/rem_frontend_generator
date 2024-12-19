namespace rem_frontend_generator.language
{
    public class object_reference : expression
    {
        public i_ast_object   reference   { get; set; }

        public object_reference(i_ast_object reference)
        {
            this.reference = reference;
        }
    }
}
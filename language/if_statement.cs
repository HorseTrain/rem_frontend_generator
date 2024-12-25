namespace rem_frontend_generator.language
{
    public class if_statment : i_ast_object
    {
        public expression   condition   { get; set; }
        public i_ast_object yes         { get; set; }
        public i_ast_object no          { get; set; }
    }
}
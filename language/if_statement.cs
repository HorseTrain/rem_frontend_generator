namespace rem_frontend_generator.language
{
    public class if_statment : i_ast_object
    {
        public i_ast_object condition   { get; set; }
        public i_ast_object yes         { get; set; }
        public i_ast_object no          { get; set; }
    }
}
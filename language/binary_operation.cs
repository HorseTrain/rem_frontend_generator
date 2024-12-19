namespace rem_frontend_generator.language
{
    public class binary_operation : expression
    {
        public string           operation   { get; set; }
        public i_ast_object     left        { get; set; }
        public i_ast_object     right       { get; set; }

        public binary_operation(i_ast_object left, i_ast_object right, string operation)
        {
            this.left = left;
            this.right = right;
            this.operation = operation;
        }
    }
}
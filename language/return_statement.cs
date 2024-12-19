namespace rem_frontend_generator.language
{
    public class return_statement : i_ast_object
    {
        public  expression  to_return   { get; set; }

        public return_statement(expression to_return)
        {
            this.to_return = to_return;
        }
    }
}
namespace rem_frontend_generator.language
{
    public class return_statement : i_ast_object
    {
        public variable_type    return_type     { get; set; }
        public  expression      to_return       { get; set; }

        public return_statement(expression to_return, variable_type return_type)
        {
            this.to_return = to_return;
            this.return_type = return_type;
        }

        public bool to_runtime_conversion_needed()
        {
            return return_type.is_runtime() && !to_return.is_runtime();
        }
    }
}
namespace rem_frontend_generator.language
{
    public class expression : i_ast_object
    {
        public virtual bool is_runtime()
        {
            throw new Exception(this.GetType().ToString());
        }

        public virtual variable_type get_type()
        {
            throw new Exception(GetType().ToString());
        }
    }
}
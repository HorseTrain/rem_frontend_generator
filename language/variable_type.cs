namespace rem_frontend_generator.language
{
    public class variable_type : i_ast_object
    {
        public virtual string get_type_key()
        {
            throw new Exception();
        }

        public bool is_runtime()
        {
            return this is not compile_time_type;
        }
    }
}
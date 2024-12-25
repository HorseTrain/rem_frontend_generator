namespace rem_frontend_generator.language
{
    public class l_value_set : i_ast_object
    {
        public  object_reference    l_value { get; set; }
        public  expression          r_value { get; set; }

        public bool to_runtime_conversion_needed()
        {
            return l_value.is_runtime() && !r_value.is_runtime();
        }
    }
}
namespace rem_frontend_generator.language
{
    public class compile_time_type : variable_type
    {
        public bool     is_void     { get; set; }
        
        public compile_time_type(bool is_void)
        {
            this.is_void = is_void;
        }

        public override string get_type_key()
        {
            return $"(compile_time: {is_void})";
        }
    }
}
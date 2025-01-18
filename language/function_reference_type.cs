namespace rem_frontend_generator.language
{
    public class function_reference_type : variable_type
    {
        public bool is_void                     { get; set; }
        public variable_type return_type        { get; set; }
        public List<variable_type> parameter_types    { get; set; }

        public override bool is_runtime()
        {
            return false;
        }
    }
}
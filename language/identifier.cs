namespace rem_frontend_generator.language
{
    public class identifier : expression
    {
        public bool is_external             { get; set; }
        public string data                  { get; set; }
        public variable_type external_type  { get; set; }

        public override variable_type get_type()
        {
            if (!is_external)
            {
                throw new Exception();
            }

            return external_type;
        }

        public override bool is_runtime()
        {
            return external_type.is_runtime();
        }
    }
}
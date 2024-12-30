namespace rem_frontend_generator.language
{
    public class physical_read : expression
    {
        public variable_type read_type  { get; set; }
        public expression address       { get; set; }

        public physical_read(variable_type type, expression address)
        {
            this.read_type = type;
            this.address = address;
        }

        public override variable_type get_type()
        {
            return read_type;
        }

        public override bool is_runtime()
        {
            return true;
        }
    }
}
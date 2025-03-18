namespace rem_frontend_generator.language
{
    public class floating_point_conversion : expression
    {
        public variable_type    new_type    { get; set; }
        public expression       source      { get; set; }
        public bool             to_float    { get; set; }
        public bool             is_signed   { get; set; }

        public floating_point_conversion(bool to_float, variable_type new_type, expression source)
        {
            this.new_type = new_type;
            this.source = source;
            this.to_float = to_float;

            if (!new_type.is_runtime())
            {
                throw new Exception();
            }
        }

        public override bool is_runtime()
        {
            return true;
        }

        public override variable_type get_type()
        {
            return new_type;
        }
    }
}
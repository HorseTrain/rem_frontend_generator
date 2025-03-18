namespace rem_frontend_generator.language
{
    public class cast : expression
    {
        public variable_type    new_type    { get; set; }
        public expression       value       { get; set; }

        public cast(variable_type new_type, expression value)
        {
            this.new_type = new_type;
            this.value = value;
        }

        public override variable_type get_type()
        {
            return new_type;
        }

        public override bool is_runtime()
        {
            return new_type.is_runtime();
        }
    }
}
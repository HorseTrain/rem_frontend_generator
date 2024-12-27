namespace rem_frontend_generator.language
{
    public class sign_extend : expression
    {
        public variable_type    new_type    { get; set; }
        public expression       value       { get; set; }

        public sign_extend(variable_type new_type, expression source)
        {
            this.new_type = new_type;
            this.value = source;
        }

        public override variable_type get_type()
        {
            return new_type;
        }

        public override bool is_runtime()
        {
            return value.is_runtime();
        }
    }
}
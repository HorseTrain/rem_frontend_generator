namespace rem_frontend_generator.language
{
    public class unary_operation : expression
    {
        public string       operation   { get; set; }
        public expression   value       { get; set; }

        public unary_operation(string operation, expression value)
        {
            this.operation = operation;
            this.value = value;
        }

        public override variable_type get_type()
        {
            return value.get_type();
        }

        public override bool is_runtime()
        {
            return value.is_runtime();
        }
    }
}
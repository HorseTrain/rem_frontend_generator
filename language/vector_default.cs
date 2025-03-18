namespace rem_frontend_generator.language
{
    public enum vector_default_type
    {
        ones,
        zeros,
    }

    public class vector_default : expression
    {
        public variable_type        v_type;
        public vector_default_type  default_type;

        public vector_default(variable_type v_type, vector_default_type type)
        {
            this.v_type = v_type;
            this.default_type = type;
        }

        public override bool is_runtime()
        {
            return true;
        }

        public override variable_type get_type()
        {
            return v_type;
        }
    }
}
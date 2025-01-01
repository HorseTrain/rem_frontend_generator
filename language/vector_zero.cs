namespace rem_frontend_generator.language
{
    public class vector_zero : expression
    {
        public variable_type v_type;

        public vector_zero(variable_type v_type)
        {
            this.v_type = v_type;
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
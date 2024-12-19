namespace rem_frontend_generator.language
{
    public enum runtime_variable_size
    {
        int8,
        int16,
        int32,
        int64,
        int128
    }

    public class runtime_variable : variable_type
    {
        public  runtime_variable_size   size    { get; set; }

        public runtime_variable(runtime_variable_size size)
        {
            this.size = size;
        }

        public override string get_type_key()
        {
            return $"(known_operand_size {size})";
        }
    }
}
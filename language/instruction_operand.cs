namespace rem_frontend_generator.language
{
    public class instruction_operand
    {
        public string   data        { get; set; }
        public int      size        { get; set; }
        public bool     is_encoding { get; set; }

        public string   extra_rule      { get; set; }
        public int      extra_number    { get; set; }
    }
}
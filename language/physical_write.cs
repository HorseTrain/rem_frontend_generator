namespace rem_frontend_generator.language
{
    public class physical_write : i_ast_object
    {
        public expression   address { get; set; }
        public expression   value   { get; set; }

        public physical_write(expression address, expression value)
        {
            this.address = address;
            this.value = value;
        }
    }
}
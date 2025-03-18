namespace rem_frontend_generator.language
{
    public class element_extract : expression
    {
        public expression   source  { get; set; }
        public expression   index   { get; set; }
        public expression   size    { get; set; }

        variable_type       d_type  { get; set; }

        public element_extract(expression source, expression index, expression size, variable_type result_type, variable_type o128)
        {
            if (index.is_runtime()) throw new Exception();
            if (size.is_runtime()) throw new Exception();
            if (source.get_type() != o128) throw new Exception();

            this.source = source;
            this.index = index;
            this.size = size;
            this.d_type = result_type;
        }

        public override variable_type get_type()
        {
            return d_type;
        }

        public override bool is_runtime()
        {
            return true;
        }
    }
}
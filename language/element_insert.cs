namespace rem_frontend_generator.language
{
    public class element_insert : i_ast_object
    {
        public expression   source  { get; set; }
        public expression   index   { get; set; }
        public expression   size    { get; set; }
        public expression   value   { get; set; }

        variable_type       d_type  { get; set; }

        public element_insert(expression source, expression index, expression size, expression value, variable_type o128)
        {
            if (index.is_runtime()) throw new Exception();
            if (size.is_runtime()) throw new Exception();
            if (source.get_type() != o128) throw new Exception();

            this.source = source;
            this.index = index;
            this.size = size;
            this.value = value;
        }
    }
}
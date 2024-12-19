namespace rem_frontend_generator.language
{
    public class function_call : expression
    {
        public  object_reference        function_reference  { get; set; }
        public  List<variable_type>     generics            { get; set; }
        public  List<expression>        function_arguments  { get; set; }
        public string                   function_name       { get; set; }

        public function_call()
        {
            generics = new List<variable_type>();
            function_arguments = new List<expression>();
        }
    }
}
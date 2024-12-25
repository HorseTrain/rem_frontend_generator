namespace rem_frontend_generator.language
{
    public class function_call : expression
    {
        public  function                function_reference  { get; set; }
        public  List<variable_type>     generics            { get; set; }
        public  List<expression>        function_arguments  { get; set; }
        public string                   function_name       { get; set; }

        public function_call()
        {
            generics = new List<variable_type>();
            function_arguments = new List<expression>();
        }

        public override bool is_runtime()
        {
            return function_reference.return_type.is_runtime();
        }

        public override variable_type get_type()
        {
            return function_reference.return_type;
        }
    }
}
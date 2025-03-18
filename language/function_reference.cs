namespace rem_frontend_generator.language
{
    public class function_reference : expression
    {
        public function function                        { get; set; }
        public function_reference_type  function_type   { get; set; }
        public string function_name                     { get; set; }

        public function_reference()
        {

        }

        public override variable_type get_type()
        {
            return function_type;
        }
    }
}
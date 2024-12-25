using Antlr4.Runtime.Tree;

namespace rem_frontend_generator.language
{
    public class function : scope
    {
        public variable_type                        return_type     { get; set; }
        public string                               function_name   { get; set; }
        public List<variable_declaration>           parameters      { get; set; }
        public List<generic_runtime_variable_type>  generics        { get; set; }
        public IParseTree                           function_source { get; set; }
        public scope                                function_body   { get; set; }
        public bool                                 is_external     { get; set; }
        public List<instruction_operand>            fixed_length_operand_data   { get; set; }

        public function(scope parent_scope) : base (parent_scope)
        {
            parameters = new List<variable_declaration>();
            generics = new List<generic_runtime_variable_type>();
            fixed_length_operand_data = new List<instruction_operand>();
        }
    }
}
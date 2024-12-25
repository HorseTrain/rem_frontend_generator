namespace rem_frontend_generator.language
{
    public class scope : i_ast_object
    {
        public scope                                parent_scope    { get; set; }
        public Dictionary<string, i_ast_object>     scoped_objects  { get; set; }
        public List<i_ast_object>                   commands        { get; set; }

        public scope(scope parent_scope) : base ()
        {
            scoped_objects = new Dictionary<string, i_ast_object>();
            this.parent_scope = parent_scope;
            this.commands = new List<i_ast_object>();
        }

        public i_ast_object get_scoped_object(string name)
        {
            if (scoped_objects.ContainsKey(name))
            {
                return scoped_objects[name];
            }

            if (parent_scope == null)
            {
                throw new Exception();
            }

            return parent_scope.get_scoped_object(name);
        }

        public bool object_exists(string name, out i_ast_object result)
        {
            result = get_scoped_object(name);

            return result != null;
        }

        public void add_object_to_scope(string name, i_ast_object object_to_add)
        {
            scoped_objects.Add(name, object_to_add);
        }

        public function get_working_function()
        {
            if (this is function f)
            {
                return f;
            }
            else if (parent_scope != null)
            {
                return parent_scope.get_working_function();
            }

            return null;
        }
    }
}
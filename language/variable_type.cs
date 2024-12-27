namespace rem_frontend_generator.language
{
    public class variable_type : i_ast_object
    {
        public virtual string get_type_key()
        {
            throw new Exception();
        }

        public bool is_runtime()
        {
            return this is not compile_time_type;
        }

        public static bool types_compatible(variable_type left, variable_type right)
        {
            if (left.GetType() != right.GetType())
            {
                return false;
            }
            else if (left is runtime_variable l && right is runtime_variable r)
            {
                return l.size == r.size;
            }

            return true;
        }
    }
}
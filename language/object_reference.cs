namespace rem_frontend_generator.language
{
    public class object_reference : expression
    {
        public i_ast_object   reference   { get; set; }

        public object_reference(i_ast_object reference)
        {
            this.reference = reference;
        }

        public override bool is_runtime()
        {
            switch(reference)
            {
                case variable_declaration vd: return vd.type.is_runtime();
            }

            throw new Exception();
        }

        public override variable_type get_type()
        {
            switch (reference)
            {
                case variable_declaration vd: return vd.type;
            }

            throw new Exception();
        }
    }
}
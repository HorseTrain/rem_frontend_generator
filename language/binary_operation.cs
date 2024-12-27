namespace rem_frontend_generator.language
{
    public class binary_operation : expression
    {
        public string           operation   { get; set; }
        public expression       left        { get; set; }
        public expression       right       { get; set; }

        public binary_operation(expression left, expression right, string operation)
        {
            this.left = left;
            this.right = right;
            this.operation = operation;
        }

        public override bool is_runtime()
        {
            return left.is_runtime() || right.is_runtime();
        }

        public override variable_type get_type()
        {
            if (left is number && right is not number)
            {
                return right.get_type();
            }
            else if (left is not number && right is number)
            {
                return left.get_type();
            }

            return left.get_type();
        }
    }
}
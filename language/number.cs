using System.Numerics;

namespace rem_frontend_generator.language
{
    public class number : expression
    {
        public  BigInteger  value       { get; set; }
        public  variable_type my_type   { get; set; }

        public number(BigInteger value, variable_type type)
        {
            this.value = value;
            this.my_type = type;
        }

        public override variable_type get_type()
        {
            return my_type;
        }

        public override bool is_runtime()
        {
            return false;
        }
    }
}
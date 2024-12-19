using System.Numerics;

namespace rem_frontend_generator.language
{
    public class number : expression
    {
        public  BigInteger  value   { get; set; }

        public number(BigInteger value)
        {
            this.value = value;
        }
    }
}
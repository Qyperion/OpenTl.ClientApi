namespace OpenTl.Common.Crypto
{
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Security;

    internal static class PollardRho
    {
        private static readonly BigInteger Zero = new BigInteger("0");

        private static readonly BigInteger One = new BigInteger("1");

        private static readonly BigInteger Two = new BigInteger("2");

        private static readonly SecureRandom Random = new SecureRandom();

        private static BigInteger Rho(BigInteger n)
        {
            BigInteger divisor;
            var c = new BigInteger(n.BitLength, Random);
            var x = new BigInteger(n.BitLength, Random);
            var xx = x;

            // check divisibility by 2
            if (n.Mod(Two).CompareTo(Zero) == 0) return Two;

            do
            {
                x = x.Multiply(x).Mod(n).Add(c).Mod(n);
                xx = xx.Multiply(xx).Mod(n).Add(c).Mod(n);
                xx = xx.Multiply(xx).Mod(n).Add(c).Mod(n);
                divisor = x.Subtract(xx).Gcd(n);
            }
            while ((divisor.CompareTo(One)) == 0);

            return divisor;
        }

        public static BigInteger Factor(BigInteger n)
        {
            if (n.CompareTo(One) == 0) return Zero;
            if (n.IsProbablePrime(20))
            {
                return n;
            }

            var divisor = Rho(n);
            
            var a = Factor(divisor);

            if (!Equals(a, Zero))
            {
                return a;
            }
            
            return Factor(n.Divide(divisor));
        }
    }
}
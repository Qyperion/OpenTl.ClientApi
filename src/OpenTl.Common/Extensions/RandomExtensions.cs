namespace OpenTl.Common.Extensions
{
    using System;

    public static class RandomExtensions
    {
        public static long NextLong(this Random random)
        {
            return (long) (uint) random.Next() << 32 | (uint) random.Next(1, int.MaxValue);
        }
    }
}
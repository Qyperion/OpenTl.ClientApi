namespace OpenTl.Common.GuardExtensions
{
    using System.Collections.Generic;

    using BarsGroup.CodeGuard.Exceptions;
    using BarsGroup.CodeGuard.Internals;

    public static class ListExtentions
    {
        public static ArgBase<List<T>> Contains<T>(this ArgBase<List<T>> args, T item)
        {
            if (args.Value.IndexOf(item) == -1)
            {
                throw new ArgumentException("Item didn't contains in the list");
            }

            return args;
        }
    }
}
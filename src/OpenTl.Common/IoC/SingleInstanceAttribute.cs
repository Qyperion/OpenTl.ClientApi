namespace OpenTl.Common.IoC
{
    using System;

    public sealed class SingleInstanceAttribute : ComponentAttribute
    {
        public SingleInstanceAttribute(params Type[] registerAs) : base(registerAs, EDependencyLifecycle.Singleton)
        {
        }
    }
}
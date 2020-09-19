namespace OpenTl.Common.IoC
{
    using System;

    public sealed class TransientInstanceAttribute : ComponentAttribute
    {
        public TransientInstanceAttribute(params Type[] registerAs) : base(registerAs, EDependencyLifecycle.Transient)
        {
        }
    }
}
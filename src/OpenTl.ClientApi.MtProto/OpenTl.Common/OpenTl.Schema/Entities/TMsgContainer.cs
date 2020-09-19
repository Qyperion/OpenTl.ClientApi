namespace OpenTl.Schema
{
    using Serialization.Attributes;

    [Serialize(0x73f1f8dc)]
    public sealed class MsgContainer: IObject
    {
        [SerializationOrder(0)]
        public TContainerMessage[] Messages { get; set; }
    }
}
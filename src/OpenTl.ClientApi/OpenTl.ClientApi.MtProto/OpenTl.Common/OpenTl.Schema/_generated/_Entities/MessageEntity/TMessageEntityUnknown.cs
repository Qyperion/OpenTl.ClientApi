﻿// ReSharper disable All

namespace OpenTl.Schema
{
	using System;
	using System.Collections;
	using System.Text;

	using OpenTl.Schema;
	using OpenTl.Schema.Serialization.Attributes;	

	[Serialize(0xbb92ba95)]
	public sealed class TMessageEntityUnknown : IMessageEntity
	{
       [SerializationOrder(0)]
       public int Offset {get; set;}

       [SerializationOrder(1)]
       public int Length {get; set;}

	}
}
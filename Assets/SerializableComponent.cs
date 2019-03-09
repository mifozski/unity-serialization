using System;
using System.Reflection;
using System.Runtime.Serialization;

using UnityEngine;

namespace Serialization
{
    [System.Serializable]
	class SerializableComponent : ISerializable
	{
		private Component component;

		[NonSerialized] public SerializationInfo DeserializeInfo;
		[System.NonSerialized()] public StreamingContext DeserializeContext;

		public SerializableComponent(Component _component)
		{
			component = _component;
		}

		public SerializableComponent(SerializationInfo info, StreamingContext context)
		{
			this.DeserializeInfo = info;
			this.DeserializeContext = context;
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Type tp = component.GetType();

			if (component is Rigidbody)
			{
				AddRigidbody(ref info, component as Rigidbody);
			}

			var wqwe = tp.GetFields();

			var we = 5;
		}

		private void AddRigidbody(ref SerializationInfo info, Rigidbody rb)
		{
			info.AddValue("velocity", rb.velocity);
		}
	}
}
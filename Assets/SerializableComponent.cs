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
		}

		private void AddRigidbody(ref SerializationInfo info, Rigidbody rb)
		{
			info.AddValue("velocity", rb.velocity);
		}
	}

	static public class ComponentDeserializationUtility
	{
		static public void DeserializeComponent(ref Component component, SerializationInfo info)
		{
			if (component is Rigidbody)
			{
				Rigidbody rb = component as Rigidbody;
				DeserializeRigidbody(ref rb, info);
			}
		}

		static private void DeserializeRigidbody(ref Rigidbody rb, SerializationInfo info)
		{
			rb.velocity = (Vector3)info.GetValue("velocity", typeof(Vector3));
		}
	}
}
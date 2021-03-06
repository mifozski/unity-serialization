using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using UnityEngine;

namespace Serialization
{
	[System.Serializable]
	public class SerializableComponent : ISerializable
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
			ComponentSerializationUtility.SerializableComponent(component, ref info);
		}
	}

	static public class ComponentSerializationUtility
	{
		#region "Serialization helpers"

		static public void SerializableComponent(Component component, ref SerializationInfo info)
		{
			if (component is Rigidbody)
			{
				SerializeRigidbody(ref info, component as Rigidbody);
			}
			else if (component is Animator)
			{
				SerializeAnimator(ref info, component as Animator);
			}
			else if (component is Light)
			{
				SerializeLight(ref info, component as Light);
			}
			// Custom Component...
			else
			{
				var members = component.GetType().GetFields(
							BindingFlags.Public |
							BindingFlags.NonPublic |
							BindingFlags.Static |
							BindingFlags.Instance |
							BindingFlags.DeclaredOnly);
				foreach (FieldInfo field in members)
				{
					var value = field.GetValue(component);
					info.AddValue(field.Name, value);
				}
			}
		}

		static private void SerializeRigidbody(ref SerializationInfo info, Rigidbody rb)
		{
			info.AddValue("velocity", rb.velocity);
			info.AddValue("angularVelocity", rb.angularVelocity);
		}

		[Serializable]
		private struct CurrentStateInfo
		{
			public int fullHashPath;
			public float normalizedTime;
		};

		private static void SerializeAnimator(ref SerializationInfo info, Animator animator)
		{
			info.AddValue("enabled", animator.enabled);

			if (animator.runtimeAnimatorController == null)
			{
				return;
			}

			// Parameters
			animator.enabled = (bool)info.GetValue("enabled", typeof(bool));
			string [] serializedParameterNames = new string[animator.parameterCount];
			int [] serializedParameterTypes = new int[animator.parameterCount];
			object [] serializedParameterValues = new object[animator.parameterCount];
			int i = 0;
			foreach (AnimatorControllerParameter parameter in animator.parameters)
			{
				serializedParameterNames[i] = parameter.name;
				serializedParameterTypes[i] = (int)parameter.type;

				if (parameter.type == AnimatorControllerParameterType.Float)
					serializedParameterValues[i] = animator.GetFloat(parameter.name);
				else if (parameter.type == AnimatorControllerParameterType.Int)
					serializedParameterValues[i] = animator.GetInteger(parameter.name);
				else if (parameter.type == AnimatorControllerParameterType.Bool || parameter.type == AnimatorControllerParameterType.Trigger)
					serializedParameterValues[i] = animator.GetBool(parameter.name);

				i++;
			}
			info.AddValue("parameterNames", serializedParameterNames);
			info.AddValue("parameterTypes", serializedParameterTypes);
			info.AddValue("parameterValues", serializedParameterValues);

			// Current states
			CurrentStateInfo [] currentStates = new CurrentStateInfo[animator.layerCount];
			for (i = 0; i < animator.layerCount; i++)
			{
				currentStates[i] = new CurrentStateInfo
				{
					fullHashPath = animator.GetCurrentAnimatorStateInfo(i).fullPathHash,
					normalizedTime = animator.GetCurrentAnimatorStateInfo(i).normalizedTime
				};
			}
			info.AddValue("currentStates", currentStates);
		}

		private static void SerializeLight(ref SerializationInfo info, Light light)
		{
			info.AddValue("enabled", light.enabled);
			info.AddValue("range", light.range);
			info.AddValue("color", light.color, typeof(UnityEngine.Color));
			info.AddValue("intensity", light.intensity);
			info.AddValue("bounceIntensity", light.bounceIntensity);
		}

		#endregion

		#region "Deserialization helpers"

		static public void DeserializeComponent(ref Component component, SerializationInfo info)
		{
			if (component is Rigidbody)
			{
				Rigidbody rb = component as Rigidbody;
				DeserializeRigidbody(ref rb, info);
			}
			else if (component is Animator)
			{
				Animator animator = component as Animator;
				DeserializeAnimator(ref animator, info);
			}
			else if (component is Light)
			{
				Light light = component as Light;
				DeserializeLight(ref light, info);
			}
			else
			{
				var members = component.GetType().GetFields(
							BindingFlags.Public |
							BindingFlags.NonPublic |
							BindingFlags.Static |
							BindingFlags.Instance |
							BindingFlags.DeclaredOnly);
				SerializationInfoEnumerator e = info.GetEnumerator();
				while (e.MoveNext())
				{
					var field = members.Where(f => f.Name == e.Name).FirstOrDefault();
					if (field != null)
					{
						field.SetValue(component, Convert.ChangeType(e.Value, field.FieldType));
					}
				}
			}
		}

		static private void DeserializeRigidbody(ref Rigidbody rb, SerializationInfo info)
		{
			rb.velocity = (Vector3)info.GetValue("velocity", typeof(Vector3));
			rb.angularVelocity = (Vector3)info.GetValue("angularVelocity", typeof(Vector3));
		}

		private static void DeserializeAnimator(ref Animator animator, SerializationInfo info)
		{
			animator.enabled = (bool)info.GetValue("enabled", typeof(bool));

			if (animator.runtimeAnimatorController == null)
			{
				return;
			}

			// Parameters
			string [] serializedParameterNames = (string[])info.GetValue("parameterNames", (typeof(string[])));
			AnimatorControllerParameterType [] serializedParameterTypes =
				((int[])info.GetValue("parameterTypes", (typeof(int[]))))
					.Select(type => (AnimatorControllerParameterType)type).ToArray();
			object [] serializedParameterValues = (object[])info.GetValue("parameterValues", (typeof(object[])));
			for (int i = 0; i < serializedParameterNames.Count(); i++)
			{
				string name = serializedParameterNames[i];
				AnimatorControllerParameterType type = serializedParameterTypes[i];
				object value = serializedParameterValues[i];

				if (type == AnimatorControllerParameterType.Float)
					animator.SetFloat(name, Convert.ToSingle(value));
				else if (type == AnimatorControllerParameterType.Int)
					animator.SetInteger(name, Convert.ToInt32(value));
				else if (type == AnimatorControllerParameterType.Bool)
					animator.SetBool(name, (bool)value);
				else if (type == AnimatorControllerParameterType.Trigger)
				{
					if ((bool)value == true)
						animator.SetTrigger(name);
					else
						animator.ResetTrigger(name);
				}
			}

			// Current states
			CurrentStateInfo [] currentStates = (CurrentStateInfo[])info.GetValue("currentStates", typeof(CurrentStateInfo[]));
			for (int i = 0; i < animator.layerCount; i++)
			{
				int fullHashPath = currentStates[i].fullHashPath;
				float normalizedTime = currentStates[i].normalizedTime;
				animator.Play(fullHashPath, i, normalizedTime);
			}
		}

		private static void DeserializeLight(ref Light light, SerializationInfo info)
		{
			light.enabled = info.GetBoolean("enabled");
			light.range = info.GetSingle("range");
			light.color = (UnityEngine.Color)info.GetValue("color", typeof(UnityEngine.Color));
			light.intensity = info.GetSingle("intensity");
			light.bounceIntensity = info.GetSingle("bounceIntensity");
		}

		#endregion
	}
}
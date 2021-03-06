#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using UnityEngine;
using Project;
using Utils;

namespace Serialization
{
	[Serializable]
	public class PersistentObject : MonoBehaviour, IPersistentUnityObject
	{
		[SerializeField]
		[HideInInspector]
		private PersistentUid _persistenceUid = null;

		[SerializeField]
		[HideInInspector]
		private bool _isPrefab;

		[SerializeField]
		[HideInInspector]
		private PersistentUid _linkedPrefabUid = null;

		[SerializeField]
		[HideInInspector]
		private PersistentUid _prefabUid = null;

		[SerializeField] private bool _serializeAllComponents;
		[SerializeField] private Component[] _componentsToSerialize;
		[SerializeField] private bool _serializeChildren;

		bool _uidSet = false;

		public PersistentUid Uid
		{
			get
			{
				if (_persistenceUid == null)
				{
					_persistenceUid = PersistentUid.NewUid();
				}
				return _persistenceUid;
			}
		}

		public PersistentUid PrefabUid
		{
			get
			{
				if (_prefabUid != null && _prefabUid.Value != "")
				{
					return _prefabUid;
				}
				else
				{
					Debug.LogError("PrefabUid was accessed but was null");
					return null;
				}
			}
		}

		public bool IsPrefab
		{
			get
			{
				return _isPrefab;
			}
		}

		void Awake()
		{
		}

		void Start()
		{
			// Created from a prefab
			if (_isPrefab && Application.isPlaying)
			{
				if (_uidSet == false)
				{
					Debug.Log($"SETTING NEW ID FOR {gameObject.name}");
					_persistenceUid = PersistentUid.NewUid();
					_isPrefab = false;
					_linkedPrefabUid = _prefabUid;
				}
			}

			PersistenceController.RegisterPersistentObject(Uid, this);
		}

		void OnDestroy()
		{
			PersistenceController.UnregisterPersistentObject(Uid);
		}

		public void OnSerialize(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("uid", this.Uid.Value);

			if (_linkedPrefabUid != null)
			{
				info.AddValue("linkedPrefabUid", _linkedPrefabUid.ToString());
			}
			else
			{
				info.AddValue("linkedPrefabUid", "");
			}

			info.AddValue("pos", this.transform.position);
			info.AddValue("rot", this.transform.rotation);
			info.AddValue("scale", this.transform.localScale);

			Component[] components = _serializeAllComponents ? GetComponents<Component>() : _componentsToSerialize;
			foreach (Component component in components)
			{
				var data = new SerializableComponent(component);
				info.AddValue(component.GetType().FullName, data, component.GetType());
			}

			var lst = new List<IPersistentUnityObject>();
			this.GetComponentsInChildren<IPersistentUnityObject>(true, lst);
			if(lst.Count > 0)
			{
				var data = new ChildObjectData();
				int cnt = 0;

				for (int i = 0; i < lst.Count; i++)
				{
					if (object.ReferenceEquals(this, lst[i])) continue;

					data.Uid = lst[i].Uid;
					data.ComponentType = lst[i].GetType();
					data.Pobj = lst[i];
					info.AddValue(cnt.ToString(), data, typeof(ChildObjectData));
					cnt++;
				}
				info.AddValue("count", cnt);
			}
		}

		public void OnDeserialize(SerializationInfo info, StreamingContext context, IAssetBundle assetBundle)
		{
			_persistenceUid = new PersistentUid(info.GetString("uid"));
			_uidSet = true;

			this.transform.position = (Vector3)info.GetValue("pos", typeof(Vector3));
			this.transform.rotation = (Quaternion)info.GetValue("rot", typeof(Quaternion));
			this.transform.localScale = (Vector3)info.GetValue("scale", typeof(Vector3));

			SerializationInfoEnumerator e = info.GetEnumerator();
			while (e.MoveNext())
			{
				Type componentType = TypeUtil.FindType(e.Name, true);
				if (componentType == null)
					continue;
				Component component = this.GetComponent(componentType);
				if (component == null)
					continue;

				SerializableComponent serializedComponent = (SerializableComponent)e.Value;

				ComponentSerializationUtility.DeserializeComponent(ref component, serializedComponent.DeserializeInfo);
			}

			int cnt = info.GetInt32("count");
			if(cnt > 0)
			{
				var lst = new List<IPersistentUnityObject>();
				this.GetComponentsInChildren<IPersistentUnityObject>(true, lst);
				for (int i = 0; i < cnt; i++)
				{
					ChildObjectData data = (ChildObjectData)info.GetValue(i.ToString(), typeof(ChildObjectData));
					if (data != null && data.ComponentType != null)
					{
						IPersistentUnityObject pobj = (from o in lst where o.Uid == data.Uid select o).FirstOrDefault();
						if (pobj != null)
						{
							pobj.OnDeserialize(data.DeserializeInfo, data.DeserializeContext, assetBundle);
						}
					}
				}
			}
		}

		[System.Serializable()]
		private class ChildObjectData : ISerializable
		{
			[System.NonSerialized()]
			public PersistentUid Uid;
			[System.NonSerialized()]
			public System.Type ComponentType;

			[System.NonSerialized()]
			public IPersistentUnityObject Pobj;
			[System.NonSerialized()]
			public SerializationInfo DeserializeInfo;
			[System.NonSerialized()]
			public StreamingContext DeserializeContext;

			public ChildObjectData()
			{
			}

			public ChildObjectData(SerializationInfo info, StreamingContext context)
			{
				this.DeserializeInfo = info;
				this.DeserializeContext = context;
				this.Uid = new PersistentUid(info.GetString("sp_uid"));
				this.ComponentType = info.GetValue("sp_t", typeof(System.Type)) as System.Type;
			}

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("sp_uid", this.Uid.Value);
				info.AddValue("sp_t", this.ComponentType, typeof(System.Type));
				if (Pobj != null) Pobj.OnSerialize(info, context);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

using Project;

namespace Serialization
{
	public class PersistentObject : MonoBehaviour, IPersistentAsset
	{
		[SerializeField()]
		private long _uid;

		[SerializeField()]
		private string _assetId;

		public string AssetId
		{
			get { return _assetId; }
			set { _assetId = value; }
		}

		long IPersistentUnityObject.Uid { get { return _uid; } }

		string IPersistentAsset.AssetId { get { return _assetId; } }

		void Start()
		{
			PersistentController.RegisterPersistentObject(this);
		}

		void OnDestroy()
		{
			PersistentController.UnregisterPersistentObject(this);
		}

		void IPersistentUnityObject.OnSerialize(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("pos", this.transform.position);
			info.AddValue("rot", this.transform.rotation);
			info.AddValue("scale", this.transform.localScale);

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

		void IPersistentUnityObject.OnDeserialize(SerializationInfo info, StreamingContext context, IAssetBundle assetBundle)
		{
			this.transform.position = (Vector3)info.GetValue("pos", typeof(Vector3));
			this.transform.rotation = (Quaternion)info.GetValue("rot", typeof(Quaternion));
			this.transform.localScale = (Vector3)info.GetValue("scale", typeof(Vector3));

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
			public long Uid;
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
				this.Uid = info.GetInt64("sp_uid");
				this.ComponentType = info.GetValue("sp_t", typeof(System.Type)) as System.Type;
			}

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("sp_uid", this.Uid);
				info.AddValue("sp_t", this.ComponentType, typeof(System.Type));
				if (Pobj != null) Pobj.OnSerialize(info, context);
			}
		}
	}
}

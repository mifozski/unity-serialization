using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Serialization
{
	public class PrefabMap : ScriptableObject
	{
		Dictionary<string, PersistentObject> _prefabs = new Dictionary<string, PersistentObject>();

		[SerializeField]
		List<PersistentObject> Prefabs = new List<PersistentObject>();
		[SerializeField]
		List<string> PrefabUids = new List<string>();

		private void OnEnable()
		{
			if (_prefabs.Count == 0 && Prefabs.Count != 0)
			{
				_prefabs = PrefabUids.Zip(Prefabs, (k, v) => new { k, v })
					.ToDictionary(x => x.k, x => x.v);
			}
		}

		public void Add(PersistentObject prefab)
		{
			string uid = prefab.PrefabUid.ToString();

			if (_prefabs.ContainsKey(uid) == false)
			{
				_prefabs.Add(uid, prefab);
				Debug.Log($"Prefab {prefab.gameObject.name} added");
			}

			Prefabs = new List<PersistentObject>(_prefabs.Values);
			PrefabUids = new List<string>(_prefabs.Keys);
		}

		public void Remove(PersistentObject prefab)
		{
			PersistentUid uid = prefab.PrefabUid;

			if (_prefabs.ContainsKey(uid))
			{
				_prefabs.Remove(uid.ToString());
				Debug.Log($"Prefab {prefab.gameObject.name} removed");
			}


			Prefabs = new List<PersistentObject>(_prefabs.Values);
			PrefabUids = new List<string>(_prefabs.Keys);
		}

		public void Set(List<PersistentObject> prefabs)
		{
			_prefabs = prefabs.ToDictionary(prefab => prefab.PrefabUid.Value, prefab => prefab);
		}

		public PersistentObject GetPrefab(string uid)
		{
			PersistentObject prefab;
			_prefabs.TryGetValue(uid, out prefab);
			return prefab;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
	public class PrefabMap : ScriptableObject
	{
		[SerializeField]
		Dictionary<string, PersistentObject> _prefabs = new Dictionary<string, PersistentObject>();

		[SerializeField]
		List<PersistentObject> Prefabs = new List<PersistentObject>();

		public void Add(PersistentObject prefab)
		{
			string uid = prefab.Uid.ToString();

			if (_prefabs.ContainsKey(uid) == false)
			{
				_prefabs.Add(uid, prefab);
				Debug.Log($"Prefab {prefab.gameObject.name} added");
			}


			Prefabs = new List<PersistentObject>(_prefabs.Values);
		}

		public void Remove(PersistentObject prefab)
		{
			PersistentUid uid = prefab.Uid;

			if (_prefabs.ContainsKey(uid))
			{
				_prefabs.Remove(uid.ToString());
				Debug.Log($"Prefab {prefab.gameObject.name} removed");
			}


			Prefabs = new List<PersistentObject>(_prefabs.Values);
		}
	}
}
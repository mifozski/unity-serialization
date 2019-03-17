using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;

using Project;

namespace Serialization
{
	public class PersistenceController : MonoBehaviour
	{
		[SerializeField] private PrefabMap _prefabMap;

		static private PersistenceController _instance;

		Serializer serializer = new Serializer();

		private static bool applicationIsQuitting = false;

		static PersistenceController Instance
		{
			get
			{
				if (applicationIsQuitting)
				{
					return null;
				}
				if (_instance == null)
				{
					_instance = FindObjectOfType<PersistenceController>();
				}
				return _instance;
			}
		}

		private Dictionary<string, PersistentObject> m_PrecreatedPersistentObjects = new Dictionary<string, PersistentObject>();

		static public void RegisterPersistentObject(PersistentUid uid, PersistentObject persistentObject)
		{
			if (Instance)
			{
				Instance.m_PrecreatedPersistentObjects.Add(uid, persistentObject);
				Debug.Log($"Registering created object: {persistentObject.name} with uid: {uid}");
			}
		}

		static public void UnregisterPersistentObject(PersistentUid uid)
		{
			if (Instance)
				Instance.m_PrecreatedPersistentObjects.Remove(uid);
		}

		static public PersistentObject GetPrecreatedPersistentObject(PersistentUid uid)
		{
			PersistentObject persistentObject;
			Instance.m_PrecreatedPersistentObjects.TryGetValue(uid, out persistentObject);
			return persistentObject;
		}

		public static void RegisterPrefab(PersistentObject prefab)
		{
			if (Instance)
			{
				Instance.AddPrefab(prefab);
			}
		}

		public static void UnregisterPrefab(PersistentObject prefab)
		{
			if (Instance)
			{
				Instance.RemovePrefab(prefab);
			}
		}

		public static void OverridePrefabs(List<PersistentObject> prefabs)
		{
			if (Instance)
			{
				Instance.SetPrefabs(prefabs);
			}
		}

		public static PersistentObject GetRegisteredPrefab(string uid)
		{
			if (Instance)
			{
				return Instance.GetPrefab(uid);
			}
			return null;
		}

		private void AddPrefab(PersistentObject prefab)
		{
			if (_prefabMap == null)
				CreatePrefabMap();

			_prefabMap.Add(prefab);
		}

		private void RemovePrefab(PersistentObject prefab)
		{
			if (_prefabMap == null)
			{
				CreatePrefabMap();
				return;
			}

			_prefabMap.Remove(prefab);
		}

		private void SetPrefabs(List<PersistentObject> prefabs)
		{
			if (_prefabMap == null)
			{
				CreatePrefabMap();
			}

			_prefabMap.Set(prefabs);
		}

		private PersistentObject GetPrefab(string uid)
		{
			if (_prefabMap == null)
				return null;

			return _prefabMap.GetPrefab(uid);
		}

		private void CreatePrefabMap()
		{
			_prefabMap = ScriptableObject.CreateInstance<PrefabMap>();

			string path = "Assets";
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(PrefabMap).ToString() + ".asset");

			AssetDatabase.CreateAsset(_prefabMap, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
		}

		public void Serialize()
		{
			string saveFilePath = Application.persistentDataPath + "/" + "Save.json";

			var serializer = new Serializer();

			FileStream file = File.Create(saveFilePath);
			file.SetLength(0);

			serializer.Serialize(file, m_PrecreatedPersistentObjects);

			// foreach (KeyValuePair<long, PrecreatedPersistentObject> entry in m_PrecreatedPersistentObjects)
			// {
			// 	serializer.Serialize(file, entry.Value);
			// }
			// foreach (PersistentObject obj in m_PersistentObjects)
			// {
			// 	serializer.Serialize(file, obj);
			// }


			Debug.Log("Data written to " + saveFilePath + " @ " + DateTime.Now.ToShortTimeString());
			file.Close();
		}

		public void Deserialize()
		{
			// guids = AssetDatabase.FindAssets("t:ScriptObj");

			string saveFilePath = Application.persistentDataPath + "/" + "Save.json";
			FileStream file = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);

			var serializer = new Serializer();

			var obj = serializer.Deserialize(file);

			Debug.Log("Deserialized");
		}

		class Serializer
		{
			JsonFormatter bf;
			SPSerializer serializer;

			public Serializer()
			{
				bf = new JsonFormatter();
				serializer = new SPSerializer();
				serializer.AssetBundle = ResourcesAssetBundle.Instance;
			}

			public void Serialize(Stream serializationStream, object graph)
			{
				serializer.Serialize(bf, serializationStream, graph);
			}
			public object Deserialize(Stream serializationStream)
			{
				return serializer.Deserialize(bf, serializationStream);
			}
		}

		private void OnDestroy ()
		{
			applicationIsQuitting = true;
		}
	}
}

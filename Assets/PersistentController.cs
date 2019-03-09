﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

using Project;

namespace Serialization
{
	public class PersistentController : MonoBehaviour
	{
		static private PersistentController _instance;

		Serializer serializer = new Serializer();

		static PersistentController Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<PersistentController>();
				}
				return _instance;
			}
		}

		private List<PersistentObject> m_PersistentObjects = new List<PersistentObject>();

		private Dictionary<long, PrecreatedPersistentObject> m_PrecreatedPersistentObjects = new Dictionary<long, PrecreatedPersistentObject>();

		static public void RegisterPersistentObject(PersistentObject persistentObject)
		{
			Instance.m_PersistentObjects.Add(persistentObject);
		}

		static public void UnregisterPersistentObject(PersistentObject persistentObject)
		{
			Instance.m_PersistentObjects.Remove(persistentObject);
		}

		static public void RegisterPersistentObject(long uid, PrecreatedPersistentObject persistentObject)
		{
			Instance.m_PrecreatedPersistentObjects.Add(uid, persistentObject);
		}

		static public void UnregisterPersistentObject(long uid)
		{
			Instance.m_PrecreatedPersistentObjects.Remove(uid);
		}

		public void Serialize()
		{
			string saveFilePath = Application.persistentDataPath + "/" + "Save.json";

			var serializer = new Serializer();

			FileStream file = File.Create(saveFilePath);
			file.SetLength(0);

			foreach (KeyValuePair<long, PrecreatedPersistentObject> entry in m_PrecreatedPersistentObjects)
			{
				serializer.Serialize(file, entry.Value);
			}


			Debug.Log("Data written to " + saveFilePath + " @ " + DateTime.Now.ToShortTimeString());
			file.Close();
		}

		public void Deserialize()
		{
			string saveFilePath = Application.persistentDataPath + "/" + "Save.json";
			FileStream file = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);

			var serializer = new Serializer();

			var obj = serializer.Deserialize(file);

			Debug.Log("Deserialized");

			var we = 5;
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
	}
}
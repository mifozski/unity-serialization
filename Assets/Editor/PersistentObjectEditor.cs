using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace Serialization
{
	[CustomEditor(typeof(PersistentObject))]
	public class PersistentObjectEditor : Editor
	{
		private const string PROP_ORDER = "_order";

		PersistentObject persistentObject;

		PersistentObject prevPrefab;

		bool isPrefab;

		SerializedProperty persistenceUidProp;
		SerializedProperty persistenceUidIdProp;

		SerializedProperty prefabUidProp;
		SerializedProperty prefabUidIdProp;

		SerializedProperty linkedPrefabUidProp;
		SerializedProperty linkedPrefabUidIdProp;

		SerializedProperty isPrefabProp;
		SerializedProperty componentsToSerializeProp;

		void OnEnable()
		{
			try
			{
				persistenceUidProp = serializedObject.FindProperty("_persistenceUid");
				persistenceUidIdProp = persistenceUidProp.FindPropertyRelative("_uid");

				prefabUidProp = serializedObject.FindProperty("_prefabUid");
				prefabUidIdProp = prefabUidProp.FindPropertyRelative("_uid");

				linkedPrefabUidProp = serializedObject.FindProperty("_linkedPrefabUid");
				linkedPrefabUidIdProp = linkedPrefabUidProp.FindPropertyRelative("_uid");

				isPrefabProp = serializedObject.FindProperty("_isPrefab");

				componentsToSerializeProp = serializedObject.FindProperty("_componentsToSerialize");

				isPrefab = isPrefabProp.boolValue;
			}
			catch
			{

			}

			persistentObject = target as PersistentObject;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Label($"Prefab status: {PrefabUtility.GetPrefabInstanceStatus(persistentObject)}");

			EditorGUI.BeginChangeCheck();

			if (isPrefab && prevPrefab == null)
			{
				prevPrefab = GetPrefab();

				if (prevPrefab)
				{
					PersistenceController.RegisterPrefab(prevPrefab);
				}
			}

			EditorGUILayout.PropertyField(isPrefabProp);

			if (EditorGUI.EndChangeCheck())
			{
				isPrefab = isPrefabProp.boolValue;
				if (isPrefab)
				{
					PersistentObject prefab = GetPrefab();

					if (prefab != null)
					{
						PersistenceController.RegisterPrefab(prefab);

						prevPrefab = prefab;
					}

					if (prefabUidIdProp.stringValue == "")
					{
						prefabUidIdProp.stringValue = PersistentUid.NewUid().Value;
					}
				}
				else
				{
					if (prevPrefab)
					{
						PersistenceController.UnregisterPrefab(prevPrefab);
					}

					if (persistenceUidIdProp.stringValue == "")
					{
						persistenceUidIdProp.stringValue = PersistentUid.NewUid().Value;
					}
				}
			}

			if (isPrefab && prevPrefab == null)
			{
				EditorGUILayout.HelpBox("Is Prefab property was set to true, but no corresponding prefab was found", MessageType.Warning);
			}

			if (isPrefab)
			{
				EditorGUILayout.PropertyField(prefabUidProp);
			}
			else
			{
				EditorGUILayout.PropertyField(persistenceUidProp);

				if (linkedPrefabUidIdProp.stringValue != "")
				{
					EditorGUILayout.PropertyField(linkedPrefabUidProp);
				}
			}

			EditorGUILayout.PropertyField(componentsToSerializeProp, true);

			serializedObject.ApplyModifiedProperties();
		}

		PersistentObject GetPrefab()
		{
			bool isInPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null;
			if (isInPrefabMode)
			{
				string prefabPath = PrefabStageUtility.GetPrefabStage(persistentObject.gameObject).prefabAssetPath;
				return AssetDatabase.LoadAssetAtPath(prefabPath, persistentObject.GetType()) as PersistentObject;
			}
			// We're probably trying to set up the prefab object directly in the scene
			else
			{
				return PrefabUtility.GetCorrespondingObjectFromSource<PersistentObject>(persistentObject);
			}
		}

		public void OnDestroy()
		{
			if (Application.isEditor && PrefabStageUtility.GetCurrentPrefabStage() != null)
			{
				if (persistentObject == null)
				{
					PersistenceController.UnregisterPrefab(persistentObject);
				}
			}
		}
	}
}

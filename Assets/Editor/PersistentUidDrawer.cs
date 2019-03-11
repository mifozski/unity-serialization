using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Serialization
{
	[CustomPropertyDrawer(typeof(PersistentUid))]
	public class PersistentUidDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, label);
			float w = Mathf.Min(position.width, 60f);
			var r2 = new Rect(position.xMax - w, position.yMin, w, position.height);
			var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width - w, 0f), position.height);

			var uidProp = property.FindPropertyRelative("_uid");
			var value = uidProp.stringValue;

			// Debug.Log($"Value: {value}");

			var attrib = this.fieldInfo.GetCustomAttributes(typeof(PersistentUid.ConfigAttribute), false).FirstOrDefault() as PersistentUid.ConfigAttribute;
			bool resetOnZero = attrib == null || !attrib.AllowZero;
			bool readWrite = attrib == null || !attrib.ReadOnly;

			// if (readWrite)
			// {
			// 	//read-write
			// 	EditorGUI.BeginChangeCheck();
			// 	var sval = EditorGUI.TextField(r1, value.ToString());
			// 	if (EditorGUI.EndChangeCheck())
			// 	{
			// 		uidProp.stringValue = sval;
			// 	}
			// }
			// else
			// {
				//read-only
				GUI.enabled = false;
				// EditorGUI.PropertyField(position, uidProp, label, true);
				EditorGUI.SelectableLabel(r1, value, EditorStyles.textField);
				GUI.enabled = true;
			// }

			if (GUI.Button(r2, "New Id") || (resetOnZero && value == ""))
			{
				value = PersistentUid.NewUid().Value;
				uidProp.stringValue = value.ToString();
			}

			EditorGUI.EndProperty();
		}
	}
}

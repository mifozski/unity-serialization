using System;

namespace Serialization
{
	[Serializable]
	public sealed class PersistentUid
	{
		[UnityEngine.SerializeField]
		string _uid;

		static public PersistentUid Zero { get { return new PersistentUid(""); } }

		public PersistentUid(string uid)
		{
			_uid = uid;
		}

		static public PersistentUid NewUid()
		{
			string uid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
			uid = uid.Substring(0, uid.Length - 2);
			return new PersistentUid(uid);
		}

		public string Value
		{
			get
			{
				return _uid;
			}
		}

		override public string ToString()
		{
			return _uid;
		}

		public static implicit operator string(PersistentUid uid)
        {
            return uid.ToString();
        }

		public class ConfigAttribute : Attribute
		{
			public bool ReadOnly;
			public bool AllowZero;
		}
	}
}
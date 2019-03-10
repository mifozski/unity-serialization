using System.Runtime.Serialization;

using Project;

namespace Serialization
{
	/// <summary>
	/// Allows serializing UnityObjects in a persistent manner at runtime.
	/// This can be used in game save files to serialize an entity that is based
	/// on some prefab.
	///
	/// Any script that needs to save persistent data will have the messages called
	/// on it with a SerializationInfo to either store its state in, or to retrieve
	/// its state from.
	/// </summary>
	public interface IPersistentUnityObject
	{
		/// <summary>
		/// A uid to identify the object on reload. This must be unique and persistent.
		/// </summary>
		PersistentUid Uid { get; }

		void OnSerialize(SerializationInfo info, StreamingContext context);

		void OnDeserialize(SerializationInfo info, StreamingContext context, IAssetBundle assetBundle);
	}

	public interface IPersistentAsset : IPersistentUnityObject
	{
		string AssetId { get; }
	}
}

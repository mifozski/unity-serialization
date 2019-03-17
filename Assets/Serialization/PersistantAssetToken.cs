using System;
using System.Runtime.Serialization;

using Project;
using Utils;

namespace Serialization
{

    [System.Serializable()]
    public class PersistentAssetToken : IPersistentUnityObject
    {
        #region Fields

        [System.NonSerialized()]
        protected IPersistentAsset _asset;

        [System.NonSerialized()]
        private SerializationInfo _info;
        [System.NonSerialized()]
        private StreamingContext _context;
        [System.NonSerialized()]
        private IAssetBundle _bundle;

        #endregion

        #region CONSTRUCTOR

        public PersistentAssetToken(IPersistentAsset obj)
        {
            _asset = obj;
        }

        public PersistentAssetToken()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// The asset to serialize, this value will be null after deserialization. See 'Create' for after deserialization.
        /// </summary>
        public IPersistentAsset Asset
        {
            get { return _asset; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create just the root object from the asset, will still need to apply all serialized data to the object.
        /// </summary>
        /// <returns></returns>
        protected object CreateRoot()
        {
            if (_bundle == null) return null;

            var resourceId = _info.GetString("sp*id");
            var obj = _bundle.LoadAsset(resourceId);
            if (obj == null) return null;

            obj = UnityEngine.Object.Instantiate(obj);
            var pobj = ObjUtil.GetAsFromSource<IPersistentAsset>(obj);
            return (object)pobj ?? (object)obj;
        }

        protected void SetObjectData(object obj)
        {
            if (_bundle == null) return;

            var pobj = ObjUtil.GetAsFromSource<IPersistentAsset>(obj);
            if (pobj != null) pobj.OnDeserialize(_info, _context, _bundle);
        }

        /// <summary>
        /// After deserializing the token, call this to get a copy of the UnityObject.
        /// </summary>
        /// <returns></returns>
        public object Create()
        {
            var obj = this.CreateRoot();
            this.SetObjectData(obj);
            return obj;
        }

        #endregion

        #region IPersistentUnityObject Interface

        PersistentUid IPersistentUnityObject.Uid
        {
            get
            {
                if (_asset == null) return PersistentUid.Zero;

                return _asset.Uid;
            }
        }

        public void OnSerialize(SerializationInfo info, StreamingContext context)
        {
            if (_asset == null) return;

            info.AddValue("sp*id", _asset.AssetId);
            _asset.OnSerialize(info, context);
        }

        public void OnDeserialize(SerializationInfo info, StreamingContext context, IAssetBundle assetBundle)
        {
            _info = info;
            _context = context;
            _bundle = assetBundle;
        }

        #endregion

    }

}

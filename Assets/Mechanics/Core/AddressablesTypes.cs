using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

namespace Armere.AddressableTypes
{

    [Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }


    [Serializable]
    public class AssetReferenceMesh : AssetReferenceT<Mesh>
    {
        public AssetReferenceMesh(string guid) : base(guid) { }
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AssetDanshari
{
	//[CreateAssetMenu (fileName = "AssetDanshariSetting", menuName = "AssetDanshari Setting", order = 1)]
    public class AssetDanshariSetting : ScriptableObject
    {
	    [Serializable]
        public class AssetReferenceInfo
        {
            public string title;
            public string referenceFolder;
            public string assetFolder;
            public string assetCommonFolder;
        }

        [FormerlySerializedAs("m_AssetReferenceInfos")] [SerializeField]
        private List<AssetReferenceInfo> mAssetReferenceInfos = new ();

        public List<AssetReferenceInfo> AssetReferenceInfos => mAssetReferenceInfos;
    }
}
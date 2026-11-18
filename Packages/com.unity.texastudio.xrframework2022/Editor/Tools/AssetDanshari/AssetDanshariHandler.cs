using System;
using System.Collections.Generic;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetDanshariHandler
    {
        public static Action<GenericMenu> OnDependenciesContextDraw;

        public static Action<string> OnDependenciesFindItem;

        public static Action<string, List<AssetTreeModel.AssetInfo>, AssetTreeModel> OnDependenciesLoadDataMore;
    }
}
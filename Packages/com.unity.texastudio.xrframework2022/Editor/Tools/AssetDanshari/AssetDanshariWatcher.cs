using System;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetDanshariWatcher : AssetPostprocessor
    {
        public static event Action<string[]> OnImportedAssets;
        public static event Action<string[]> OnDeletedAssets;
        public static event Action<string[], string[]> OnMovedAssets;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0 && OnImportedAssets != null)
            {
                OnImportedAssets(importedAssets);
            }

            if (deletedAssets.Length > 0 && OnDeletedAssets != null)
            {
                OnDeletedAssets(deletedAssets);
            }

            if (movedAssets.Length > 0 && OnMovedAssets != null)
            {
                OnMovedAssets(movedFromAssetPaths, movedAssets);
            }
        }
    }
}
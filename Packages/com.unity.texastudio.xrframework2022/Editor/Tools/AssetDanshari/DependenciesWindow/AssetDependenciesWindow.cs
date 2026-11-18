using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDependenciesWindow : AssetBaseWindow
    {
        private bool _mFilterEmpty;

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().dependenciesTitle;
            minSize = new Vector2(727f, 331f);
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            _mFilterEmpty = false;
            MAssetTreeModel = new AssetDependenciesTreeModel();
            MAssetTreeView = new AssetDependenciesTreeView(mTreeViewState, multiColumnHeader, MAssetTreeModel);
        }

        protected override void DrawGUI(GUIContent waiting, GUIContent nothing, bool expandCollapseComplex)
        {
            base.DrawGUI(AssetDanshariStyle.Get().dependenciesWaiting, AssetDanshariStyle.Get().dependenciesNothing, true);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().nameHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 280,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 350,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }

        protected override void DrawToolbarMore()
        {
            EditorGUI.BeginChangeCheck();
            _mFilterEmpty = GUILayout.Toggle(_mFilterEmpty, AssetDanshariStyle.Get().dependenciesFilter, EditorStyles.toolbarButton,
                GUILayout.Width(70f));
            if (EditorGUI.EndChangeCheck() && MAssetTreeView != null)
            {
                (MAssetTreeView as AssetDependenciesTreeView)?.SetFilterEmpty(_mFilterEmpty);
            }
        }
    }
}
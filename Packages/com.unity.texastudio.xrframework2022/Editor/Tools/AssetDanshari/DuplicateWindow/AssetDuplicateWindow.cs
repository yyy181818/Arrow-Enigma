using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetDuplicateWindow : AssetBaseWindow
    {
        private bool _mManualAdd;
        private readonly string[] _mManualPaths = new string[5];

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().duplicateTitle;
            minSize = new Vector2(727f, 331f);
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            MAssetTreeModel = new AssetDuplicateTreeModel();
            MAssetTreeView = new AssetDuplicateTreeView(mTreeViewState, multiColumnHeader, MAssetTreeModel);
        }

        protected override void DrawGUI(GUIContent waiting, GUIContent nothing, bool expandCollapseComplex)
        {
            if (_mManualAdd)
            {
                DrawManualAdd();
            }
            else
            {
                base.DrawGUI(AssetDanshariStyle.Get().duplicateWaiting, AssetDanshariStyle.Get().duplicateNothing,
                    false);
            }
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 200,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 300,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent3,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 60,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent4,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }

        protected override void DrawToolbarMore()
        {
            if (GUILayout.Button(AssetDanshariStyle.Get().duplicateManualAdd, EditorStyles.toolbarButton,
                    GUILayout.Width(70f)))
            {
                _mManualAdd = true;
            }
        }

        private void DrawManualAdd()
        {
            var style = AssetDanshariStyle.Get();
            EditorGUILayout.LabelField(style.duplicateManualAdd);
            EditorGUI.indentLevel++;
            for (int i = 0; i < _mManualPaths.Length; i++)
            {
                var textRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField,
                    GUILayout.ExpandWidth(true));
                _mManualPaths[i] = EditorGUI.TextField(textRect, style.duplicateHeaderContent2, _mManualPaths[i]);
                _mManualPaths[i] = OnDrawElementAcceptDrop(textRect, _mManualPaths[i]);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(style.sureStr))
            {
                var id = ((AssetDuplicateTreeModel)MAssetTreeModel).AddManualData(_mManualPaths);
                if (id > 0)
                {
                    Array.Clear(_mManualPaths, 0, _mManualPaths.Length);
                    MAssetTreeView.Reload();
                    MAssetTreeView.ForceRefresh();
                    MAssetTreeView.SetSelection(new List<int>() { id }, TreeViewSelectionOptions.RevealAndFrame);
                    _mManualAdd = false;
                }
            }

            if (GUILayout.Button(style.cancelStr))
            {
                _mManualAdd = false;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private string OnDrawElementAcceptDrop(Rect rect, string label)
        {
            if (!rect.Contains(Event.current.mousePosition)) return label;
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length <= 0 ||
                string.IsNullOrEmpty(DragAndDrop.paths[0])) return label;
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            if (Event.current.type != EventType.DragPerform) return label;
            DragAndDrop.AcceptDrag();

            if (AssetDatabase.IsValidFolder(DragAndDrop.paths[0])) return label;
            GUI.changed = true;
            return DragAndDrop.paths[0];
        }
    }
}
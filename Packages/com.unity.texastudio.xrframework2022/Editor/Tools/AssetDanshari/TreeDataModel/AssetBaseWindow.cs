using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Serialization;

namespace AssetDanshari
{
    public class AssetBaseWindow : EditorWindow
    {
        public static void CheckPaths<T>(string refPaths, string paths, string commonPaths) where T : AssetBaseWindow
        {
            var window = GetWindow<T>();
            window.Focus();
            window.SetCheckPaths(refPaths, paths, commonPaths);
        }

        private SearchField _mSearchField;
        protected AssetTreeModel MAssetTreeModel;
        protected AssetTreeView MAssetTreeView;
        private Action _mCallbackAfterFrame;
        private double _mCallbackDelayTime;

        [FormerlySerializedAs("m_TreeViewState")] [SerializeField]
        protected TreeViewState mTreeViewState;
        [FormerlySerializedAs("m_MultiColumnHeaderState")] [SerializeField]
        private MultiColumnHeaderState mMultiColumnHeaderState;

        private void OnDisable()
        {
            DestroyTree();
        }

        private void OnGUI()
        {
            if (_mCallbackAfterFrame != null)
            {
                if (_mSearchField == null)
                {
                    // 延迟处理，防止MAC上闪退
                    _mCallbackDelayTime = EditorApplication.timeSinceStartup + 0.5f;
                }

                if (_mCallbackDelayTime < EditorApplication.timeSinceStartup)
                {
                    var cb = _mCallbackAfterFrame;
                    _mCallbackAfterFrame = null;
                    cb();
                    RemoveNotification();
                }
                else
                {
                    Repaint();
                }
            }
            Init();
            DrawGUI(GUIContent.none, GUIContent.none, false);
        }

        private void Init()
        {
            if (_mSearchField != null)
            {
                return;
            }

            mTreeViewState ??= new TreeViewState();

            var firstInit = mMultiColumnHeaderState == null;
            var headerState = CreateMultiColumnHeader();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(mMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(mMultiColumnHeaderState, headerState);
            mMultiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
            {
                multiColumnHeader.ResizeToFit();
            }

            _mSearchField = new SearchField();

            // 需要在 OnGUI 里面进行构造，否则treeView的GUIView会取到上一个窗口，导致焦点问题
            InitTree(multiColumnHeader);
        }

        protected virtual void InitTree(MultiColumnHeader multiColumnHeader)
        {
            MAssetTreeModel = new AssetTreeModel();
            MAssetTreeView = new AssetTreeView(mTreeViewState, multiColumnHeader, MAssetTreeModel);
        }

        private void DestroyTree()
        {
            MAssetTreeView?.Destroy();
        }

        protected virtual void DrawGUI(GUIContent waiting, GUIContent nothing, bool expandCollapseComplex)
        {
            var style = AssetDanshariStyle.Get();
            style.InitGUI();

            if (MAssetTreeModel.AssetPaths != null)
            {
                if (!MAssetTreeModel.HasData())
                {
                    ShowNotification(nothing);
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    if (expandCollapseComplex)
                    {
                        DrawToolbarExpandCollapse2();
                    }
                    else
                    {
                        DrawToolbarExpandCollapse();
                    }
                    EditorGUI.BeginChangeCheck();
                    MAssetTreeView.searchString = _mSearchField.OnToolbarGUI(MAssetTreeView.searchString);
                    if (EditorGUI.EndChangeCheck() && GUIUtility.keyboardControl == 0)
                    {
                        MAssetTreeView.SetFocusAndEnsureSelectedItem();
                    }
                    DrawToolbarMore();
                    if (GUILayout.Button(style.exportCsv, EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    {
                        MAssetTreeModel.ExportCsv();
                    }
                    EditorGUILayout.EndHorizontal();
                    MAssetTreeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
                }
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(MAssetTreeModel.AssetPaths, style.labelStyle);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                ShowNotification(waiting);
            }
        }

        protected virtual MultiColumnHeaderState CreateMultiColumnHeader()
        {
            return null;
        }

        protected virtual void DrawToolbarMore()
        {
        }

        private void DrawToolbarExpandCollapse()
        {
            if (GUILayout.Button(AssetDanshariStyle.Get().expandAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
            {
                MAssetTreeView.ExpandAll();
            }
            if (GUILayout.Button(AssetDanshariStyle.Get().collapseAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
            {
                MAssetTreeView.CollapseAll();
            }
        }

        private void DrawToolbarExpandCollapse2()
        {
            var style = AssetDanshariStyle.Get();
            var toolBtnRect = GUILayoutUtility.GetRect(style.expandAll, EditorStyles.toolbarDropDown, GUILayout.Width(50f));
            if (GUI.Button(toolBtnRect, style.expandAll, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(style.expandAll2, false, MAssetTreeView.ExpandAll);
                menu.AddItem(style.expandAll3, false, MAssetTreeView.ExpandAllExceptLast);
                menu.DropDown(toolBtnRect);
            }
            toolBtnRect = GUILayoutUtility.GetRect(style.collapseAll, EditorStyles.toolbarDropDown, GUILayout.Width(50f));
            if (!GUI.Button(toolBtnRect, style.collapseAll, EditorStyles.toolbarDropDown)) return;
            {
                var menu = new GenericMenu();
                menu.AddItem(style.collapseAll2, false, MAssetTreeView.CollapseAll);
                menu.AddItem(style.collapseAll3, false, MAssetTreeView.CollapseOnlyLast);
                menu.DropDown(toolBtnRect);
            }
        }

        private void SetCheckPaths(string refPaths, string paths, string commonPaths)
        {
            _mCallbackAfterFrame = () =>
            {
                MAssetTreeModel.SetDataPaths(refPaths, paths, commonPaths);
                if (!MAssetTreeModel.HasData()) return;
                MAssetTreeView.Reload();
                MAssetTreeView.ExpandAllExceptLast();
            };
            Repaint();
        }
    }
}

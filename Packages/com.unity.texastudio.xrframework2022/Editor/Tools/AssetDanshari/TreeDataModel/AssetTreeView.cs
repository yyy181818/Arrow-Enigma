using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetTreeView : TreeView
    {
        private readonly AssetTreeModel _mModel;
        private readonly List<TreeViewItem> _mWatcherItems = new();

        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(
            state, multiColumnHeader)
        {
            _mModel = model;
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.height = 23f;

            AssetDanshariWatcher.OnImportedAssets += OnWatcherImportedAssets;
            AssetDanshariWatcher.OnDeletedAssets += OnWatcherDeletedAssets;
            AssetDanshariWatcher.OnMovedAssets += OnWatcherMovedAssets;
        }

        public virtual void Destroy()
        {
            AssetDanshariWatcher.OnImportedAssets -= OnWatcherImportedAssets;
            AssetDanshariWatcher.OnDeletedAssets -= OnWatcherDeletedAssets;
            AssetDanshariWatcher.OnMovedAssets -= OnWatcherMovedAssets;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            if (_mModel is { Data: not null })
            {
                foreach (var info in _mModel.Data.Children)
                {
                    BuildDataDir(info, root);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            SortTreeViewNaturalCompare(root);
            return root;
        }

        private static void BuildDataDir(AssetTreeModel.AssetInfo dirInfo, TreeViewItem parent)
        {
            var dirItem = new AssetTreeViewItem<AssetTreeModel.AssetInfo>(dirInfo.ID, -1, dirInfo.DisplayName, dirInfo);
            if (dirInfo.IsFolder)
            {
                dirItem.icon = AssetDanshariStyle.Get().folderIcon;
            }

            parent.AddChild(dirItem);

            if (!dirInfo.HasChildren) return;
            foreach (var childInfo in dirInfo.Children)
            {
                BuildDataDir(childInfo, dirItem);
            }
        }

        protected static AssetTreeModel.AssetInfo GetItemAssetInfo(TreeViewItem item)
        {
            var item2 = item as AssetTreeViewItem<AssetTreeModel.AssetInfo>;
            return item2?.Data;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is AssetTreeViewItem<AssetTreeModel.AssetInfo> item)
            {
                for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }

                return;
            }

            base.RowGUI(args);
        }

        protected virtual void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeModel.AssetInfo> item, int column,
            ref RowGUIArgs args)
        {
        }

        protected void DrawItemWithIcon(Rect cellRect, TreeViewItem item, ref RowGUIArgs args,
            string displayName, string fileRelativePath, bool deleted, bool added, bool contentIndent = true,
            bool foldoutIndent = false)
        {
            if (contentIndent)
            {
                var num = GetContentIndent(item);
                cellRect.xMin += num;
            }

            if (foldoutIndent)
            {
                var num = GetFoldoutIndent(item);
                cellRect.xMin += num;
            }

            var position = cellRect;
            position.width = 16f;
            position.height = 16f;
            position.y += 2f;
            Texture iconForItem = item.icon;
            if (iconForItem == null && !deleted)
            {
                iconForItem = AssetDatabase.GetCachedIcon(fileRelativePath);
                if (iconForItem)
                {
                    item.icon = iconForItem as Texture2D;
                }
            }

            if (iconForItem)
            {
                GUI.DrawTexture(position, iconForItem, ScaleMode.ScaleToFit);
                item.icon = iconForItem as Texture2D;
            }

            cellRect.xMin += 18f;
            DefaultGUI.Label(cellRect, displayName, args.selected, args.focused);
            if (!deleted && !added) return;
            position.x = cellRect.xMax - 40f;
            position.y += 3f;
            position.height = 9f;
            position.width = 40f;
            GUI.DrawTexture(position,
                added ? AssetDanshariStyle.Get().iconNew.image : AssetDanshariStyle.Get().iconDelete.image,
                ScaleMode.ScaleToFit);
        }

        protected override void DoubleClickedItem(int id)
        {
            var assetInfo = GetItemAssetInfo(FindItem(id, rootItem));
            if (assetInfo == null || assetInfo.Deleted)
            {
                return;
            }

            AssetTreeModel.PingObject(assetInfo.FileRelativePath);
        }


        protected void OnContextSetActiveItem(object userdata)
        {
            DoubleClickedItem((int)userdata);
        }

        protected void OnContextExplorerActiveItem(object userdata)
        {
            var assetInfo = GetItemAssetInfo((TreeViewItem)userdata);
            if (assetInfo == null || assetInfo.Deleted || string.IsNullOrEmpty(assetInfo.FileRelativePath))
            {
                return;
            }

            EditorUtility.RevealInFinder(assetInfo.FileRelativePath);
        }

        protected void AddContextMoveComm(GenericMenu menu)
        {
            if (_mModel.CommonDirs == null) return;
            foreach (var dir in _mModel.CommonDirs)
            {
                menu.AddItem(new GUIContent(AssetDanshariStyle.Get().duplicateContextMoveComm + dir.DisplayName), false,
                    OnContextMoveItem, dir.FileRelativePath);
            }
        }

        private void OnContextMoveItem(object userdata)
        {
            if (!HasSelection())
            {
                return;
            }

            var selects = GetSelection();
            foreach (var select in selects)
            {
                var assetInfo = GetItemAssetInfo(FindItem(select, rootItem));
                if (assetInfo == null || assetInfo.Deleted)
                {
                    continue;
                }

                var dirPath = userdata as string;
                AssetTreeModel.SetMoveToCommon(assetInfo, dirPath);
            }
        }

        /// <summary>
        /// 展开全部，除了最后一层
        /// </summary>
        public void ExpandAllExceptLast()
        {
            ExpandAll();
            SetExpandedAtLast(rootItem, false);
        }

        public void CollapseOnlyLast()
        {
            SetExpandedAtLast(rootItem, false);
        }

        private bool SetExpandedAtLast(TreeViewItem item, bool expanded)
        {
            if (item.hasChildren)
            {
                foreach (var child in item.children.Where(child => SetExpandedAtLast(child, expanded)))
                {
                    break;
                }
            }
            else if (IsExtraItem(item))
            {
                SetExpanded(item.parent.id, expanded);
                return true;
            }

            return false;
        }

        private bool IsExtraItem(int id)
        {
            return IsExtraItem(FindItem(id, rootItem));
        }

        private static bool IsExtraItem(TreeViewItem item)
        {
            var assetInfo = GetItemAssetInfo(item);
            return assetInfo is { IsExtra: true };
        }

        /// <summary>
        /// 选中包括了额外显示项
        /// </summary>
        /// <returns></returns>
        protected bool IsSelectionContainsReverseItem()
        {
            var selects = GetSelection();
            return selects.Any(IsExtraItem);
        }

        /// <summary>
        /// 是否选中了多个
        /// </summary>
        /// <returns></returns>
        protected bool IsSelectionMulti()
        {
            var selects = GetSelection();
            return selects.Count > 1;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sortFromThisItem"></param>
        private static void SortTreeViewNaturalCompare(TreeViewItem sortFromThisItem)
        {
            if (sortFromThisItem == null)
            {
                return;
            }

            if (!sortFromThisItem.hasChildren) return;
            sortFromThisItem.children.Sort((a, b) => EditorUtility.NaturalCompare(a.displayName, b.displayName));
            foreach (var child in sortFromThisItem.children)
            {
                SortTreeViewNaturalCompare(child);
            }
        }

        public void ForceRefresh()
        {
            SetExpanded(rootItem.id, false);
            SetExpanded(rootItem.id, true);
        }

        #region 数据变化

        private void OnWatcherImportedAssets(string[] importedAssets)
        {
            Debug.Log("importedAsset");
            foreach (var importedAsset in importedAssets)
            {
                Debug.Log(importedAsset);
            }

            if (!OnWatcherImportedAssetsEvent(importedAssets, true)) return;
            ForceRefresh();
            Repaint();
        }

        private void OnWatcherDeletedAssets(string[] deletedAssets)
        {
            Debug.Log("deletedAssets");
            foreach (var importedAsset in deletedAssets)
            {
                Debug.Log(importedAsset);
            }

            if (OnWatcherDeletedAssetsEvent(deletedAssets))
            {
                Repaint();
            }
        }

        private void OnWatcherMovedAssets(string[] movedFromAssetPaths, string[] movedAssets)
        {
            Debug.Log("movedAssets");
            foreach (var importedAsset in movedFromAssetPaths)
            {
                Debug.Log(importedAsset);
            }

            if (!OnWatcherMovedAssetsEvent(movedFromAssetPaths, movedAssets, true)) return;
            ForceRefresh();
            Repaint();
        }

        protected virtual bool OnWatcherImportedAssetsEvent(string[] importedAssets, bool resortItem)
        {
            if (importedAssets.Length == 0 || !resortItem || rootItem == null)
            {
                return false;
            }

            Array.Sort(importedAssets, EditorUtility.NaturalCompare);
            foreach (var importedAsset in importedAssets)
            {
                var item = FindItemByAssetPath(rootItem, Path.GetDirectoryName(importedAsset));
                if (item == null)
                {
                    continue;
                }

                var assetInfo = GetItemAssetInfo(item);
                if (assetInfo == null)
                {
                    continue;
                }

                // 查找是否原先已经有了（被删除再导入）
                var item2 = FindItemByAssetPath(item, importedAsset);
                if (item2 != null)
                {
                    var assetInfo2 = GetItemAssetInfo(item2);
                    if (assetInfo2 == null) continue;
                    assetInfo2.Deleted = false;
                    assetInfo2.Added = true;
                }
                else
                {
                    var assetInfo2 = _mModel.GenAssetInfo(importedAsset);
                    assetInfo.AddChild(assetInfo2);

                    item2 = new AssetTreeViewItem<AssetTreeModel.AssetInfo>(assetInfo2.ID, -1, assetInfo2.DisplayName,
                        assetInfo2);
                    if (assetInfo2.IsFolder)
                    {
                        item2.icon = AssetDanshariStyle.Get().folderIcon;
                    }

                    assetInfo2.Added = true;
                    item.AddChild(item2);
                }
            }

            SetupDepthsFromParentsAndChildren(rootItem);
            SortTreeViewNaturalCompare(rootItem);
            return true;
        }

        private bool OnWatcherDeletedAssetsEvent(string[] deletedAssets)
        {
            _mWatcherItems.Clear();
            FindItemsByAssetPaths(rootItem, deletedAssets, _mWatcherItems);
            if (_mWatcherItems.Count <= 0) return false;
            foreach (var assetInfo in _mWatcherItems.Select(GetItemAssetInfo).Where(assetInfo => assetInfo != null))
            {
                assetInfo.Deleted = true;
                assetInfo.Added = false;
            }

            return true;
        }

        protected virtual bool OnWatcherMovedAssetsEvent(string[] movedFromAssetPaths, string[] movedAssets,
            bool resortItem)
        {
            _mWatcherItems.Clear();
            FindItemsByAssetPaths(rootItem, movedFromAssetPaths, _mWatcherItems);
            var importedAssets = movedAssets.ToList();
            if (_mWatcherItems.Count > 0)
            {
                var movedFromAssetPathsList = movedFromAssetPaths.ToList();
                foreach (var item in _mWatcherItems)
                {
                    var assetInfo = GetItemAssetInfo(item);
                    if (assetInfo == null) continue;
                    var idx = movedFromAssetPathsList.IndexOf(assetInfo.FileRelativePath);
                    if (idx == -1) continue;
                    assetInfo.FileRelativePath = movedAssets[idx];
                    assetInfo.DisplayName = Path.GetFileName(assetInfo.FileRelativePath);
                    importedAssets.Remove(movedAssets[idx]);
                }

                // 移除掉额外显示的项，因为不需要变动
                _mWatcherItems.RemoveAll(IsExtraItem);
            }

            if (resortItem && _mWatcherItems.Count > 0)
            {
                // 先移除
                foreach (var watcherItem in _mWatcherItems)
                {
                    if (watcherItem.parent != null)
                    {
                        watcherItem.parent.children.Remove(watcherItem);
                    }

                    watcherItem.parent = null;

                    var assetInfo = GetItemAssetInfo(watcherItem);
                    if (assetInfo == null) continue;
                    assetInfo.Parent?.Children.Remove(assetInfo);

                    assetInfo.Parent = null;
                }

                // 排序，以防止先处理了文件
                _mWatcherItems.Sort((a, b) =>
                {
                    var aa = GetItemAssetInfo(a);
                    var bb = GetItemAssetInfo(b);
                    if (aa != null && bb != null)
                    {
                        return EditorUtility.NaturalCompare(aa.FileRelativePath, bb.FileRelativePath);
                    }

                    return EditorUtility.NaturalCompare(a.displayName, b.displayName);
                });

                foreach (var watcherItem in _mWatcherItems)
                {
                    var assetInfo = GetItemAssetInfo(watcherItem);
                    if (assetInfo == null)
                    {
                        continue;
                    }

                    var item = FindItemByAssetPath(rootItem, Path.GetDirectoryName(assetInfo.FileRelativePath));
                    if (item == null)
                    {
                        continue;
                    }

                    var assetInfo2 = GetItemAssetInfo(item);
                    if (assetInfo2 == null)
                    {
                        continue;
                    }

                    item.AddChild(watcherItem);
                    assetInfo2.AddChild(assetInfo);
                }

                SetupDepthsFromParentsAndChildren(rootItem);
                SortTreeViewNaturalCompare(rootItem);
            }

            var ret = OnWatcherImportedAssetsEvent(importedAssets.ToArray(), resortItem);
            return _mWatcherItems.Count > 0 || ret;
        }

        private static void FindItemsByAssetPaths(TreeViewItem searchFromThisItem, IEnumerable<string> assetPaths,
            ICollection<TreeViewItem> result)
        {
            if (searchFromThisItem == null)
            {
                return;
            }

            var assetInfo = GetItemAssetInfo(searchFromThisItem);
            var enumerable = assetPaths.ToList();
            if (assetInfo != null)
            {
                if (enumerable.Any(assetPath => assetPath == assetInfo.FileRelativePath))
                {
                    result.Add(searchFromThisItem);
                }
            }

            if (!searchFromThisItem.hasChildren) return;
            foreach (var child in searchFromThisItem.children)
            {
                FindItemsByAssetPaths(child, enumerable, result);
            }
        }

        protected static TreeViewItem FindItemByAssetPath(TreeViewItem searchFromThisItem, string assetPath)
        {
            if (searchFromThisItem == null)
            {
                return null;
            }

            var assetInfo = GetItemAssetInfo(searchFromThisItem);
            if (assetInfo != null)
            {
                if (assetPath == assetInfo.FileRelativePath)
                {
                    return searchFromThisItem;
                }
            }

            if (!searchFromThisItem.hasChildren) return null;
            return searchFromThisItem.children.Select(child => FindItemByAssetPath(child, assetPath))
                .FirstOrDefault(item => item != null);
        }

        #endregion
    }
}
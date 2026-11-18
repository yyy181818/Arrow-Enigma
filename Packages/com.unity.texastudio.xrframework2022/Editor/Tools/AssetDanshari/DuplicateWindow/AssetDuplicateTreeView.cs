using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeView : AssetTreeView
    {
        private AssetDuplicateTreeModel Model { get; set; }

        public AssetDuplicateTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) :
            base(state, multiColumnHeader, model)
        {
            Model = model as AssetDuplicateTreeModel;
        }

        protected override void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeModel.AssetInfo> item, int column,
            ref RowGUIArgs args)
        {
            var info = item.Data;
            if (info.IsExtra)
            {
                if (column == 0)
                {
                    DefaultGUI.Label(cellRect, info.DisplayName, args.selected, args.focused);
                }

                return;
            }

            var md5Info = info.BindObj as AssetDuplicateTreeModel.FileMd5Info;
            switch (column)
            {
                case 0:
                    DrawItemWithIcon(cellRect, item, ref args, info.DisplayName, info.FileRelativePath, info.Deleted,
                        info.Added, false, true);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, info.FileRelativePath, args.selected, args.focused);
                    break;
                case 2:
                    if (md5Info != null) DefaultGUI.Label(cellRect, md5Info.FileLength, args.selected, args.focused);
                    break;
                case 3:
                    if (md5Info != null) DefaultGUI.Label(cellRect, md5Info.FileTime, args.selected, args.focused);
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetTreeModel.AssetInfo>;
            if (item == null || item.Data.Deleted || item.Data.IsExtra)
            {
                return;
            }

            var menu = new GenericMenu();
            menu.AddItem(AssetDanshariStyle.Get().locationContext, false, OnContextSetActiveItem, id);
            menu.AddItem(AssetDanshariStyle.Get().explorerContext, false, OnContextExplorerActiveItem, item);
            menu.AddSeparator(String.Empty);
            AddContextMoveComm(menu);
            menu.AddItem(AssetDanshariStyle.Get().dependenciesTitle, false, OnContextFindDependenciesActiveItem, item);
            menu.AddSeparator(String.Empty);
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextOnlyUseThis, false, OnContextUseThisItem, item);
            menu.ShowAsContext();
        }

        private void OnContextUseThisItem(object userdata)
        {
            if (userdata is not AssetTreeViewItem<AssetTreeModel.AssetInfo> item) return;
            if (item.parent is AssetTreeViewItem<AssetTreeModel.AssetInfo> itemParent)
                Model.SetUseThis(itemParent.Data, item.Data);
            Repaint();
        }

        private static void OnContextFindDependenciesActiveItem(object userdata)
        {
            if (userdata is not AssetTreeViewItem<AssetTreeModel.AssetInfo> item) return;
            if (AssetDanshariHandler.OnDependenciesFindItem != null)
            {
                AssetDanshariHandler.OnDependenciesFindItem(item.Data.FileRelativePath);
            }
        }

        #region 数据变化

        protected override bool OnWatcherImportedAssetsEvent(string[] importedAssets, bool resortItem)
        {
            return base.OnWatcherImportedAssetsEvent(importedAssets, false);
        }

        protected override bool OnWatcherMovedAssetsEvent(string[] movedFromAssetPaths, string[] movedAssets,
            bool resortItem)
        {
            return base.OnWatcherMovedAssetsEvent(movedFromAssetPaths, movedAssets, false);
        }

        #endregion
    }
}
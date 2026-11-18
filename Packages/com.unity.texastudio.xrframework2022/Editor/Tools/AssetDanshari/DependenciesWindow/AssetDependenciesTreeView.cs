using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetDependenciesTreeView : AssetTreeView
    {
        private AssetDependenciesTreeModel Model { get; set; }

        public AssetDependenciesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader, model)
        {
            Model = model as AssetDependenciesTreeModel;
            AssetDanshariHandler.OnDependenciesFindItem += OnDependenciesFindItem;
        }

        public override void Destroy()
        {
            base.Destroy();
            AssetDanshariHandler.OnDependenciesFindItem -= OnDependenciesFindItem;
        }

        protected override void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeModel.AssetInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.Data;

            switch (column)
            {
                case 0:
                    if (!info.IsExtra)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.DisplayName, info.FileRelativePath, info.Deleted, info.Added);
                    }
                    break;
                case 1:
                    if (info.IsExtra)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.DisplayName, info.FileRelativePath, info.Deleted, info.Added, false);
                    }
                    else
                    {
                        if (!info.IsFolder && info.HasChildren && info.Children.Count > 0)
                        {
                            DefaultGUI.Label(cellRect, info.Children.Count.ToString(), args.selected, args.focused);
                        }
                    }
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            var assetInfo = GetItemAssetInfo(item);
            if (item == null || assetInfo == null || assetInfo.Deleted)
            {
                return;
            }

            var menu = new GenericMenu();
            if (!IsSelectionMulti())
            {
                menu.AddItem(AssetDanshariStyle.Get().locationContext, false, OnContextSetActiveItem, id);
                menu.AddItem(AssetDanshariStyle.Get().explorerContext, false, OnContextExplorerActiveItem, item);
            }

            if (!IsSelectionContainsReverseItem())
            {
                menu.AddSeparator(String.Empty);
                AddContextMoveComm(menu);
                menu.AddSeparator(String.Empty);
                menu.AddItem(AssetDanshariStyle.Get().dependenciesDelete, false, OnContextDeleteThisItem);

                if (AssetDanshariHandler.OnDependenciesContextDraw != null)
                {
                    AssetDanshariHandler.OnDependenciesContextDraw(menu);
                }
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
        }

        private void OnContextDeleteThisItem()
        {
            if (!HasSelection())
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.dependenciesDelete.text,
                style.sureStr, style.cancelStr))
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

                if (AssetDatabase.DeleteAsset(assetInfo.FileRelativePath))
                {
                    assetInfo.Deleted = true;
                }
            }
            Repaint();
        }

        public void SetFilterEmpty(bool filterEmpty)
        {
            Reload();
            if (filterEmpty)
            {
                // 剔除有被引用的
                SetFilterEmptyStay(rootItem);
            }
            ForceRefresh();
            Repaint();
        }

        private void SetFilterEmptyStay(TreeViewItem item)
        {
            var assetInfo = GetItemAssetInfo(item);
            if (assetInfo == null)
            {
                if (!item.hasChildren) return;
                for (var i = item.children.Count - 1; i >= 0; i--)
                {
                    var child = item.children[i];
                    SetFilterEmptyStay(child);
                }
                return;
            }

            switch (assetInfo.IsFolder)
            {
                case false when assetInfo.HasChildren && assetInfo.Children.Count > 0:
                    item.parent.children.Remove(item);
                    item.parent = null;
                    return;
                case true when item.hasChildren:
                {
                    for (var i = item.children.Count - 1; i >= 0; i--)
                    {
                        var child = item.children[i];
                        SetFilterEmptyStay(child);
                    }

                    break;
                }
            }
        }

        private void OnDependenciesFindItem(string assetPath)
        {
            var item = FindItemByAssetPath(rootItem, assetPath);
            if (item == null) return;
            SetSelection(new List<int>() { item.id }, TreeViewSelectionOptions.RevealAndFrame);
            Repaint();
        }
    }
}
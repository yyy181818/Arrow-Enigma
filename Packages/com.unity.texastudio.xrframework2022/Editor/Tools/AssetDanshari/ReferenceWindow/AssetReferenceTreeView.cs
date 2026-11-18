using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetReferenceTreeView : AssetTreeView
    {
        private AssetReferenceTreeModel Model { get; set; }

        public AssetReferenceTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader, model)
        {
            Model = model as AssetReferenceTreeModel;
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
                case 2:
                    if (info.IsExtra)
                    {
                        DefaultGUI.Label(cellRect, info.BindObj as string, args.selected, args.focused);
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

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
        }
    }
}
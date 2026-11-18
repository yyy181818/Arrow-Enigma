using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariStyle
    {
        public class Style
        {
            public GUIContent help = new("帮助");
            public GUIContent about = new("关于");
            public GUIContent exportCsv = new("导出 CSV");
            public GUIContent forceText = new("Asset Serialization must be ForceText");
            public GUIContent assetReferenceTitle = new("检查列表");
            public GUIContent assetReferenceAsset = new("资源目录", "存放资源的文件夹路径");
            public GUIContent assetReferenceAssetCommon = new("公共资源目录", "整理资源时所放置的公共文件夹路径");
            public GUIContent assetReferenceReference = new("引用目录", "使用到资源的预制文件夹路径");
            public GUIContent assetReferenceCheckDup = new("检查重复");
            public GUIContent assetReferenceCheckRef = new("引用查找");
            public GUIContent assetReferenceDepend = new("被引用查找");

            public GUIContent duplicateTitle = new("重复资源检查");
            public GUIContent duplicateWaiting = new("等待进行检查重复资源");
            public GUIContent duplicateNothing = new("所检查的文件夹没有重复资源");
            public GUIContent duplicateManualAdd = new("手动添加");
            public GUIContent duplicateHeaderContent = new("名称");
            public GUIContent duplicateHeaderContent2 = new("路径");
            public GUIContent duplicateHeaderContent3 = new("大小");
            public GUIContent duplicateHeaderContent4 = new("创建时间");
            public GUIContent duplicateContextOnlyUseThis = new("仅使用此资源，其余删除");
            public string duplicateContextMoveComm = "移入公共目录/";
            public string duplicateGroup = "文件数：{0}";

            public GUIContent dependenciesTitle = new("资源被引用查找");
            public GUIContent dependenciesWaiting = new("等待进行资源被引用查找");
            public GUIContent dependenciesNothing = new("资源文件夹没有任何数据");
            public GUIContent dependenciesHeaderContent2 = new("被引用的路径");
            public GUIContent dependenciesDelete = new("删除选中资源");
            public GUIContent dependenciesFilter = new("过滤为空");

            public GUIContent referenceTitle = new("引用查找");
            public GUIContent referenceWaiting = new("等待进行引用查找");
            public GUIContent referenceNothing = new("文件夹没有任何数据");
            public GUIContent referenceHeaderContent2 = new("引用的资源");
            public GUIContent referenceHeaderContent3 = new("隶属");

            public GUIContent iconDelete = EditorGUIUtility.IconContent("AS Badge Delete");
            public GUIContent iconNew = EditorGUIUtility.IconContent("AS Badge New");

            public string progressTitle = "正在处理";
            public string errorTitle = "错误信息";
            public string continueStr = "继续执行";
            public string cancelStr = "取消";
            public string sureStr = "确定";
            public string progressFinish = "处理结束";
            public string deleteFile = "正在删除文件...";
            public GUIContent expandAll = new("展开");
            public GUIContent expandAll2 = new("全部展开");
            public GUIContent expandAll3 = new("全部展开除最后一层");
            public GUIContent collapseAll = new("折叠");
            public GUIContent collapseAll2 = new("全部折叠");
            public GUIContent collapseAll3 = new("仅折叠最后一层");
            public GUIContent locationContext = new("定位");
            public GUIContent explorerContext = new("打开所在文件夹");
            public GUIContent nameHeaderContent = new("名称");
            public Texture2D folderIcon = EditorGUIUtility.FindTexture("Folder Icon");

            public GUIStyle labelStyle;

            public void InitGUI()
            {
                if (labelStyle != null) return;
                labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
            }
        }

        private static Style _sStyle;

        public static Style Get()
        {
            return _sStyle ??= new Style();
        }
    }
}
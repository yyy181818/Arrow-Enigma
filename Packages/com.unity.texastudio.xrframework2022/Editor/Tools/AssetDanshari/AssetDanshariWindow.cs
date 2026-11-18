using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariWindow : EditorWindow
    {
        [MenuItem("Tools/资源相关/资源断舍离")]
        private static void ShowWindow()
        {
            GetWindow<AssetDanshariWindow>();
        }

        private AssetDanshariSetting _mAssetDanshariSetting;
        private Vector2 _mScrollViewVector2;
        private ReorderableList _mReorderableList;
        private bool _mIsForceText;
        private AssetDanshariHandlerDemo _mAssetDanshariHandlerDemo;

        private void Awake()
        {
            titleContent.text = "资源断舍离";
        }

        private void OnGUI()
        {
            Init();
            var style = AssetDanshariStyle.Get();

            if (!_mIsForceText)
            {
                EditorGUILayout.LabelField(style.forceText);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            var toolBtnRect = GUILayoutUtility.GetRect(style.help, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
            if (GUI.Button(toolBtnRect, style.help, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(style.about, false, OnContextAbout);
                menu.DropDown(toolBtnRect);
            }
            EditorGUILayout.EndHorizontal();

            if (_mReorderableList == null) return;
            _mScrollViewVector2 = GUILayout.BeginScrollView(_mScrollViewVector2);
            _mReorderableList.DoLayoutList();
            GUILayout.EndScrollView();
        }

        private void Init()
        {
            if (_mAssetDanshariSetting is null)
            {
                _mIsForceText = EditorSettings.serializationMode == SerializationMode.ForceText;
                if (!_mIsForceText)
                {
                    return;
                }

                _mAssetDanshariSetting = AssetDatabase.LoadAssetAtPath<AssetDanshariSetting>("Assets/Editor/Tools/AssetDanshari/AssetDanshariSetting.asset");
                if (_mAssetDanshariSetting is null)
                {
                    var guids = AssetDatabase.FindAssets("t:" + nameof(AssetDanshariSetting));
                    if (guids.Length == 0)
                    {
                        Debug.LogError("Missing AssetDanshariSetting File");
                        return;
                    }

                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _mAssetDanshariSetting = AssetDatabase.LoadAssetAtPath<AssetDanshariSetting>(path);
                }
            }

            if (_mReorderableList == null)
            {
                _mReorderableList = new ReorderableList(_mAssetDanshariSetting.AssetReferenceInfos, null, true, true, true, true)
                    {
                        drawHeaderCallback = OnDrawHeaderCallback,
                        drawElementCallback = OnDrawElementCallback
                    };
                _mReorderableList.elementHeight += 55;
            }

            _mAssetDanshariHandlerDemo ??= new AssetDanshariHandlerDemo();
        }

        private static void OnDrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, AssetDanshariStyle.Get().assetReferenceTitle);
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (_mAssetDanshariSetting == null || _mAssetDanshariSetting.AssetReferenceInfos.Count < index)
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var info = _mAssetDanshariSetting.AssetReferenceInfos[index];
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            EditorGUI.BeginChangeCheck();
            info.title = EditorGUI.TextField(rect, info.title);
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            var rect2 = new Rect(rect) { width = 50f };
            var rect3 = new Rect(rect) { x = rect2.x + rect2.width, width = rect.width - rect2.width - 150f };
            var rect4 = new Rect(rect) { x = rect3.x + rect3.width + 5f, width = 70f };
            var rect5 = new Rect(rect) { x = rect4.x + rect4.width + 5f, width = 70f };
            EditorGUI.LabelField(rect2, style.assetReferenceReference);
            info.referenceFolder = EditorGUI.TextField(rect3, info.referenceFolder);
            info.referenceFolder = OnDrawElementAcceptDrop(rect3, info.referenceFolder);
            var valueChanged = EditorGUI.EndChangeCheck();
            if (GUI.Button(rect4, style.assetReferenceCheckRef))
            {
                AssetBaseWindow.CheckPaths<AssetReferenceWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder);
            }

            rect2.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.y += EditorGUIUtility.singleLineHeight + 2;
            rect4.y += EditorGUIUtility.singleLineHeight + 2;
            rect5.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.LabelField(rect2, style.assetReferenceAsset);
            EditorGUI.BeginChangeCheck();
            info.assetFolder = EditorGUI.TextField(rect3, info.assetFolder);
            info.assetFolder = OnDrawElementAcceptDrop(rect3, info.assetFolder);
            valueChanged |= EditorGUI.EndChangeCheck();
            if (GUI.Button(rect4, style.assetReferenceCheckDup))
            {
                AssetBaseWindow.CheckPaths<AssetDuplicateWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder);
            }
            if (GUI.Button(rect5, style.assetReferenceDepend))
            {
                AssetBaseWindow.CheckPaths<AssetDependenciesWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder);
            }

            rect2.width += 25f;
            rect2.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.x += 25f;
            rect3.width = rect.width - rect2.width;
            EditorGUI.LabelField(rect2, style.assetReferenceAssetCommon);
            EditorGUI.BeginChangeCheck();
            info.assetCommonFolder = EditorGUI.TextField(rect3, info.assetCommonFolder);
            info.assetCommonFolder = OnDrawElementAcceptDrop(rect3, info.assetCommonFolder);
            valueChanged |= EditorGUI.EndChangeCheck();

            if (!valueChanged) return;
            EditorUtility.SetDirty(_mAssetDanshariSetting);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private static string OnDrawElementAcceptDrop(Rect rect, string label)
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
            GUI.changed = true;

            return AssetDanshariUtility.PathArrayToStr(DragAndDrop.paths);

        }

        private static void OnContextAbout()
        {
            Application.OpenURL("https://github.com/akof1314/UnityAssetDanshari");
        }
    }
}
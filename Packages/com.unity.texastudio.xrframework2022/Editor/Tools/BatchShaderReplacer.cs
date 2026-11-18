using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchShaderReplacer : EditorWindow
{
    private GameObject targetRoot;
    private Shader targetShader;
    private Material tempMaterial; // 模板材质
    private MaterialEditor materialEditor;
    private Vector2 scrollPos;

    // --- 新增选项：是否保留原材质的引用 ---
    private bool keepOriginalProperties = true;

    [MenuItem("Tools/批量 Shader 替换工具")]
    public static void ShowWindow()
    {
        GetWindow<BatchShaderReplacer>("批量 Shader 替换");
    }

    private void OnDisable()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        if (tempMaterial != null) DestroyImmediate(tempMaterial);
        if (materialEditor != null) DestroyImmediate(materialEditor);
    }

    private void OnGUI()
    {
        GUILayout.Label("批量 Shader 替换与参数调整", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 选择父物体
        targetRoot = (GameObject)EditorGUILayout.ObjectField("根物体 (父节点)", targetRoot, typeof(GameObject), true);

        // 2. 选择目标 Shader
        EditorGUI.BeginChangeCheck();
        targetShader = (Shader)EditorGUILayout.ObjectField("目标 Shader", targetShader, typeof(Shader), false);
        if (EditorGUI.EndChangeCheck())
        {
            CreateTempMaterial();
        }

        // --- 新增 UI：保留属性开关 ---
        EditorGUILayout.Space();
        keepOriginalProperties = EditorGUILayout.ToggleLeft("保留原材质的同名属性 (贴图/颜色)", keepOriginalProperties);
        if (keepOriginalProperties)
        {
            EditorGUILayout.HelpBox("开启后：脚本会尝试匹配旧材质的属性（支持 _MainTex -> _BaseMap 映射）。\n未匹配到的属性将使用下方模板的参数。",
                MessageType.Info);
        }

        EditorGUILayout.Space();

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        // 3. 显示 Shader 属性并调整
        if (targetShader != null)
        {
            if (tempMaterial == null) CreateTempMaterial();

            GUILayout.Label("设定统一模板参数:", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (materialEditor != null)
            {
                EditorGUI.BeginChangeCheck();

                // --- 使用您确认有效的绘制方式 ---
                MaterialProperty[] props =
                    MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { tempMaterial });

                foreach (var prop in props)
                {
                    // 如果属性被标记为隐藏，则跳过
                    if ((prop.flags & (MaterialProperty.PropFlags.HideInInspector |
                                       MaterialProperty.PropFlags.PerRendererData)) != 0)
                        continue;

                    // 计算高度并绘制
                    float height = materialEditor.GetPropertyHeight(prop, prop.displayName);
                    Rect r = EditorGUILayout.GetControlRect(true, height);
                    materialEditor.ShaderProperty(r, prop, prop.displayName);
                }
                // -----------------------------

                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Material Change");
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 4. 执行替换
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("执行替换并应用参数", GUILayout.Height(40)))
            {
                ApplyShader();
            }

            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("请先指定一个目标 Shader。", MessageType.Info);
        }
    }

    private void CreateTempMaterial()
    {
        CleanUp();

        if (targetShader == null) return;

        tempMaterial = new Material(targetShader);
        tempMaterial.hideFlags = HideFlags.DontSave;

        materialEditor = (MaterialEditor)Editor.CreateEditor(tempMaterial);
    }

    private void ApplyShader()
    {
        if (targetRoot == null)
        {
            EditorUtility.DisplayDialog("错误", "请先指定根物体！", "OK");
            return;
        }

        if (targetShader == null || tempMaterial == null)
        {
            EditorUtility.DisplayDialog("错误", "Shader 数据无效！", "OK");
            return;
        }

        Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "该物体下没有找到 Renderer 组件。", "OK");
            return;
        }

        Undo.RecordObjects(renderers, "Batch Shader Replace");

        int count = 0;
        HashSet<Material> uniqueMaterials = new HashSet<Material>();

        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null) uniqueMaterials.Add(mat);
            }
        }

        foreach (var mat in uniqueMaterials)
        {
            Undo.RecordObject(mat, "Replace Shader");

            // ================= 智能保留逻辑开始 =================
            // 字典用于暂存旧属性值
            Dictionary<string, Texture> oldTextures = new Dictionary<string, Texture>();
            Dictionary<string, Color> oldColors = new Dictionary<string, Color>();
            Dictionary<string, float> oldFloats = new Dictionary<string, float>();
            Dictionary<string, Vector4> oldVectors = new Dictionary<string, Vector4>();

            // 1. 如果勾选了保留，先读取旧 Shader 的所有属性
            if (keepOriginalProperties)
            {
                Shader oldShader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(oldShader);

                for (int i = 0; i < propertyCount; i++)
                {
                    string propName = ShaderUtil.GetPropertyName(oldShader, i);
                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(oldShader, i);

                    if (mat.HasProperty(propName))
                    {
                        switch (type)
                        {
                            case ShaderUtil.ShaderPropertyType.TexEnv:
                                oldTextures[propName] = mat.GetTexture(propName);
                                break;
                            case ShaderUtil.ShaderPropertyType.Color:
                                oldColors[propName] = mat.GetColor(propName);
                                break;
                            case ShaderUtil.ShaderPropertyType.Float:
                            case ShaderUtil.ShaderPropertyType.Range:
                                oldFloats[propName] = mat.GetFloat(propName);
                                break;
                            case ShaderUtil.ShaderPropertyType.Vector:
                                oldVectors[propName] = mat.GetVector(propName);
                                break;
                        }
                    }
                }
            }
            // =================================================

            // 2. 切换 Shader
            mat.shader = targetShader;

            // 3. 应用面板上的统一模板参数 (覆盖一切)
            mat.CopyPropertiesFromMaterial(tempMaterial);

            // 4. 复制 Keywords
            foreach (string keyword in tempMaterial.shaderKeywords)
            {
                mat.EnableKeyword(keyword);
            }

            // ================= 智能恢复逻辑开始 =================
            // 5. 将刚才存下来的旧属性，根据名字匹配，覆盖回去
            if (keepOriginalProperties)
            {
                int newPropCount = ShaderUtil.GetPropertyCount(targetShader);
                for (int i = 0; i < newPropCount; i++)
                {
                    string propName = ShaderUtil.GetPropertyName(targetShader, i);
                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(targetShader, i);

                    // 获取可能的旧名字列表 (处理 _MainTex <-> _BaseMap 等映射)
                    string[] possibleOldNames = GetPossibleOriginalNames(propName);

                    foreach (var oldName in possibleOldNames)
                    {
                        bool restored = false;
                        switch (type)
                        {
                            case ShaderUtil.ShaderPropertyType.TexEnv:
                                if (oldTextures.ContainsKey(oldName) && oldTextures[oldName] != null)
                                {
                                    mat.SetTexture(propName, oldTextures[oldName]);
                                    restored = true;
                                }

                                break;
                            case ShaderUtil.ShaderPropertyType.Color:
                                if (oldColors.ContainsKey(oldName))
                                {
                                    mat.SetColor(propName, oldColors[oldName]);
                                    restored = true;
                                }

                                break;
                            case ShaderUtil.ShaderPropertyType.Float:
                            case ShaderUtil.ShaderPropertyType.Range:
                                if (oldFloats.ContainsKey(oldName))
                                {
                                    mat.SetFloat(propName, oldFloats[oldName]);
                                    restored = true;
                                }

                                break;
                            case ShaderUtil.ShaderPropertyType.Vector:
                                if (oldVectors.ContainsKey(oldName))
                                {
                                    mat.SetVector(propName, oldVectors[oldName]);
                                    restored = true;
                                }

                                break;
                        }

                        if (restored) break; // 只要有一个旧名字匹配成功，就停止查找
                    }
                }
            }
            // =================================================

            count++;
        }

        Debug.Log($"<color=green>成功替换了 {count} 个材质的 Shader 并应用了参数。</color>");
        AssetDatabase.SaveAssets();
    }

    // 辅助：名称映射表
    private string[] GetPossibleOriginalNames(string newName)
    {
        List<string> names = new List<string> { newName };

        // 常用映射规则 (URP <-> Standard)
        if (newName == "_BaseMap") names.Add("_MainTex");
        if (newName == "_MainTex") names.Add("_BaseMap");
        if (newName == "_BaseColor") names.Add("_Color");
        if (newName == "_Color") names.Add("_BaseColor");
        if (newName == "_EmissionMap") names.Add("_Emission");
        if (newName == "_Brightness") names.Add("_Brightness");
        return names.ToArray();
    }
}
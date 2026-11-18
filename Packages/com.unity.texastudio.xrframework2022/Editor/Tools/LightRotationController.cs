#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


/// <summary>
/// Unity编辑器工具：用于方便控制场景中任意GameObject的旋转
/// 支持鼠标拖拽和滑块控制对象角度，特别适用于光源控制
/// </summary>
public class LightRotationController : EditorWindow
{
    #region 常量定义

    private const float MIN_PITCH_ANGLE = -90f;
    private const float MAX_PITCH_ANGLE = 90f;
    private const float DEFAULT_ANGLE = 45f;
    private const string WINDOW_TITLE = "(Light) Rotation Controller Tool";
    private const string MENU_PATH = "Tools/LightRotationController";

    #endregion

    #region 静态实例

    public static LightRotationController window;

    #endregion

    #region 私有字段

    private GameObject targetObject;
    private GameObject[] sceneObjects;
    private float angle = DEFAULT_ANGLE;
    private bool lockPitch = true;

    #endregion


    #region Unity菜单项

    [MenuItem(MENU_PATH)]
    public static void ShowWindow()
    {
        window = GetWindow<LightRotationController>(WINDOW_TITLE, true);
        window.minSize = new Vector2(300, 150);
    }

    #endregion

    #region Unity生命周期

    protected virtual void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        UnsubscribeFromSceneEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromSceneEvents();
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 初始化工具，订阅场景事件并尝试查找平行光
    /// </summary>
    private void Initialize()
    {
        SubscribeToSceneEvents();

        // 第一次打开时自动尝试查找平行光
        if (targetObject == null)
        {
            FindDirectionalLight();
        }

        // 同步角度值
        SyncAngleFromTarget();
    }

    /// <summary>
    /// 订阅场景GUI事件
    /// </summary>
    private void SubscribeToSceneEvents()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    /// <summary>
    /// 取消订阅场景GUI事件
    /// </summary>
    private void UnsubscribeFromSceneEvents()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    /// <summary>
    /// 从目标对象同步角度值
    /// </summary>
    private void SyncAngleFromTarget()
    {
        if (targetObject != null)
        {
            angle = NormalizeAngle(targetObject.transform.localEulerAngles.x);
        }
    }

    #endregion

    #region 对象查找方法

    /// <summary>
    /// 查找场景中的平行光作为默认目标
    /// 优先选择最后创建的平行光
    /// </summary>
    public void FindDirectionalLight()
    {
        Light[] lights = FindObjectsOfType<Light>();

        // 从后往前遍历，获取最后创建的平行光
        for (int i = lights.Length - 1; i >= 0; i--)
        {
            if (lights[i].type == LightType.Directional)
            {
                targetObject = lights[i].gameObject;
                SyncAngleFromTarget();
                break;
            }
        }
    }

    /// <summary>
    /// 查找场景中所有GameObject（用于快速选择）
    /// </summary>
    public void RefreshSceneObjects()
    {
        sceneObjects = FindObjectsOfType<GameObject>();
    }

    /// <summary>
    /// 验证当前目标对象是否有效
    /// </summary>
    private bool IsValidTarget()
    {
        return targetObject != null;
    }

    /// <summary>
    /// 检查对象是否为平行光
    /// </summary>
    private bool IsDirectionalLight(GameObject obj)
    {
        if (obj == null) return false;
        Light lightComponent = obj.GetComponent<Light>();
        return lightComponent != null && lightComponent.type == LightType.Directional;
    }

    #endregion

    #region 角度处理方法

    /// <summary>
    /// 标准化角度到-180到180度范围内
    /// </summary>
    /// <param name="inputAngle">输入角度</param>
    /// <returns>标准化后的角度</returns>
    private float NormalizeAngle(float inputAngle)
    {
        while (inputAngle > 180f) inputAngle -= 360f;
        while (inputAngle < -180f) inputAngle += 360f;
        return inputAngle;
    }

    /// <summary>
    /// 标准化Vector3角度
    /// </summary>
    /// <param name="angles">输入角度向量</param>
    /// <returns>标准化后的角度向量</returns>
    private Vector3 NormalizeAngles(Vector3 angles)
    {
        return new Vector3(
            NormalizeAngle(angles.x),
            NormalizeAngle(angles.y),
            NormalizeAngle(angles.z)
        );
    }

    #endregion

    #region 场景GUI处理

    /// <summary>
    /// 处理场景视图中的GUI事件，支持Ctrl+鼠标拖拽控制对象旋转
    /// </summary>
    /// <param name="sceneView">场景视图</param>
    public void OnSceneGUI(SceneView sceneView)
    {
        if (!IsValidTarget())
        {
            return;
        }

        Event currentEvent = Event.current;

        // 检查是否按下Ctrl键
        if (IsControlKeyPressed(currentEvent))
        {
            HandleMouseDragRotation(currentEvent);
        }
    }

    /// <summary>
    /// 检查是否按下了Ctrl键
    /// </summary>
    /// <param name="evt">当前事件</param>
    /// <returns>是否按下Ctrl键</returns>
    private bool IsControlKeyPressed(Event evt)
    {
        return (evt.modifiers & EventModifiers.Control) != 0;
    }

    /// <summary>
    /// 处理鼠标拖拽旋转逻辑
    /// </summary>
    /// <param name="evt">当前事件</param>
    private void HandleMouseDragRotation(Event evt)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (evt.type == EventType.MouseDrag && evt.button == 0)
        {
            // 水平旋转（绕世界Y轴）
            targetObject.transform.Rotate(0, -evt.delta.x, 0, Space.World);

            // 垂直旋转（仅在未锁定俯仰角时）
            if (!lockPitch)
            {
                targetObject.transform.Rotate(-evt.delta.y, 0, 0, Space.Self);
                angle = NormalizeAngle(targetObject.transform.localEulerAngles.x);
            }

            // 无论是否锁定俯仰角，都需要刷新GUI显示
            Repaint();
        }
    }

    #endregion

    #region GUI界面

    /// <summary>
    /// 绘制工具窗口的GUI界面
    /// </summary>
    private void OnGUI()
    {
        DrawTitle();
        EditorGUILayout.Space(5);
        DrawTargetObjectField();

        if (targetObject == null)
        {
            DrawErrorMessage();
        }
        else
        {
            DrawRotationControls();
        }

        EditorGUILayout.Space(5);
        DrawInstructions();
    }

    /// <summary>
    /// 绘制窗口标题
    /// </summary>
    private void DrawTitle()
    {
        EditorGUILayout.Space(5);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.LabelField("（灯光）旋转控制工具", titleStyle);

        // 添加分隔线
        EditorGUILayout.Space(3);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    /// <summary>
    /// 绘制目标对象字段
    /// </summary>
    private void DrawTargetObjectField()
    {
        EditorGUI.BeginChangeCheck();
        targetObject =
            (GameObject)EditorGUILayout.ObjectField("目标对象 (Target Object)", targetObject, typeof(GameObject), true);

        if (EditorGUI.EndChangeCheck())
        {
            if (targetObject != null)
            {
                SyncAngleFromTarget();

                // 如果是平行光，给出提示
                if (IsDirectionalLight(targetObject))
                {
                    EditorGUILayout.HelpBox("✓ 检测到平行光源，非常适合光照控制！", MessageType.Info);
                }
            }
        }
    }


    /// <summary>
    /// 绘制错误信息和查找按钮
    /// </summary>
    private void DrawErrorMessage()
    {
        GUI.backgroundColor = Color.yellow;
        EditorGUILayout.HelpBox("未选择目标对象，请手动设置或点击下方按钮自动查找场景中的平行光。", MessageType.Warning);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("  查找平行光"))
        {
            FindDirectionalLight();
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("提示", "场景中未找到平行光源，请手动选择其他对象或创建平行光。", "确定");
            }
        }

        if (GUILayout.Button("  刷新场景对象"))
        {
            RefreshSceneObjects();
            EditorUtility.DisplayDialog("提示", $"已刷新场景对象列表，共找到 {sceneObjects?.Length ?? 0} 个对象。", "确定");
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制旋转控制界面
    /// </summary>
    private void DrawRotationControls()
    {
        Vector3 currentAngles = NormalizeAngles(targetObject.transform.localEulerAngles);

        EditorGUILayout.Space(5);

        // 显示对象信息
        DrawObjectInfo();

        EditorGUILayout.Space(3);

        // 锁定俯仰角选项
        lockPitch = EditorGUILayout.Toggle("锁定俯仰角", lockPitch);

        EditorGUILayout.Space(3);

        // 俯仰角滑块
        EditorGUI.BeginChangeCheck();
        angle = EditorGUILayout.Slider("俯仰角 (Pitch)", angle, MIN_PITCH_ANGLE, MAX_PITCH_ANGLE);

        if (EditorGUI.EndChangeCheck())
        {
            ApplyAngleToTarget(currentAngles);
        }

        // 显示当前角度信息
        EditorGUILayout.Space(3);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector3Field("当前旋转角度", currentAngles);
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// 绘制对象信息
    /// </summary>
    private void DrawObjectInfo()
    {
        string objectType = "GameObject";
        Color infoColor = Color.white;

        if (IsDirectionalLight(targetObject))
        {
            objectType = "平行光 (Directional Light)";
            infoColor = Color.green;
        }
        else if (targetObject.GetComponent<Light>() != null)
        {
            Light light = targetObject.GetComponent<Light>();
            objectType = $"{light.type} Light";
            infoColor = Color.cyan;
        }

        GUI.color = infoColor;
        EditorGUILayout.LabelField("对象类型:", objectType, EditorStyles.boldLabel);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 应用角度到目标对象
    /// </summary>
    /// <param name="currentAngles">当前角度</param>
    private void ApplyAngleToTarget(Vector3 currentAngles)
    {
        if (targetObject != null)
        {
            targetObject.transform.rotation = Quaternion.Euler(angle, currentAngles.y, currentAngles.z);
        }
    }

    /// <summary>
    /// 绘制使用说明
    /// </summary>
    private void DrawInstructions()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("使用说明:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• 选择任意GameObject作为旋转目标\n" +
            "• 在场景视图中按住 Ctrl + 鼠标左键拖拽可旋转对象\n" +
            "• 水平拖拽控制方位角（绕Y轴旋转）\n" +
            "• 垂直拖拽控制俯仰角（绕X轴旋转，需解锁俯仰角）",
            MessageType.Info);
    }

    #endregion
}
#endif
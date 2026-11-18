using UnityEditor;
using UnityEngine;

internal class LightDir : MaterialPropertyDrawer
{
    private float _height = 16;
    private bool _isEditor;
    private bool _starEditor = true;
    private GameObject _selectGameObj;
    private Quaternion _rot = Quaternion.identity;

    private MaterialProperty _mProp;

    //判断是否为Vector类型
    private static bool IsPropertyTypeSuitable(MaterialProperty prop)
    {
        return prop.type == MaterialProperty.PropType.Vector;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //如果不是Vector类型，则把unity的默认警告框的高度40
        if (!IsPropertyTypeSuitable(prop))
        {
            return 40f;
        }

        _height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, new GUIContent(label));
        return _height;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //如果不是Vector类型，则显示一个警告框
        if (!IsPropertyTypeSuitable(prop))
        {
            var c = EditorGUIUtility.TrTextContent("LightDir used on a non-Vector property: " + prop.name,
                EditorGUIUtility.IconContent("console.erroricon").image);
            EditorGUI.LabelField(position, c, EditorStyles.helpBox);
            return;
        }

        EditorGUI.BeginChangeCheck();

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 0f;

        var oldColor = GUI.color;
        if (_isEditor) GUI.color = Color.green;

        //绘制属性
        var vectorRect = new Rect(position)
        {
            width = position.width - 68f
        };
        var value = new Vector3(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z);
        value = EditorGUI.Vector3Field(vectorRect, label, value);

        //绘制开关
        var toggleRect = new Rect(position)
        {
            x = position.xMax - 64f,
            y = (_height > 16) ? position.y + 16 : position.y,
            width = 60f,
            height = 16
        };
        _isEditor = GUI.Toggle(toggleRect, _isEditor, "Set", "Button");
        if (_isEditor)
        {
            if (_starEditor)
            {
                _mProp = prop;
                InitSceneGUI(value);
            }
        }
        else
        {
            if (!_starEditor)
            {
                ClearSceneGUI();
            }
        }

        GUI.color = oldColor;
        EditorGUIUtility.labelWidth = oldLabelWidth;
        if (EditorGUI.EndChangeCheck())
        {
            prop.vectorValue = new Vector4(value.x, value.y, value.z);
        }
    }

    private void InitSceneGUI(Vector3 value)
    {
        Tools.current = Tool.None;
        _selectGameObj = Selection.activeGameObject;
        var worldDir = _selectGameObj.transform.rotation * value;
        _rot = Quaternion.FromToRotation(Vector3.forward, worldDir);
        SceneView.duringSceneGui += OnSceneGUI;
        _starEditor = false;
    }

    private void ClearSceneGUI()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        _mProp = null;
        _selectGameObj = null;
        _starEditor = true;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject != _selectGameObj)
        {
            ClearSceneGUI();
            _isEditor = false;
            return;
        }

        var pos = _selectGameObj.transform.position;

        _rot = Handles.RotationHandle(_rot, pos);
        var newLocalDir = Quaternion.Inverse(_selectGameObj.transform.rotation) * _rot * Vector3.forward;

        _mProp.vectorValue = new Vector4(newLocalDir.x, newLocalDir.y, newLocalDir.z);

        Handles.color = Color.green;
        Handles.ConeHandleCap(0, pos, _rot, HandleUtility.GetHandleSize(pos), EventType.Repaint);
    }
}
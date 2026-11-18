using System.Drawing;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor
{
    public enum LitExpandable
    {
        SurfaceOptions = 1 << 0,
        Tessellation = 1 << 1,
        SurfaceInputs = 1 << 2,
        MainLayer = 1 << 3,
        Layer1 = 1 << 4,
        Layer2 = 1 << 5,
        Layer3 = 1 << 6,
        Transparency = 1 << 7,
        PlanarShadow = 1 << 8,
        Emission = 1 << 9,
        Advanced = 1 << 10,
    }
    enum WorkflowMode
    {
        Specular,
        Metallic
    }
    enum SurfaceType
    {
        Opaque,
        Transparent
    }
    enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }
    enum MaterialType 
    {
		SubsurfaceScattering,
        Standard,
        Anisotropy,
        Iridescence,
        Translucency
    }
    enum FabricMaterialType 
    {
		CottonWool,
        Silk
    }
    enum DisplacementMode 
    {
		None,
        VertexDisplacement,
        PixelDisplacement,
        Tessellation
    }
    enum TessellationMode 
    {
		None,
        EdgeLength,
        Distance
    }
    enum HeightParametrization
    {
        Amplitude,
        MinMax
    }
    enum RenderFace
    {
        Front = 2,
        Back = 1,
        Both = 0
    }
    enum DoubleSidedNormalMode
    {
        Mirror,
        Flip,
        None
    }
    enum SpecularOcclusionMode
    {
        Off,
        FromAmbientOcclusion,
        FromBentNormals,
        FromGI
    }
    public enum VertexColorMode
    {
        None,
        Multiply,
        Add
    }
    
    public static partial class MaterialEditorExtension
    {
        static Rect GetRect(MaterialProperty prop)
        {
            return EditorGUILayout.GetControlRect(true, MaterialEditor.GetDefaultPropertyHeight(prop), EditorStyles.layerMaskField);
        }
        
        public static void MinMaxShaderProperty(this MaterialEditor editor, MaterialProperty min, MaterialProperty max, float minLimit, float maxLimit, GUIContent label)
        {
            float minValue = min.floatValue;
            float maxValue = max.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.MinMaxSlider(label, ref minValue, ref maxValue, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
            {
                min.floatValue = minValue;
                max.floatValue = maxValue;
            }
        }

        public static void MinMaxShaderPropertyXY(this MaterialEditor editor, MaterialProperty remapProp, float minLimit, float maxLimit, GUIContent label)
        {
            Vector4 remap = remapProp.vectorValue;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.MinMaxSlider(label, ref remap.x, ref remap.y, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
                remapProp.vectorValue = remap;
        }

        public static void MinMaxShaderPropertyZW(this MaterialEditor editor, MaterialProperty remapProp, float minLimit, float maxLimit, GUIContent label)
        {
            Vector4 remap = remapProp.vectorValue;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.MinMaxSlider(label, ref remap.z, ref remap.w, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
                remapProp.vectorValue = remap;
        }

        public static void IntSliderShaderProperty(this MaterialEditor editor, MaterialProperty prop, GUIContent label)
        {
            var limits = prop.rangeLimits;
            editor.IntSliderShaderProperty(prop, (int)limits.x, (int)limits.y, label);
        }

        public static void IntSliderShaderProperty(this MaterialEditor editor, MaterialProperty prop, int min, int max, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            int newValue = EditorGUI.IntSlider(GetRect(prop), label, (int)prop.floatValue, min, max);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterPropertyChangeUndo(label.text);
                prop.floatValue = newValue;
            }
        }

        public static void DrawFloatToggleProperty(this MaterialEditor editor, GUIContent styles, MaterialProperty prop, int indentLevel = 0, bool isDisabled = false)
        {
            if (prop == null) return;

            using (new EditorGUI.DisabledScope(isDisabled))
            using (new EditorGUI.IndentLevelScope(indentLevel))
            {
                EditorGUI.BeginChangeCheck();
                bool newValue = EditorGUILayout.Toggle(styles, prop.floatValue == 1);
                if (EditorGUI.EndChangeCheck())
                    prop.floatValue = newValue ? 1.0f : 0.0f;
            }
        }
    }
}

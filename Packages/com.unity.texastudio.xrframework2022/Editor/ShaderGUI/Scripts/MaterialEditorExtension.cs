using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor
{
    public static partial class MaterialEditorExtension
    {
        internal static void BeginBlock(this MaterialEditor materialEditor, string label)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        internal static void EndBlock(this MaterialEditor materialEditor)
        {
            EditorGUILayout.EndVertical();
        }

        internal static void DrawMaskMapArea(this MaterialEditor materialEditor, MaterialProperty workflowMode,
            MaterialProperty maskMap,
            MaterialProperty metallic, MaterialProperty smoothness,
            MaterialProperty metallicMin, MaterialProperty metallicMax,
            MaterialProperty smoothnessMin, MaterialProperty smoothnessMax,
            MaterialProperty aoMin, MaterialProperty aoMax)
        {
            var workflowModeEnum = (WorkflowMode)workflowMode.floatValue;
            if (maskMap.textureValue != null)
            {
                if (workflowModeEnum == WorkflowMode.Metallic)
                    materialEditor.MinMaxShaderProperty(metallicMin, metallicMax, 0.0f, 1.0f,
                        URPPlusStyles.metallicRemappingText);

                materialEditor.MinMaxShaderProperty(smoothnessMin, smoothnessMax, 0.0f, 1.0f,
                    URPPlusStyles.smoothnessRemappingText);
                materialEditor.MinMaxShaderProperty(aoMin, aoMax, 0.0f, 1.0f, URPPlusStyles.aoRemappingText);
            }
            else
            {
                if (workflowModeEnum == WorkflowMode.Metallic)
                    materialEditor.ShaderProperty(metallic, URPPlusStyles.metallicText);

                materialEditor.ShaderProperty(smoothness, URPPlusStyles.smoothnessText);
            }

            var maskMapText = (workflowModeEnum == WorkflowMode.Metallic)
                ? URPPlusStyles.maskMapText
                : URPPlusStyles.maskMapSpecularText;
            materialEditor.TexturePropertySingleLine(maskMapText, maskMap);
        }

        internal static void DrawCustomNormalArea(this MaterialEditor materialEditor, GUIContent label,
            MaterialProperty bumpMap, MaterialProperty bumpMapScale = null)
        {
            if (bumpMapScale != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(label, bumpMap,
                    bumpMap.textureValue != null ? bumpMapScale : null);
                if (!EditorGUI.EndChangeCheck()) return;
                if (bumpMapScale.floatValue is 1 ||
                    !UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(
                        EditorUserBuildSettings.activeBuildTarget)) return;
                if (materialEditor.HelpBoxWithButton(URPPlusStyles.bumpScaleNotSupported, URPPlusStyles.fixNormalNow))
                    bumpMapScale.floatValue = 1;
            }
            else
            {
                materialEditor.TexturePropertySingleLine(label, bumpMap);
            }
        }
    }
}
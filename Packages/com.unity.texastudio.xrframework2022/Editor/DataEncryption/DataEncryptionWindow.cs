using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace TexaXRFramework.Core.Editor
{
    public class DataEncryptionWindow : EditorWindow
    {
        private string LegalIVCode = "*gdskfdske4324fd3243432fdsfdsfgfd";
        private string TempKey = "fdsfds23432213-0432432dsfds";
        private string EncodeSuffix = "data";
        private string DecodeSuffix = "txt";
        private DataEncryptionWindow()
        {
            base.titleContent = new GUIContent("数据加密配置窗口");
        }

        [MenuItem("XR开发工具/数据加密配置", false, 99)]
        public static void ShowRegisterWindow()
        {
            DataEncryptionWindow wind = (DataEncryptionWindow)EditorWindow.GetWindow(typeof(DataEncryptionWindow));

            wind.TempKey = DataEncryptionFileEditor.GetKey();
            wind.LegalIVCode = DataEncryptionFileEditor.GetCode();
            wind.EncodeSuffix = DataEncryptionFileEditor.GetEnSuffix();
            wind.DecodeSuffix = DataEncryptionFileEditor.GetDeSuffix();
        }
        private void OnGUI()
        {
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label("加密文件配置", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label("加密KEY", new GUILayoutOption[0]);
            this.TempKey = EditorGUILayout.TextArea(this.TempKey, new GUILayoutOption[] { GUILayout.MinHeight(50f) });
            GUILayout.Space(10f);
            GUILayout.Label("加密Code", new GUILayoutOption[0]);
            this.LegalIVCode = EditorGUILayout.TextArea(this.LegalIVCode, new GUILayoutOption[] { GUILayout.MinHeight(50f) });
            GUILayout.Space(10f);
            GUILayout.Label("加密文件后缀", new GUILayoutOption[0]);
            this.EncodeSuffix = EditorGUILayout.TextArea(this.EncodeSuffix, new GUILayoutOption[] { GUILayout.MinHeight(20f) });
            GUILayout.Space(10f);
            GUILayout.Label("解密文件后缀", new GUILayoutOption[0]);
            this.DecodeSuffix = EditorGUILayout.TextArea(this.DecodeSuffix, new GUILayoutOption[] { GUILayout.MinHeight(20f) });
            GUILayout.Space(10f);
            if (GUILayout.Button("保存配置", new GUILayoutOption[0]))
            {
                this.SaveConfigs(this.TempKey, this.LegalIVCode, this.EncodeSuffix, this.DecodeSuffix);
            }
            GUILayout.EndVertical();
        }

        private void SaveConfigs(string key, string code, string ensuffix, string desuffix)
        {
            if (string.IsNullOrEmpty(key))
                if (EditorUtility.DisplayDialog("配置失败", "请填入key!", "OK"))
                {
                    return;
                }
            if (string.IsNullOrEmpty(key))
                if (EditorUtility.DisplayDialog("配置失败", "请填入LegalIVCode!", "OK"))
                {
                    return;
                }
            try
            {
                string text = Application.dataPath + "/Resources/DataEncryptionConfig";
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                byte[] keybytes = Encoding.ASCII.GetBytes(key);
                File.WriteAllBytes(text + "/Key.txt", keybytes);
                byte[] codebytes = Encoding.ASCII.GetBytes(code);
                File.WriteAllBytes(text + "/Code.txt", codebytes);
                byte[] ensuffixbytes = Encoding.ASCII.GetBytes(ensuffix);
                File.WriteAllBytes(text + "/EnSuffix.txt", ensuffixbytes);
                byte[] desuffixbytes = Encoding.ASCII.GetBytes(desuffix);
                File.WriteAllBytes(text + "/DeSuffix.txt", desuffixbytes);
                AssetDatabase.Refresh();
                if (EditorUtility.DisplayDialog("配置成功", "配置保存成功，请备份一下配置文件!", "OK"))
                {
                    EditorWindow.GetWindow(typeof(DataEncryptionWindow)).Close();
                    return;
                }
            }
            catch (Exception e)
            {
                if (EditorUtility.DisplayDialog("配置失败", "配置异常，请查看日志!", "OK"))
                {
                    UnityEngine.Debug.LogError(e);
                    EditorWindow.GetWindow(typeof(DataEncryptionWindow)).Close();
                }
            }
        }
    }
}

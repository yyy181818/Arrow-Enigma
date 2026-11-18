using Codice.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace TexaXRFramework.Core.Editor
{
    public enum EncodeType
    {
        direct = 1,
        custom = 2,
        steamingassets = 3,
        resources = 4,
        custompath = 5
    }
    public class DataEncryptionFileEditor
    {
        private static SymmetricAlgorithm cryptService;
        [MenuItem("Assets/XR开发工具/加密文件/加密单个文件")]
        private static void DoEncodeFileCustom()
        {
            DoEncodeFiles(EncodeType.custom);
        }
    
       static void DoEncodeFiles(EncodeType pt)
        {
            try
            {
                if (!isInitConfig())
                    return;
                cryptService = new RijndaelManaged();

                string[] strs = Selection.assetGUIDs;
                string suffix = (Resources.Load("DataEncryptionConfig/EnSuffix") as TextAsset).text;
                string spath = "";
                if (pt == EncodeType.direct)
                {
                    for (int i = 0; i < strs.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(strs[i]);
                        spath = GetPrePath(path) + GetFileName(path) + "." + suffix;

                        if (!string.IsNullOrEmpty(path))
                        {
                            string text = File.ReadAllText(path);
                            string encode = Encrypt(text);
                            File.WriteAllText(spath, encode);
                            AssetDatabase.Refresh();//刷新
                        }
                        else
                            Debug.LogError("请选择正确的文件进行加密打包！");
                    }
                }
                else if (pt == EncodeType.custom)  //自定义只支持单文件
                {
                    if (strs.Length > 1)
                        Debug.LogWarning("仅支持单文件处理");
                    string path = AssetDatabase.GUIDToAssetPath(strs[0]);
                    string buildPath = EditorUtility.SaveFilePanel("请选择保存的路径", GetPrePath(path), GetFileName(path), suffix);

                    spath = buildPath;

                    if (!string.IsNullOrEmpty(path))
                    {
                        string text = File.ReadAllText(path);
                        string encode = Encrypt(text);
                        File.WriteAllText(spath, encode);
                        AssetDatabase.Refresh();//刷新
                    }
                    else
                        Debug.LogError("请选择正确的文件进行加密打包！");

                }
                else if (pt == EncodeType.steamingassets)
                {
                    for (int i = 0; i < strs.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(strs[i]);
                        string buildPath = Application.streamingAssetsPath;
                        if (!Directory.Exists(buildPath))
                            Directory.CreateDirectory(buildPath);
                        spath = buildPath + "/" + GetFileName(path) + "." + suffix;

                        if (!string.IsNullOrEmpty(path))
                        {
                            string text = File.ReadAllText(path);
                            string encode = Encrypt(text);
                            File.WriteAllText(spath, encode);
                            AssetDatabase.Refresh();//刷新
                        }
                        else
                            Debug.LogError("请选择正确的文件进行加密打包！");
                    }
                }
                else if (pt == EncodeType.resources)
                {
                    for (int i = 0; i < strs.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(strs[i]);
                        string buildPath = Application.dataPath + "/Resources";
                        if (!Directory.Exists(buildPath))
                            Directory.CreateDirectory(buildPath);
                        spath = buildPath + "/" + GetFileName(path) + "." + suffix;

                        if (!string.IsNullOrEmpty(path))
                        {
                            string text = File.ReadAllText(path);
                            string encode = Encrypt(text);
                            File.WriteAllText(spath, encode);
                            AssetDatabase.Refresh();//刷新
                        }
                        else
                            Debug.LogError("请选择正确的文件进行加密打包！");
                    }
                }
                else if (pt == EncodeType.custompath)
                {
                    string path = AssetDatabase.GUIDToAssetPath(strs[0]);
                    string buildPath = EditorUtility.SaveFolderPanel("请选择保存的位置", GetPrePath(path), "");
                    for (int i = 0; i < strs.Length; i++)
                    {
                        path = AssetDatabase.GUIDToAssetPath(strs[i]);
                        if (!Directory.Exists(buildPath))
                            Directory.CreateDirectory(buildPath);
                        spath = buildPath + "/" + GetFileName(path) + "." + suffix;

                        if (!string.IsNullOrEmpty(path))
                        {
                            string text = File.ReadAllText(path);
                            string encode = Encrypt(text);
                            File.WriteAllText(spath, encode);
                            AssetDatabase.Refresh();//刷新
                        }
                        else
                            Debug.LogError("请选择正确的文件进行加密打包！");
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError("加密文件失败！" + e);
            }
        }
        public static string GetKey()
        {
            return (Resources.Load("DataEncryptionConfig/Key") as TextAsset).text;
        }
        public static string GetCode()
        {
            return (Resources.Load("DataEncryptionConfig/Code") as TextAsset).text;
        }
        public static string GetEnSuffix()
        {
            return (Resources.Load("DataEncryptionConfig/EnSuffix") as TextAsset).text;
        }
        public static string GetDeSuffix()
        {
            return (Resources.Load("DataEncryptionConfig/DeSuffix") as TextAsset).text;
        }

        private static byte[] GetKeys()
        {
            string keyTemp = (Resources.Load("DataEncryptionConfig/Key") as TextAsset).text;
            cryptService.GenerateKey();
            byte[] bytTemp = cryptService.Key;
            int KeyLength = bytTemp.Length;

            if (keyTemp.Length > KeyLength)
                keyTemp = keyTemp.Substring(0, KeyLength);
            else if (keyTemp.Length < KeyLength)
                keyTemp = keyTemp.PadRight(KeyLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(keyTemp);
        }

        private static byte[] GetCodes()
        {
            string codeTemp = (Resources.Load("DataEncryptionConfig/Code") as TextAsset).text;
            cryptService.GenerateIV();
            byte[] bytTemp = cryptService.IV;
            int IVLength = bytTemp.Length;
            if (codeTemp.Length > IVLength)
                codeTemp = codeTemp.Substring(0, IVLength);
            else if (codeTemp.Length < IVLength)
                codeTemp = codeTemp.PadRight(IVLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(codeTemp);
        }


        public static string Encrypt(string Source)
        {
            byte[] bytIn = UTF8Encoding.UTF8.GetBytes(Source);
            MemoryStream ms = new MemoryStream();
            cryptService.Key = GetKeys();
            cryptService.IV = GetCodes();
            ICryptoTransform encrypto = cryptService.CreateEncryptor();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();
            ms.Close();
            byte[] bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }



        public static string Decrypt(string Source)
        {
            byte[] bytIn = Convert.FromBase64String(Source);
            MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
            cryptService.Key = GetKeys();
            cryptService.IV = GetCodes();
            ICryptoTransform encrypto = cryptService.CreateDecryptor();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        static bool isInitConfig()
        {
            if (Resources.Load("DataEncryptionConfig/Key") != null && Resources.Load("DataEncryptionConfig/Code") != null
                && Resources.Load("DataEncryptionConfig/EnSuffix") != null && Resources.Load("DataEncryptionConfig/DeSuffix") != null
                && !string.IsNullOrEmpty((Resources.Load("DataEncryptionConfig/Key") as TextAsset).text)
                && !string.IsNullOrEmpty((Resources.Load("DataEncryptionConfig/Code") as TextAsset).text)
                && !string.IsNullOrEmpty((Resources.Load("DataEncryptionConfig/EnSuffix") as TextAsset).text)
                && !string.IsNullOrEmpty((Resources.Load("DataEncryptionConfig/DeSuffix") as TextAsset).text))
            {
                return true;
            }
            if (EditorUtility.DisplayDialog("警告", "未进行配置，请在 Tools > 加密配置窗口 进行配置!", "OK"))
            {
                DataEncryptionWindow.ShowRegisterWindow();
                return false;
            }
            return false;
        }
        static string GetPrePath(string path)
        {
            return path.Substring(0, path.LastIndexOf("/") + 1);
        }
        static string GetFileName(string path)
        {
            string Name = "UnknownName";
            try
            {
                int stIdx = path.LastIndexOf("/") + 1;
                int edIdx = path.LastIndexOf(".") - 1;

                return path.Substring(stIdx, edIdx - stIdx + 1);
            }
            catch (Exception e)
            {
                return Name;
            }
        }
    }
}

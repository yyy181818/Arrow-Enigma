using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class DataMaskingDecoder
{
    private static SymmetricAlgorithm cryptService;


    public  static string GetDecodeString(string str) {
        cryptService = new RijndaelManaged();
        return Decrypt(str);
    }
    
    static string Decrypt(string Source)
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


    private static byte[] GetKeys()
    {
        string keyTemp = (Resources.Load("EncodeFile/Key") as TextAsset).text;
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
        string codeTemp = (Resources.Load("EncodeFile/Code") as TextAsset).text;
        cryptService.GenerateIV();
        byte[] bytTemp = cryptService.IV;
        int IVLength = bytTemp.Length;
        if (codeTemp.Length > IVLength)
            codeTemp = codeTemp.Substring(0, IVLength);
        else if (codeTemp.Length < IVLength)
            codeTemp = codeTemp.PadRight(IVLength, ' ');
        return ASCIIEncoding.ASCII.GetBytes(codeTemp);
    }
}

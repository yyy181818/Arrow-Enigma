using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TexaXRFramework.Core.Utility
{
    public class FileExtend
    {
        public static string GetMoviesFolder()
        {
            string[] temp = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);
            return (temp[0] + "/Movies");
        }

        public static string GetConfigFolder()
        {
            string[] temp = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);
            return (temp[0] + "/TexaStudioXR/Config/");
        }
    }
}

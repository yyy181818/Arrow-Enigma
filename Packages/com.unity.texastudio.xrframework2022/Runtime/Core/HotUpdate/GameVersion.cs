using System;
using System.Collections.Generic;

namespace TexaXRFramework.Core
{
    public class GameVersion
    {
        public string Version;
        public BuildVersion VersionEnv;
        public bool LocallySavedLog;
        public bool DeviceRestriction;
        public GameVersion(string version, BuildVersion versionEnv, bool deviceRestriction)
        {
            Version = version;
            VersionEnv = versionEnv;
            LocallySavedLog = versionEnv == BuildVersion.Release ? false : true;
            DeviceRestriction = deviceRestriction;
        }
        
        public GameVersion()
        {
            
        }
    }

    public class GameConfig
    {
        public static GameVersion LocalGameVersion = new GameVersion();
    }

    /// <summary>
    /// 版本环境
    /// </summary>
    public enum BuildVersion : byte
    { 
        //商用       
        Release = 1,
        //测试
        Beta=2
    }
}

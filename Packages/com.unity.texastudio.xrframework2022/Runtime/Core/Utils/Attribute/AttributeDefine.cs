
using System;
using UnityEngine;

namespace TexaXRFramework.Core
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BaseAttribute : Attribute
    {
        public Type AttributeType { get; }

        public BaseAttribute()
        {
            this.AttributeType = this.GetType();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StageAttribute : BaseAttribute
    {
        public string Type { get; }

        public StageAttribute(string type)
        {
            this.Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AreaAttribute : BaseAttribute
    {
        public string Type { get; }

        public AreaAttribute(string type)
        {
            this.Type = type;
        }
    }
    /// <summary>
    /// 设置枚举名称
    /// </summary>
#if UNITY_EDITOR
    [AttributeUsage(AttributeTargets.Field)]
#endif
    public class EnumAttirbute : PropertyAttribute
    {
        /// <summary>
        /// 枚举名称
        /// </summary>
        public string name;
        public EnumAttirbute(string name)
        {
            this.name = name;
        }
    }
}

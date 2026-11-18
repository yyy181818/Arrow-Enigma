using UnityEngine;
namespace TexaXRFramework.Core
{    /// <summary>
     /// 关卡定义组件（用于标记场景中的关卡区域及顺序）
     /// </summary>
    [DisallowMultipleComponent]
    public class LBEStageDefine : MonoBehaviour
    {
        [Header("关卡顺序")]
        public int Order;

        [Header("关卡名称(自动生成)")]
        public string StageName;
    }
}

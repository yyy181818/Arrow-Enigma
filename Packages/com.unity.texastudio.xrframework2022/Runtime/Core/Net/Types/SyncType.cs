using Newtonsoft.Json;
using System;
using UnityEngine;
namespace TexaXRFramework.Core.Net
{

    public enum HeadlessStartOptions
    {
        [EnumAttirbute("不执行任何操作")]
        DoNothing,
        [EnumAttirbute("自动启动客户端")]
        AutoStartClient,
        [EnumAttirbute("离线启动客户端")]
        OfflineStartClient
    }

    /// <summary>
    /// 播控服务器类型枚举
    /// 定义不同的服务器播控来源
    /// </summary>
    public enum ServerControlType
    {
        /// <summary>
        /// 自有服务器播控
        /// </summary>
        [EnumAttirbute("自有服务器播控")]
        SelfHostedServer,

        /// <summary>
        /// PICO服务器播控
        /// 指使用PICO官方提供的服务器服务进行播控管理
        /// </summary>
        [EnumAttirbute("PICO服务器播控")]
        PicoServer
    }
    /// <summary>
    /// 同步模式枚举（两种核心模式）
    /// 控制同步的数据内容和处理逻辑
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// 状态+绝对位置模式：同步当前状态和完整位置旋转
        /// 接收端直接覆盖本地位置（适合位置变化少的物体）
        /// </summary>
        [EnumAttirbute("状态+绝对位置模式")]
        StateAndAbsoluteTransform,

        /// <summary>
        /// 状态+差值模式：同步当前状态和位置旋转变化量
        /// 接收端叠加差值更新本地位置（适合频繁移动的物体）
        /// </summary>
        [EnumAttirbute("状态+差值模式")]
        StateAndDeltaTransform
    }

    /// <summary>
    /// 位置同步空间枚举（控制坐标类型）
    /// 决定同步时使用世界坐标还是本地坐标
    /// </summary>
    public enum PositionSyncSpace
    {
        /// <summary>
        /// 世界坐标
        /// </summary>
        [EnumAttirbute("世界坐标")]
        World,  // 世界坐标
        /// <summary>
        /// 本地坐标
        /// </summary>
        [EnumAttirbute("本地坐标")]
        Local   // 本地坐标（相对于父物体）
    }

    /// <summary>
    /// 互动物体状态枚举（通用状态定义）
    /// 定义互动物体可能处于的基础状态，便于统一管理和同步
    /// </summary>
    public enum InteractionState
    {
        [EnumAttirbute("空闲")]
        Idle =1,           // 空闲（未被互动）
        [EnumAttirbute("已激活")]
        Activated =2,      // 已激活（如按钮按下）
        [EnumAttirbute("空闲")]
        Deactivated =3,    // 已禁用（如开关关闭）
        [EnumAttirbute("互动中")]
        InProgress =4,     // 互动中（如门正在打开）
        [EnumAttirbute("互动完成")]
        Completed =5,      // 互动完成（如任务物品已拾取）
        [EnumAttirbute("锁定状态")]
        Locked =6,         // 锁定状态（无法互动）
        [EnumAttirbute("错误状态")]
        Error =7           // 错误状态（互动失败）
    }

    /// <summary>
    /// 互动同步数据结构
    /// </summary>
    [Serializable]
    public class LBEInteractiveSyncData
    {
        public string ObjectId;// 物体唯一标识
        public int State;// 互动状态

        // 变换数据
        public Vector3 Position;
        public Quaternion Rotation;

        // 互动状态数据（JSON序列化）
        public string Data;
    }

    /// <summary>
    /// 进度跳转数据（用于场景/区域进度同步）
    /// </summary>
    [Serializable]
    public class RecoveryData
    {
        /// <summary>目标阶段名称</summary>
        public string StageName;

        /// <summary>目标区域名称</summary>
        public string AreaName;

        /// <summary>区域内跳转时间（毫秒）</summary>
        public long AreaSeekTime = 0;

        /// <summary>区域起始时间（毫秒时间戳）</summary>
        public long AreaStartTime = 0;

        /// <summary>场景总运行时间（毫秒）</summary>
        public long StageRunningTime = 0;

        /// <summary>是否为重连恢复（标记特殊处理）</summary>
        public bool IsReconnect = false;

        /// <summary>转换为字符串（JSON格式）</summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

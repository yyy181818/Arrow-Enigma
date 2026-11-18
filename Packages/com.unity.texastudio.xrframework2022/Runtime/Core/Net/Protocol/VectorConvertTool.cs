using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 向量转换工具类（统一处理各种向量类型的填充转换）
    /// </summary>
    public static class VectorConvertTool
    {
        #region Unity Vector3 转换

        /// <summary>
        /// 将Unity Vector3填充到PBMessage.FVector3
        /// </summary>
        public static void FillVector3(PBMessage.FVector3 target, Vector3 source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
        }

        /// <summary>
        /// 将Unity Vector3填充到ProtoMessagesNet.FVector3
        /// </summary>
        public static void FillVector3(ProtoMessagesNet.FVector3 target, Vector3 source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
        }

        /// <summary>
        /// 将Unity Vector3填充到SharedModels.LBEFVector3
        /// </summary>
        public static void FillVector3(SharedModels.LBEFVector3 target, Vector3 source)
        {
            target.X = source.x;
            target.Y = source.y;
            target.Z = source.z;
        }

        /// <summary>
        /// 将Unity Vector3填充到NS_LBEShareData.LBEFVector3
        /// </summary>
        public static void FillVector3(NS_LBEShareData.LBEFVector3 target, Vector3 source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
        }

        #endregion

        #region Unity Quaternion 转换

        /// <summary>
        /// 将Unity Quaternion填充到PBMessage.FVector4
        /// </summary>
        public static void FillQuaternion(PBMessage.FVector4 target, Quaternion source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
            target.w = source.w;
        }

        /// <summary>
        /// 将Unity Quaternion填充到ProtoMessagesNet.FVector4
        /// </summary>
        public static void FillQuaternion(ProtoMessagesNet.FVector4 target, Quaternion source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
            target.w = source.w;
        }

        /// <summary>
        /// 将Unity Quaternion填充到SharedModels.LBEFVector4
        /// </summary>
        public static void FillQuaternion(SharedModels.LBEFVector4 target, Quaternion source)
        {
            target.X = source.x;
            target.Y = source.y;
            target.Z = source.z;
            target.W = source.w;
        }

        /// <summary>
        /// 将Unity Quaternion填充到NS_LBEShareData.LBEFVector4
        /// </summary>
        public static void FillQuaternion(NS_LBEShareData.LBEFVector4 target, Quaternion source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
            target.w = source.w;
        }

        #endregion

        #region 反向转换（协议向量 -> Unity向量）

        /// <summary>
        /// 将PBMessage.FVector3转换为Unity Vector3
        /// </summary>
        public static void ToUnityVector3(PBMessage.FVector3 source,ref Vector3 vector3)
        {
            vector3.x = source.x;
            vector3.y = source.y;
            vector3.z = source.z;
        }

        /// <summary>
        /// 将ProtoMessagesNet.FVector3转换为Unity Vector3
        /// </summary>
        public static void ToUnityVector3(ProtoMessagesNet.FVector3 source, ref Vector3 pos)
        {
            pos.x = source.x;
            pos.y = source.y;
            pos.z = source.z;
        }

        /// <summary>
        /// 将PBMessage.FVector4转换为Unity Quaternion
        /// </summary>
        public static void ToUnityQuaternion(PBMessage.FVector4 source,ref Quaternion rot)
        {
            rot.x = source.x;
            rot.y = source.y;
            rot.z = source.z;
            rot.w = source.w;
        }

        /// <summary>
        /// 将ProtoMessagesNet.FVector4转换为Unity Quaternion
        /// </summary>
        public static void ToUnityQuaternion(ProtoMessagesNet.FVector4 source, ref Quaternion rot)
        {
            rot.x = source.x;
            rot.y = source.y;
            rot.z = source.z;
            rot.w = source.w;
        }

        #endregion
    }
}
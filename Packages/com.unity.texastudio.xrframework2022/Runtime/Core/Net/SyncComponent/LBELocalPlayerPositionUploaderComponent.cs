using UnityEngine;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// LBE本地玩家位置上传组件
    /// </summary>
    [Tooltip("同步本地玩家在【物理空间】位置、旋转、缩放数据")]
    [DisallowMultipleComponent]
    public class LBELocalPlayerPositionUploaderComponent : MonoBehaviour
    {
        [Header("同步变换类型")]
        public PositionSyncSpace SyncTransType = PositionSyncSpace.Local;

        /// <summary>
        /// 固定更新间隔（无变化）
        /// </summary>
        [Header("固定更新间隔（无变化）")]
        public float FixedSendTime = 1f;

        /// <summary>
        /// 同步更新频率
        /// </summary>
        [Header("同步更新频率")]
        public float Rate = 30;

        protected Vector3 targetPosition;
        protected Vector3 targetLocalScale;
        protected Quaternion targetRotation;

        protected Vector3 netPosition;
        protected Vector3 netLocalScale;
        protected Quaternion netRotation;

        protected Vector3 lastPosition;
        protected Vector3 lastLocalScale;
        protected Quaternion lastRotation;

        protected Vector3 curPosition;
        protected Vector3 curLocalScale;
        protected Quaternion curRotation;


        private float _sendTime = 0;
        private float _fixedTime = 0;

        public void Start()
        {
            netPosition = lastPosition = SyncTransType == PositionSyncSpace.Local ? transform.localPosition : transform.position;
            netRotation = lastRotation = SyncTransType == PositionSyncSpace.Local ? transform.localRotation : transform.rotation;
            netLocalScale = lastLocalScale = transform.localScale;
        }
        

        void Update()
        {
            if (Time.time > _sendTime)
            {
                CheckMainRoleTransform();
                _sendTime = Time.time + (1f / Rate);
            }
        }
        protected virtual void CheckMainRoleTransform()
        {
            if (LBENetworkSyncManager.Instance == null ||
              !LBENetworkSyncManager.Instance.NetworkLogic.LoginStatus ||
              !LBENetworkSyncManager.Instance.NetworkLogic.ISCurPlayerDataValid)
            { return; }

            curPosition = SyncTransType == PositionSyncSpace.Local ? transform.localPosition : transform.position;
            curRotation = SyncTransType == PositionSyncSpace.Local ? transform.localRotation : transform.rotation;
            if (curPosition != lastPosition |
              curRotation != lastRotation |
              curLocalScale != lastLocalScale |
              Time.time > _fixedTime)
            {

                LBENetworkSyncManager.Instance.OperationService.SendPlayerTransformSync(
                    curPosition,
                    Quaternion.Euler(0f, curRotation.eulerAngles.y, 0f),
                    curLocalScale);

                lastPosition = curPosition;
                lastRotation = curRotation;
                lastLocalScale = curLocalScale;
                _fixedTime = Time.time + FixedSendTime;
            }
        }

    }
}

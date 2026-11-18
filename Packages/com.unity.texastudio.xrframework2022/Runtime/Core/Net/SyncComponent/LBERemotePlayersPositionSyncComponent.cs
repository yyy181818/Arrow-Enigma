using TMPro;
using UnityEngine;
namespace TexaXRFramework.Core.Net
{
    /*
    *注意：
    * 1.该组件是用于同步角色在【物理空间】位置、旋转、缩放数据；
    * 2.一定不能是【虚拟空间】，因为玩家可见性是为了在当前虚拟空间中，创建出物理空间靠近自己的玩家形象，防止物理空间位置过近导致碰撞，不同组玩家同一时刻是在不同的虚拟空间；
    */
    /// <summary>
    /// LBE远程玩家位置同步组件
    /// </summary>
    [Tooltip("同步远程玩家在【物理空间】位置、旋转、缩放数据")]
    [DisallowMultipleComponent]
    public class LBERemotePlayersPositionSyncComponent : MonoBehaviour
    {
        public string ObjectId;

        [Header("同步变换类型")]
        public PositionSyncSpace SyncTransType = PositionSyncSpace.Local;

        /// <summary>
        /// 是否强制Y轴为0
        /// </summary>
        [Header("是否强制Y轴为0")]
        public bool LockYToZero = true;

        [Header("平滑速度")]
        public float lerpSpeed = 10f;

        protected Vector3 targetPosition;
        protected Vector3 targetLocalScale;
        protected Quaternion targetRotation;

        public  Vector3 netPosition;
        protected Vector3 netLocalScale;
        protected Quaternion netRotation;

        protected Vector3 curPosition;
        protected Vector3 curLocalScale;
        protected Quaternion curRotation;
        private bool hasReceivedFirstUpdate = false;
        private float positionThreshold = 0.01f;   
        private float rotationThreshold = 0.1f;    
        private float updateThreshold = 0.01f; 
        private bool isInterpolating = false;
        private float _refreshTime = 0;
        private bool _isDestory = false;
        void Start()
        {
            netPosition = SyncTransType == PositionSyncSpace.Local ? transform.localPosition : transform.position;
            netRotation = SyncTransType == PositionSyncSpace.Local ? transform.localRotation : transform.rotation;
            netLocalScale = transform.localScale;
            if (LockYToZero)
            {
                ResetYPosition(ref netPosition);
            }
        }
        public void Update()
        {
            _refreshTime += Time.deltaTime;
            if (_refreshTime > 3)
            {
                LBENetworkSyncManager.Instance.NetworkLogic.OnSyncTransformUnRegister(this);
                _isDestory = true;
                Destroy(gameObject);
            }

            if (_isDestory) { return; }

            UpdateDoSyncTransform();
        }
        private void UpdateDoSyncTransform()
        {
            if (!isInterpolating) { return; }

            if (SyncTransType == PositionSyncSpace.Local)
            {
                if (Vector3.Distance(transform.localPosition = Vector3.Lerp(
                   transform.localPosition, targetPosition, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime)),
                   targetPosition) < positionThreshold &&
                   Quaternion.Angle(transform.localRotation = Quaternion.Lerp(
                   transform.localRotation, targetRotation, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime)),
                   targetRotation) < rotationThreshold)
                {
                    // 最终对齐并停止插值
                    transform.localPosition = targetPosition;
                    transform.localRotation = targetRotation;
                    isInterpolating = false;
                }
            }
            else
            {
                if (Vector3.Distance(transform.position = Vector3.Lerp(
                    transform.position, targetPosition, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime)),
                    targetPosition) < positionThreshold &&
                    Quaternion.Angle(transform.rotation = Quaternion.Lerp(
                    transform.rotation, targetRotation, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime)),
                    targetRotation) < rotationThreshold)
                {
                    // 最终对齐并停止插值
                    transform.position = targetPosition;
                    transform.rotation = targetRotation;
                    isInterpolating = false;
                }
            }
        }


        /// <summary>
        /// 位置同步
        /// </summary>
        /// <param name="netPos"></param>
        /// <param name="netRot"></param>
        /// <param name="isSameTeam"></param>
        public void DoSyncTransform(Vector3 netPos, Quaternion netRot, bool isSameTeam = true)
        {
            //Debug.Log($"[位置同步] 收到玩家位置同步数据:{netPos.x}-{netPos.y}-{netPos.z}-{isSameTeam}");
            if (_isDestory)
            {
                return;
            }
            netRotation = netRot;
            netPosition = netPos;
            if (!isSameTeam)
            {
                netRotation = Quaternion.Euler(0f, netRot.eulerAngles.y, 0f);
                netPosition.y = netPos.y;
            }

            if (LockYToZero)
            {
                ResetYPosition(ref netPosition);
            }

            ProcessSyncTransformData();
        }

        private void ProcessSyncTransformData()
        {
            if (!hasReceivedFirstUpdate)
            {
                if (SyncTransType == PositionSyncSpace.Local)
                {
                    transform.localPosition = targetPosition = netPosition;
                    transform.localRotation = targetRotation = netRotation;
                }
                else
                {
                    transform.position = targetPosition = netPosition;
                    transform.rotation = targetRotation = netRotation;
                }
                hasReceivedFirstUpdate = true;
            }

            if (Vector3.Distance(netPosition, targetPosition) >= updateThreshold ||
            Quaternion.Angle(netRotation, targetRotation) >= rotationThreshold)
            {
                targetPosition = netPosition;
                targetRotation = netRotation;
                isInterpolating = true;
            }
            _refreshTime = 0;
        }

        /// <summary>
        /// 客态同步对象
        /// </summary>
        /// <param name="identity"></param>
        public void OnInitNetObject(string identity, string nick)
        {
            ObjectId = identity;
            if (nick != string.Empty)
            {
                SetSyncNick(nick);
            }
            LBENetworkSyncManager.Instance.NetworkLogic.OnSyncTransformRegister(this);
        }

        private void SetSyncNick(string nick)
        {
            TextMeshPro firstText = GetFirstTextMeshProComponent(transform);
            if (firstText != null)
            {
                firstText.text = nick;

            }
        }

        private TextMeshPro GetFirstTextMeshProComponent(Transform parent)
        {
            if (parent == null) return null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null)
                {
                    TextMeshPro text = child.GetComponent<TextMeshPro>();
                    if (text != null)
                    {
                        return text;
                    }
                }
            }
            return null;
        }

        void ResetYPosition(ref Vector3 curPos)
        {
            curPos.y = 0f;
        }
    }
}

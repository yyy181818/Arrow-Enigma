using System.Collections;
using System.Collections.Generic;
using TexaXRFramework.Core.Net;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// 出界指示器管理器
    /// </summary>
    [DisallowMultipleComponent]
    public class LBEIndicatorManager :MonoSingleton<LBEIndicatorManager>
    {
        [Header("检测间隔时间(秒)")]
        public float CheckInterval = 0.1f; // 检测间隔时间（秒）
        private float _timer = 0f;
        private Transform _checkTransform;
        private LBENetworkSyncManager _network;
        public LBEIndicatorBase _indicatorBase;
        private void Start()
        {
            _checkTransform = Camera.main.transform;
            _network=GetComponent<LBENetworkSyncManager>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= CheckInterval)
            {
                if (_network != null &&
                _network.NetworkLogic.GetCurStage != null &&
               _network.NetworkLogic.GetCurStage.CurArea != null)
                {
                    if (!_network.NetworkLogic.GetCurStage.CurArea.OnIsContentBoundaryInside(_checkTransform.position, out var centerPotint))
                    {
                        _indicatorBase?.OnShowIndicator(true, new Vector3(_checkTransform.position.x, 0.2f, _checkTransform.position.z), new Vector3(centerPotint.x, 0.2f, centerPotint.y));
                        _network.NetworkLogic.GetCurStage.CurArea.ForceShowContentArea();
                    }
                    else
                    {
                        _indicatorBase?.OnShowIndicator(false, Vector3.zero, Vector3.zero);
                        _network.NetworkLogic.GetCurStage.CurArea.ForceRecoveryContentArea();
                    }
                }
                _timer = 0f;
            }
        }

        public void SetIndicator(LBEIndicatorBase indicatorBase )
        {
            _indicatorBase = indicatorBase;
        }

    }
}

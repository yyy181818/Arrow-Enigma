using SharedModels;
using System.Collections.Generic;
using TexaXRFramework.Core.Net;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    ///  关卡逻辑基类
    /// </summary>
    [DisallowMultipleComponent]
    public class LBEStageLogicBase : MonoBehaviour
    {
        [HideInInspector]
        public LBEStageData StageData;
        [HideInInspector]
        public LBEArea CurArea;
        [HideInInspector]
        public string StageName;

        private Dictionary<string, LBEAreaDefine> _allAreaDefineDic = new Dictionary<string, LBEAreaDefine>();

        private List<LBENetworkMonoBehaviour> _allNetworkMonoList = new List<LBENetworkMonoBehaviour>();

        protected virtual void Awake()
        {
            if (LBENetworkSyncManager.Instance != null)
            {
                LBENetworkSyncManager.Instance.NetworkLogic.OnSetCurStage(this);
            }

            var stageDefine = GetComponent<LBEStageDefine>();
            Debug.Log($"[场景逻辑] 当前场景名字:{stageDefine.StageName}");
            if (stageDefine != null)
            {
                StageName = stageDefine.StageName;
            }
            else
            {
                Debug.LogError("[场景逻辑] 找不到 LBEStageDefine 脚本");
            }
            StageData = LBELoaderContentConfig.GetStageData(StageName);

            BindAreaDefine();

            FindAllLBENetworkMonoBehaviour();

            if (StageData != null)
            {
                Debug.Log($"[场景逻辑] 将进入场景:{StageData.StageName} 发送到服务器");
                LBENetworkSyncManager.Instance.OperationService.SyncStageSwitch(StageData.StageName);
                OnInit();
            }
            else
            {
                Debug.LogError($"[场景逻辑] 当前场景:{StageName}无法找到配置");
            }
        }
        private void BindAreaDefine()
        {
            var allAreas = GetFirstAreaChild().GetComponentsInChildren<LBEAreaDefine>(true);
            if (allAreas != null)
            {
                foreach (var area in allAreas)
                {
                    if (!_allAreaDefineDic.ContainsKey(area.AreaName))
                    {
                        _allAreaDefineDic.Add(area.AreaName, area);
                        Debug.Log($"[场景逻辑] 当前场景:{StageName}, 绑定区域游戏对象:{area.gameObject.name}  到 {area.AreaName}  顺序:{area.Order} 区域最大可停留时间:{area.MaxStayTime}");
                        area.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError($"[场景逻辑] 当前场景:{StageName}中区域:{area.AreaName} 不止一个!");
                    }
                }
            }
        }

        /// <summary>
        /// 查找并返回第一个名称包含"Area"的第一层子物体Transform
        /// </summary>
        /// <returns>找到的Transform，未找到则返回null</returns>
        private Transform GetFirstAreaChild()
        {
            // 遍历所有第一层子物体
            foreach (Transform child in transform)
            {
                // 检查名称是否包含"Area"（不区分大小写）
                if (child.name.IndexOf("Area", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return child; // 找到第一个匹配项后立即返回
                }
            }
            // 遍历结束仍未找到，返回null
            return null;
        }

        protected virtual void OnInit()
        {
            if (LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.IsReconnect)
            {
                if (StageName.Equals(LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.StageName))
                {
                    Debug.Log($"[断线重连] 重连区域:{LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.AreaName}");
                    OnEntryArea("", LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.AreaName);
                }
                return;
            }
            EntryFirstArea();
        }

        /// <summary>
        /// 进入第一个区域
        /// </summary>
        private void EntryFirstArea()
        {
            var data = LBELoaderContentConfig.GetFirstArea(StageName);
            if (data != null)
            {
                OnEntryArea("", data.AreaName);
            }
            else
            {
                Debug.LogError($"[场景逻辑] 场景:{StageName}, 找不到第一个区域!");
            }
        }

        public virtual void  OnEntryArea(string currentAreaName, string nextAreaName)
        {
            if (CurArea != null && CurArea.AreaName.Equals(nextAreaName))
            {
                if (LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.IsReconnect)
                {
                    Debug.Log($"[断线重连] 重新连接处理, 恢复数据...");
                    RestoreProgress(LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.StageRunningTime);
                    LBENetworkSyncManager.Instance.NetworkLogic.ResetReconnectRecoveryData();
                }
                return;
            }
            if (!string.IsNullOrEmpty(currentAreaName))
            {
                Debug.Log($"[场景逻辑] 更改区域从: {currentAreaName} 到: {nextAreaName}");
            }
            else
            {
                Debug.Log($"[场景逻辑] 进入首个区域: {nextAreaName}");
            }


            if (CurArea != null)
            {
                if (!string.IsNullOrEmpty(currentAreaName) && !CurArea.AreaName.Equals(currentAreaName))
                {
                    Debug.LogError($"[场景逻辑] 更改区域，当前区域名称出错");
                }

                CurArea.OnDispose();
            }

            var data = LBELoaderContentConfig.GetArea(StageName, nextAreaName);
            if (data != null)
            {
                CurArea= new LBEArea(nextAreaName, this, OnGetAreaObject(nextAreaName));
                CurArea.OnInit();
                if (LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.IsReconnect)
                {
                    RestoreProgress(LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.StageRunningTime);
                    LBENetworkSyncManager.Instance.NetworkLogic.ResetReconnectRecoveryData();
                }
                Debug.Log($"[场景逻辑] 更改到区域:{nextAreaName} 已完成, 发送进入区域:{nextAreaName} 到服务器");
                LBENetworkSyncManager.Instance.OperationService.SyncAreaSwitch(currentAreaName, nextAreaName);
            }
            else
            {
                if (LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.IsReconnect)
                {
                    RestoreProgress(LBENetworkSyncManager.Instance.NetworkLogic.ReconnectRecoveryData.StageRunningTime);
                    LBENetworkSyncManager.Instance.NetworkLogic.ResetReconnectRecoveryData();
                }
                Debug.LogError($"[场景逻辑] 当前场景:{StageName}, 找不到区域:{nextAreaName}的数据");
            }
        }

        public GameObject OnGetAreaObject(string areaName)
        {
            if (_allAreaDefineDic.TryGetValue(areaName, out var value))
            {
                return value.gameObject;
            }
            return null;
        }

        public void FindAllLBENetworkMonoBehaviour()
        {
            var foundComponents = FindObjectsOfType<LBENetworkMonoBehaviour>(includeInactive: true);

            // 过滤掉可能已销毁的对象（安全检查）
            foreach (var component in foundComponents)
            {
                if (component != null)
                {
                    _allNetworkMonoList.Add(component);
                }
            }
        }

        /// <summary>
        /// 切换到下一个区域
        /// </summary>
        /// <param name="nextAreaName">区域名字</param>
        public virtual void OnSwitchNextArea(string nextAreaName)
        {
            var oldAreaName = CurArea.AreaName != null ? CurArea.AreaName : "";
            OnEntryArea(oldAreaName, nextAreaName);
        }

        /// <summary>
        /// 切换到下一个区域
        /// </summary>
        public virtual void OnSwitchNextArea()
        {
            var data = LBELoaderContentConfig.GetNextArea(StageName, CurArea.AreaName);
            var oldAreaName = CurArea.AreaName != null ? CurArea.AreaName : "";
            OnEntryArea(oldAreaName, data.AreaName);
        }

        /// <summary>
        /// 队伍触发器
        /// </summary>
        /// <param name="areaName"></param>
        /// <param name="triggerName"></param>
        public void OnTeamTriggered(string areaName, string triggerName)
        {
            //if (CurArea != null &&
            //    !string.IsNullOrEmpty(areaName) &&
            //    !string.IsNullOrEmpty(triggerName) &&
            //    CurArea.AreaName.Equals(areaName))
            //{
            //    CurArea.OnTeamTriggered(areaName, triggerName);
            //}
            //else
            //{
            //    Debug.LogError("[场景逻辑-队伍触发器] 队伍触发错误");
            //}
        }

        /// <summary>
        /// 恢复进度
        /// </summary>
        /// <param name="progressTime"></param>
        public virtual void RestoreProgress(long progressTime, string operation=null)
        {
            Debug.Log($"[断线重连] 重新连接处理, 恢复场景数据...{progressTime}");
            foreach (var component in _allNetworkMonoList)
            {
                if (component != null)
                {
                    component.RestoreProgress(progressTime, operation);
                }
            }
        }

        /// <summary>
        /// 业务逻辑
        /// </summary>
        /// <param name="msg"></param>
        public virtual void OnOperation()
        {

        }

        /// <summary>
        /// 资源的释放
        /// </summary>
        public virtual void OnDispose()
        {

        }
    }
}

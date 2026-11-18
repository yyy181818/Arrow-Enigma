using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 网络互动同步服务（封装所有互动组件的网络同步调用）
    /// </summary>
    public class NetworkInteractiveSyncService
    {
        // 互动组件字典（ObjectId -> 组件实例）
        private Dictionary<string, LBENetworkInteractiveSyncComponent<object>> _componentsById = new Dictionary<string, LBENetworkInteractiveSyncComponent<object>>();
        private LBENetworkSyncManager _network;
        public NetworkInteractiveSyncService(LBENetworkSyncManager network)
        {
            _network = network;
        }
        /// 注册互动同步组件
        /// </summary>
        public void RegisterComponent<T>(LBENetworkInteractiveSyncComponent<T> component) where T : class, new()
        {
            var baseComponent = component as LBENetworkInteractiveSyncComponent<object>;
            if (baseComponent == null) return;

            if (!_componentsById.ContainsKey(component.ObjectId))
            {
                _componentsById[component.ObjectId] = baseComponent;
                Debug.Log($"注册互动同步组件: {component.ObjectId}");
            }
        }
        /// <summary>
        /// 重连时恢复所有互动物体状态
        /// </summary>
        private void RestoreInteractiveStates()
        {
            //// 从登录数据中恢复状态（登录时已获取）
            //var teamData = LBENetworkSyncManager.Instance.NetworkLogic.CurPlayerData?.ReconnectData;
            //if (teamData?.InteractiveStates == null) return;

            //foreach (var stateData in teamData.InteractiveStates)
            //{
            //    if (_componentsById.TryGetValue(stateData.ObjectId, out var component) && component.RestoreOnReconnect)
            //    {
            //        component.HandleSyncData(stateData);
            //        Debug.Log($"重连恢复互动物体状态: {stateData.ObjectId}");
            //    }
            //}
        }

        /// <summary>
        /// 处理接收到的互动同步数据
        /// </summary>
        public void HandleSyncData(string objectId, int state, Vector3 position, Quaternion rotation, string data)
        {
            var syncData = new LBEInteractiveSyncData
            {
                ObjectId = objectId,
                State = state,
                Position = position,
                Rotation = rotation,
                Data = data
            };
            if (_componentsById.TryGetValue(syncData.ObjectId, out var component))
            {
                component.HandleSyncData(syncData);
            }
            else
            {
                // 动态创建未加载的互动物体
                RequestCreateInteractiveObject(syncData);
            }
        }

        /// <summary>
        /// 请求创建未加载的互动物体（需配合预制体系统）
        /// </summary>
        private void RequestCreateInteractiveObject(LBEInteractiveSyncData data)
        {
            Debug.Log($"请求创建互动物体: {data.ObjectId}");
            // 实际项目中应从配置表获取预制体信息
            // ObjectPool.Instance.Spawn(prefabPath, data.Position, data.Rotation, (go) => {
            //     var syncComp = go.GetComponent<InteractiveSyncComponent<object>>();
            //     if (syncComp != null) syncComp.HandleSyncData(data);
            // });
        }

        /// <summary>
        /// 发送互动同步数据
        /// </summary>
        public void SendSyncData(LBEInteractiveSyncData data)
        {
            if (_network.IsNetworkConnected)
            {
                _network.OperationService.SendInteractiveSync(data.ObjectId,data.State,data.Position,data.Rotation,data.Data);
            }
        }
        /// <summary>
        /// 注销互动同步组件
        /// </summary>
        public void UnregisterComponent<T>(LBENetworkInteractiveSyncComponent<T> component) where T : class, new()
        {
            // 将泛型组件转换为基类的 object 泛型版本（与注册逻辑一致）
            var baseComponent = component as LBENetworkInteractiveSyncComponent<object>;
            if (baseComponent != null && _componentsById.ContainsKey(component.ObjectId))
            {
                _componentsById.Remove(component.ObjectId);
                Debug.Log($"注销互动同步组件: {component.ObjectId}");
            }
        }
    }
}

using System;
using System.Collections;
using TexaXRFramework.Core.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TexaXRFramework.Core
{
    public class LBESceneHelper
    {
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="progressAction">进度回调</param>
        /// <param name="loadCompleteAction">加载完成</param>
        /// <param name="loadType">加载类型</param>
        public static void OnSyncLoad(string sceneName, Action<float> progressAction, Action<bool> loadCompleteAction, LoadSceneMode loadType = LoadSceneMode.Single)
        {
            LBENetworkSyncManager.Instance.StartCoroutine(AsyncLoadScene(sceneName, progressAction, loadCompleteAction, loadType));
        }

        private static IEnumerator AsyncLoadScene(string sceneName, Action<float> progressAction, Action<bool> loadCompleteAction, LoadSceneMode loadType)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, loadType);
            if (loadCompleteAction != null)
            {
                op.completed += operation =>
                {
                    loadCompleteAction.Invoke(true);
                };
            }
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                progressAction?.Invoke(op.progress);
                yield return null;
            }
            progressAction?.Invoke(1f);
            yield return new WaitForSeconds(1f);
            op.allowSceneActivation = true;
        }

        /// <summary>
        /// 获取场景名称
        /// </summary>
        /// <param name="sceneId"></param>
        /// <returns></returns>
        public static string GetSceneName(int sceneId)
        {
            if (sceneId >= 0 && sceneId < SceneManager.sceneCountInBuildSettings)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneId);
                int lastSlashIndex = scenePath.LastIndexOf('/');
                if (lastSlashIndex != -1)
                {
                    return scenePath.Substring(lastSlashIndex + 1).Replace(".unity", "");
                }
            }
            return null;
        }
    }

}

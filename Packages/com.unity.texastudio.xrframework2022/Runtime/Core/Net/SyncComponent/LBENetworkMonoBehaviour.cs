using UnityEngine;
using UnityEngine.SceneManagement;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 网络行为基类
    /// </summary>
    [DisallowMultipleComponent]
    public class LBENetworkMonoBehaviour : MonoBehaviour
    {
        protected virtual void Start()
        {

        }

        public virtual void RestoreProgress(long curTime, string operation = null)
        {

        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="nextSceneName">场景名字</param>
        public void LoadScene(string nextSceneName)
        {
            LBENetworkSyncManager.Instance.NetworkLogic.OnEntryStage(nextSceneName);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneId">场景ID</param>
        public void LoadScene(int sceneId)
        {
            LBENetworkSyncManager.Instance.NetworkLogic.OnEntryStage(LBESceneHelper.GetSceneName(sceneId));
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene()
        {
            LBENetworkSyncManager.Instance.NetworkLogic.OnEntryStage(LBESceneHelper.GetSceneName(SceneManager.GetActiveScene().buildIndex + 1));
        }

        /// <summary>
        /// 加载区域
        /// </summary>
        /// <param name="nextAreaName"></param>
        public void LoadArea(string nextAreaName)
        {
            LBENetworkSyncManager.Instance.NetworkLogic.OnSwitchNextArea(nextAreaName);
        }

        /// <summary>
        /// 加载区域
        /// </summary>
        /// <param name="nextAreaName"></param>
        public void LoadArea()
        {
            LBENetworkSyncManager.Instance.NetworkLogic.OnSwitchNextArea();
        }
    }
}

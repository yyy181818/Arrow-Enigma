using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

/// <summary>
/// Laucher 场景常驻管理器：玩家、相机、线索展示点、提示文字、场景重载。
/// 原来的下注盘 / 手部面板相关代码已随轮盘赌一起移除。
/// </summary>
public class LaucherManager : MonoBehaviour
{
    public static LaucherManager Instance { get; private set; }

    [Header("玩家和相机")]
    public Transform player;
    public Transform headCamera;

    [Header("线索展示点")]
    public Transform Point;

    [Header("提示文字")]
    public Text resultText;

    void Awake()
    {
        Instance = this;

        if (player == null)
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>(FindObjectsInactive.Include);
            if (origin != null) player = origin.transform;
        }

        if (headCamera == null && Camera.main != null)
        {
            headCamera = Camera.main.transform;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowText(string content)
    {
        if (resultText == null) return;

        resultText.text = content;
        resultText.gameObject.SetActive(true);
    }

    public void TeleportPlayer(Vector3 target)
    {
        if (player == null) return;
        player.position = target;
    }

    /// <summary>
    /// 重生用：只重载 GameScene（叠加模式），保留 Laucher 场景里的 UI。
    /// </summary>
    public void ReloadGameScene()
    {
        StartCoroutine(ReloadGameSceneRoutine());
    }

    System.Collections.IEnumerator ReloadGameSceneRoutine()
    {
        UnityEngine.SceneManagement.Scene scene =
            UnityEngine.SceneManagement.SceneManager.GetSceneByName("GameScene");

        if (scene.IsValid() && scene.isLoaded)
        {
            UnityEngine.AsyncOperation unload =
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("GameScene");
            while (unload != null && !unload.isDone) yield return null;
        }

        UnityEngine.AsyncOperation load =
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                "GameScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        while (load != null && !load.isDone) yield return null;
    }
}

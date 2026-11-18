using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameScene 的全局管理器：玩家出生点、区域、死亡重生。
/// 轮盘赌相关内容已全部移除，失败条件只剩炸弹倒计时和选错简历。
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("玩家出生点")]
    public GameObject Spawn;

    [Header("区域")]
    [Tooltip("所有区域，初始只有第 0 个是激活的")]
    public GameObject[] areas;

    [Header("筹码为 0 的提示文字")]
    public string deathText = "You died";

    [Header("死亡后等多久重新加载场景")]
    public float respawnDelay = 2f;

    // 死亡时的位置，场景重载后恢复，保证原地复活
    static bool _hasDeathPosition;
    static Vector3 _deathPosition;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 场景重载完成后，把玩家放回死亡时的位置（用 Start 保证 LaucherManager 已初始化）
        if (_hasDeathPosition && LaucherManager.Instance != null && LaucherManager.Instance.player != null)
        {
            LaucherManager.Instance.player.position = _deathPosition;
            _hasDeathPosition = false;
        }
    }

    /// <summary>死亡重生：原地复活，然后重新加载 GameScene。</summary>
    public void Respawn()
    {
        if (LaucherManager.Instance != null)
        {
            LaucherManager.Instance.ShowText("");

            // 记下死亡位置，重载后原地复活
            if (LaucherManager.Instance.player != null)
            {
                _deathPosition = LaucherManager.Instance.player.position;
                _hasDeathPosition = true;
            }

            LaucherManager.Instance.ReloadGameScene();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}

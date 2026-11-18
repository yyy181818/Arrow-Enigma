using UnityEngine;

/// <summary>
/// 背景音：电子钟走动的滴答声。
///
/// 游戏启动时自动创建并常驻（DontDestroyOnLoad），跨 GameScene 重载不会中断。
/// 如果 Laucher 场景里已经手动放了一个挂了本脚本的物体，就不会再自动创建，
/// 可以在 Inspector 里调音量 / 换音频。
/// 音频默认从 Resources/Sound/ClockTick 读取。
/// </summary>
public class ClockBgm : MonoBehaviour
{
    public static ClockBgm Instance { get; private set; }

    [Header("音效")]
    [Tooltip("留空则从 Resources/Sound/CarnivalLoop 加载（找不到再退回 ClockTick）")]
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.12f;

    AudioSource _source;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        // 场景里已经手动放了一个就不重复创建
        if (FindAnyObjectByType<ClockBgm>() != null) return;

        GameObject go = new GameObject("ClockBgm");
        DontDestroyOnLoad(go);
        go.AddComponent<ClockBgm>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();

        _source.loop = true;
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        _source.volume = volume;

        if (clip == null) clip = Resources.Load<AudioClip>("Sound/CarnivalLoop");
        if (clip == null) clip = Resources.Load<AudioClip>("Sound/ClockTick");

        if (clip == null)
        {
            Debug.LogWarning("[ClockBgm] 没找到 Resources/Sound/CarnivalLoop.wav，背景音不会播放");
            return;
        }

        _source.clip = clip;
        _source.Play();
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (_source != null) _source.volume = volume;
    }

    public void Play()
    {
        if (_source != null && !_source.isPlaying) _source.Play();
    }

    public void Stop()
    {
        if (_source != null) _source.Stop();
    }
}

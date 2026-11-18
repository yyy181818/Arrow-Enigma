using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ZivenSDK;
using ZivenSDK.Content;

public class BoundaryBlackoutIndicator : MonoBehaviour
{
    [Header("黑屏-全局光照")]
    [Tooltip("场景所有 Shader 读取的全局变量名,出界推到 maxLight 变暗")]
    public string lightProperty = "_Light";
    [Range(0f, 1f)] public float maxLight = 1f;
    public float fadeDuration = 1f;

    [Header("黑屏-相机剔除")]
    [Tooltip("出界时只渲染这些层(边界提示层)。留空则不切 cullingMask")]
    public string[] keepLayersWhenOutside = { "Area" };
    [Tooltip("留空=自动拿本物体上的 Camera 组件(挂在相机上时用)")]
    public Camera targetCamera;

    [Header("音频")]
    [Tooltip("出界时淡出场景音频")]
    public bool muteAudioOnOutside = true;
    [Tooltip("这些名字的 AudioSource 不被静音(如玩家旁白)")]
    public string[] audioWhitelist = { "Player", "AppRoot" };
    [Tooltip("出界提示音(挂在本物体的 AudioSource)")]
    public AudioSource hintSound;

    private const string TWEEN_ID = "BoundaryBlackout_Light";
    private float _light;
    private int _originalCullingMask;
    private CameraClearFlags _originalClearFlags;
    private Color _originalBgColor;
    private bool _isOutside;
    private bool _initialized;

    private readonly Dictionary<AudioSource, float> _originalVolumes =
        new Dictionary<AudioSource, float>();

    private BoundaryEventModule _boundaryEvent;

    public bool IsOutside => _isOutside;

    void Awake()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (targetCamera != null)
        {
            _originalCullingMask = targetCamera.cullingMask;
            _originalClearFlags = targetCamera.clearFlags;
            _originalBgColor = targetCamera.backgroundColor;
        }
        if (hintSound == null) hintSound = GetComponent<AudioSource>();
        Shader.SetGlobalFloat(lightProperty, 0f);
    }

    IEnumerator Start()
    {
        while (LBEManager.Instance == null || LBEManager.Instance.BoundaryEvent == null)
            yield return null;

        _boundaryEvent = LBEManager.Instance.BoundaryEvent;
        _boundaryEvent.OnAnyBoundaryExit += OnAnyBoundaryExitHandler;
        _boundaryEvent.OnAnyBoundaryEnter += OnAnyBoundaryEnterHandler;

        _initialized = true;
    }

    void OnDestroy()
    {
        if (_boundaryEvent != null)
        {
            _boundaryEvent.OnAnyBoundaryExit -= OnAnyBoundaryExitHandler;
            _boundaryEvent.OnAnyBoundaryEnter -= OnAnyBoundaryEnterHandler;
        }

        DOTween.Kill(TWEEN_ID);
        Shader.SetGlobalFloat(lightProperty, 0f);

        if (targetCamera != null)
        {
            targetCamera.cullingMask = _originalCullingMask;
            targetCamera.clearFlags = _originalClearFlags;
            targetCamera.backgroundColor = _originalBgColor;
        }

        _originalVolumes.Clear();
    }

    public void OnAnyBoundaryExitHandler()
    {
        if (!_initialized) return;
        FadeIn();
    }

    public void OnAnyBoundaryEnterHandler()
    {
        if (!_initialized) return;
        FadeOut();
    }

    public void FadeIn()
    {
        DOTween.Kill(TWEEN_ID);
        _light = maxLight;
        Shader.SetGlobalFloat(lightProperty, maxLight);
        ApplyCullingMask(true);

        if (muteAudioOnOutside)
            MuteAudioImmediate();

        if (hintSound != null) hintSound.Play();
    }

    public void FadeOut()
    {
        DOTween.Kill(TWEEN_ID);
        if (hintSound != null) hintSound.Stop();
        ApplyCullingMask(false);

        _light = 0f;
        Shader.SetGlobalFloat(lightProperty, 0f);

        if (muteAudioOnOutside)
            UnmuteAudioImmediate();
    }

    private void ApplyCullingMask(bool outside)
    {
        if (targetCamera == null) return;

        if (outside)
        {
            if (keepLayersWhenOutside != null && keepLayersWhenOutside.Length > 0)
                targetCamera.cullingMask = LayerMask.GetMask(keepLayersWhenOutside);

            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.black;
        }
        else
        {
            targetCamera.cullingMask = _originalCullingMask;
            targetCamera.clearFlags = _originalClearFlags;
            targetCamera.backgroundColor = _originalBgColor;
        }
    }

    private void MuteAudioImmediate()
    {
        var all = FindObjectsOfType<AudioSource>(true);
        foreach (var src in all)
        {
            if (src == hintSound || IsWhitelisted(src.name))
                continue;

            if (!_originalVolumes.ContainsKey(src))
                _originalVolumes[src] = src.volume;

            src.volume = 0f;
        }

        CleanupDestroyedSources();
    }

    private void UnmuteAudioImmediate()
    {
        CleanupDestroyedSources();

        foreach (var kvp in _originalVolumes)
        {
            if (kvp.Key != null)
                kvp.Key.volume = kvp.Value;
        }

        _originalVolumes.Clear();
    }

    private void CleanupDestroyedSources()
    {
        List<AudioSource> toRemove = null;

        foreach (var src in _originalVolumes.Keys)
        {
            if (src == null)
            {
                toRemove ??= new List<AudioSource>();
                toRemove.Add(src);
            }
        }

        if (toRemove != null)
        {
            foreach (var src in toRemove)
                _originalVolumes.Remove(src);
        }
    }

    private bool IsWhitelisted(string srcName)
    {
        if (audioWhitelist == null) return false;
        foreach (var w in audioWhitelist)
            if (!string.IsNullOrEmpty(w) && srcName.Contains(w))
                return true;
        return false;
    }
}
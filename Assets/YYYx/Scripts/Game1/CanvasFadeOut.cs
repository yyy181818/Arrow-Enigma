using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFadeOut : MonoBehaviour
{
    [Header("淡出时长（秒）")]
    public float duration = 1.2f;

    [Header("淡出完成后销毁的物体（留空 = 销毁挂脚本的这个物体）")]
    public GameObject destroyTarget;

    [Header("淡出曲线（横轴时间、纵轴 alpha）")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    bool _running;

    public bool IsFading => _running;

    public void FadeOutAndDestroy()
    {
        if (_running) return;
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        _running = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        float from = group.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(t / Mathf.Max(0.01f, duration)));
            group.alpha = Mathf.Lerp(from, 0f, k);
            yield return null;
        }

        group.alpha = 0f;

        Destroy(destroyTarget != null ? destroyTarget : gameObject);
    }
}
using UnityEngine;

/// <summary>
/// 爆炸特效组：默认隐藏，触发时显示并播放所有子粒子系统。
///
/// 用法：挂在放了所有特效的父物体上，
/// 炸弹爆炸时调 Play()，之后场景重载，特效自动随场景消失。
/// </summary>
public class ExplosionGroup : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>显示并播放全部特效</summary>
    public void Play()
    {
        gameObject.SetActive(true);

        ParticleSystem[] all = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            all[i].Clear();
            all[i].Play(true);
        }
    }
}

// using DG.Tweening;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// //using VLB;
// //[ExecuteInEditMode]
// //[ExecuteAlways]
//
// public class LightControl : MonoBehaviour
// {
//     //[Range(0.0f,1.0f)]
//     public float Light = 0;
//
//     //public VolumetricLightBeamSD SD;
//     // Start is called before the first frame update
//     void Start()
//     {
//         
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         
//        
//         //SD.intensityInside = Light;
//
//     }
//
//     public void FadeInToOut1()
//     {
//         Light = 0;
//         Sequence sq = DOTween.Sequence();
//         sq.AppendCallback(() => { DOTween.To(() => Light, x => {  Light = x; Debug.Log("-----"+Light); Shader.SetGlobalFloat("_Light", Light); }, 1, 2); });
//         sq.AppendInterval(2);
//         sq.AppendCallback(() => { DOTween.To(() => Light, x => { Light = x; Debug.Log("///////" + Light); Shader.SetGlobalFloat("_Light", Light); }, 0, 2f); });
//         sq.Play();
//     }
//
//     public void FadeInToOut2()
//     {
//         Light = 0;
//         Sequence sq = DOTween.Sequence();
//         sq.AppendCallback(() => { DOTween.To(() => Light, x => { Light = x; Debug.Log("-----" + Light); Shader.SetGlobalFloat("_Light", Light); }, 1, 4.8f); });
//         sq.AppendInterval(5);
//         sq.AppendCallback(() => { DOTween.To(() => Light, x => { Light = x; Debug.Log("///////" + Light); Shader.SetGlobalFloat("_Light", Light); }, 0, 3f); });
//         sq.Play();
//     }
// }

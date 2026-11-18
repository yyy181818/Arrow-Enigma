using System.Collections.Generic;
using UnityEngine;
using System;
using static EasingFunction;
using CustomInspector;
using UnityEditor;
//[ExecuteAlways]
public class TransformEffector : MonoBehaviour
{
    [HorizontalLine("001 Main")]
    public bool UseTag = false;
    [ShowIf(nameof(UseTag))]
    [Tag]public string Tag = "Untagged";
    public bool CheckRenderer = false;
    public List<Transform> transforms;

    public bool UseDistance = true;
    [Min(0.01f)]
    public float DistanceStart = 0.1f;
    [Min2(nameof(DistanceStart))]
    public float DistanceEnd = 10.0f;
    public bool InvertEffect = false;
    public float Effect = 0.0f;
    [Min(1.0f)]
    public float EffectHardness = 1.0f;
    public bool DebugDrawRange = false;
    [ShowIf(nameof(DebugDrawRange))]
    [Range(2, 20)]
    public int DrawCount = 10;
    [ShowIf(nameof(DebugDrawRange))]
    [Range(0.0f, 1.0f)]
    public float DrawOpacity = 0.7f;

    [HorizontalLine("002 Location")]
    public bool LocationEnabled = true;
    public Vector3 StartLocationOffset = Vector3.up * -7.0f;
    public bool LocationRandom = true;
    [ShowIf(nameof(LocationRandom))]
    public Vector3 StartLocationRandom = Vector3.zero;
    public Ease LocationEaseType = Ease.Linear;

    public bool FloatAnim = false;
    [ShowIf(nameof(FloatAnim))]
    public float FloatSpeed = 0.5f;
    [ShowIf(nameof(FloatAnim))]
    public float FloatRange = 0.5f;
    [ShowIf(nameof(FloatAnim))]
    public Vector3 FloatDir = Vector3.up;

    [HorizontalLine("003 Rotation")]
    public bool RotationEnabled = true;
    public Vector3 StartRotationOffset = Vector3.up * 180.0f;
    public bool RotationRandom = true;
    [ShowIf(nameof(RotationRandom))]
    public Vector3 StartRotationRandom = Vector3.zero;
    public Ease RotationEaseType = Ease.Linear;

    [HorizontalLine("004 Scale")]
    public bool ScaleEnabled = false;
    public float StartScale = 0.0f;
    public bool ScaleEaseSeparate = false;
    [ShowIfNot(nameof(ScaleEaseSeparate))]
    public Ease ScaleEaseType = Ease.Linear;
    [ShowIf(nameof(ScaleEaseSeparate))]
    public Ease ScaleXEaseType = Ease.Linear;
    [ShowIf(nameof(ScaleEaseSeparate))]
    public Ease ScaleYEaseType = Ease.Linear;
    [ShowIf(nameof(ScaleEaseSeparate))]
    public Ease ScaleZEaseType = Ease.Linear;

    private List<Vector3> origin_locaitions;
    private List<Vector3> origin_rotations;
    private List<Vector3> origin_scales;

    private bool location_enabled;
    private bool rotation_enabled;
    private bool scale_enabled;
        

    void Start()
    {
        origin_locaitions = new List<Vector3>(); 
        origin_rotations = new List<Vector3>();
        origin_scales = new List<Vector3>();
        StoreTransformValues();
        location_enabled = LocationEnabled;
        rotation_enabled = RotationEnabled;
        scale_enabled = ScaleEnabled;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transforms.Count; i++)
        {
            Transform trans = transforms[i];
            Vector3 origin_pos = origin_locaitions[i];
            Vector3 origin_rot = origin_rotations[i];
            Vector3 origin_scale = origin_scales[i];
            float dist = 0f;
            float factor = 0.0f;

            if (UseDistance)
            {
                dist = Vector3.Distance(origin_pos, this.transform.position) + Effect;
                factor = 1.0f - Mathf.Clamp01(Mathf.Pow(Remap(dist, DistanceStart, DistanceEnd, 0.0f, 1.0f),
                    EffectHardness));
                if (InvertEffect)
                {
                    factor = 1.0f - factor;
                }
            }
            else
            {
                dist = 0.0f;
                factor = 0.0f;
            }

            int seed = trans.gameObject.GetEntityId().GetHashCode();
            if (LocationEnabled)
            {
                var newPos = TransformLocation(origin_pos, factor, seed);
                trans.position = newPos;
            }
            if (RotationEnabled){
                var newRot = TransformRotation(origin_rot, factor, seed);
                trans.eulerAngles = newRot;
            }
            if (ScaleEnabled){
                var newScale = TransformScale(origin_scale, factor);
                trans.localScale = newScale;
            }

            if (location_enabled != LocationEnabled || rotation_enabled != RotationEnabled || scale_enabled != ScaleEnabled)
            {
                location_enabled = LocationEnabled;
                rotation_enabled = RotationEnabled;
                scale_enabled = ScaleEnabled;
                ResetToDefault();
            }
        }
    }
    void OnDisable()
    {
        ResetToDefault();
    }
    void OnDestroy()
    {
        transforms.Clear();
        origin_locaitions.Clear();
        origin_rotations.Clear();
        origin_scales.Clear();
    }
    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (!DebugDrawRange)
            return;
        float maxdistance = Mathf.Max(DistanceStart, DistanceEnd);
        float mindistance = Mathf.Min(DistanceStart, DistanceEnd);
        for (int i = 0; i < DrawCount; i++)
        {
            float distance = Mathf.Lerp(mindistance,maxdistance,(float)i / (float)(DrawCount - 1));
            float dist = Vector3.Distance(this.transform.position + Vector3.up * distance, this.transform.position) + Effect;
            dist = 1.0f - Mathf.Clamp01(Mathf.Pow(Remap(dist, DistanceStart, DistanceEnd, 0.0f, 1.0f), EffectHardness));
            if (InvertEffect)
            {
                dist = 1.0f - dist;
            }
            var gizmos_color = Color.Lerp(Color.blue,Color.red, dist);
            gizmos_color.a = DrawOpacity;
            Gizmos.color = gizmos_color;
            Gizmos.DrawWireSphere(transform.position, distance);
        }
    }
    //收集Transform,并储存每个Transform的信息，保存为数组
    void StoreTransformValues()
    {
        if (UseTag)
        {
            transforms = new List<Transform>();
            var gos = GameObject.FindGameObjectsWithTag(Tag);
            foreach (var go in gos)
            {
                if (CheckRenderer == true)
                {
                    MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                    if (meshRenderer == null || !go.activeInHierarchy)
                        continue;
                }
                var trans = go.GetComponent<Transform>();
                transforms.Add(trans);
                origin_locaitions.Add(trans.position);
                origin_rotations.Add(trans.eulerAngles);
                origin_scales.Add(trans.localScale);
            }
        }
        else
        {
            foreach (var trans in transforms)
            {
                origin_locaitions.Add(trans.position);
                origin_rotations.Add(trans.eulerAngles);
                origin_scales.Add(trans.localScale);
            }

        }
    }
    void ResetToDefault()
    {
        for (int i = 0; i < transforms.Count; i++)
        {
            var trans = transforms[i];
            if(trans == null)
                continue;
            var origin_pos = origin_locaitions[i];
            var origin_rot = origin_rotations[i];
            var origin_scale = origin_scales[i];
            trans.position = origin_pos;
            trans.eulerAngles = origin_rot;
            trans.localScale = origin_scale;
        }
    }
    Vector3 TransformLocation(Vector3 origin_pos, float factor, int seed)
    {
        Vector3 startPos = origin_pos + StartLocationOffset;
        if (LocationRandom)
        {
            UnityEngine.Random.InitState(seed);
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            );
            randomOffset = Vector3.Scale(randomOffset, StartLocationRandom);
            startPos = startPos + randomOffset;
        }
        if (FloatAnim)
        {
            float yOffset = Mathf.Sin(Time.time * FloatSpeed + UnityEngine.Random.Range(-10f, 10f)) * (FloatRange * UnityEngine.Random.Range(-1f, 1f));
            startPos += yOffset * FloatDir;
        }
        Vector3 endPos = origin_pos;

        var func = EasingFunction.GetEasingFunction(LocationEaseType);
        var blend = func.Invoke(0.0f,1.0f, factor);

        Vector3 newPos = Vector3.LerpUnclamped(startPos, endPos, blend);
        return newPos;
    }
    Vector3 TransformRotation(Vector3 origin_rot, float factor, int seed)
    {
        Vector3 startRot = origin_rot + StartRotationOffset;
        if (RotationRandom)
        {
            UnityEngine.Random.InitState(seed);
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            );
            randomOffset = Vector3.Scale(randomOffset, StartRotationRandom);
            startRot = startRot + randomOffset;
        }
        Vector3 endRot = origin_rot;

        var func = EasingFunction.GetEasingFunction(RotationEaseType);
        var blend = func.Invoke(0.0f, 1.0f, factor);

        Vector3 newRot = Vector3.LerpUnclamped(startRot, endRot, blend);
        return newRot;
    }
    Vector3 TransformScale(Vector3 origin_scale, float factor)
    {
        if (!ScaleEaseSeparate)
        {
            Vector3 startScale = origin_scale * StartScale;
            Vector3 endScale = origin_scale;

            var func = EasingFunction.GetEasingFunction(ScaleEaseType);
            var blend = func.Invoke(0.0f, 1.0f, factor);
            Vector3 newScale = Vector3.LerpUnclamped(startScale, endScale, blend);
            return newScale;
        }
        else
        {
            Vector3 startScale = origin_scale * StartScale;
            Vector3 endScale = origin_scale;
            var funcX = EasingFunction.GetEasingFunction(ScaleXEaseType);
            var blendX = funcX.Invoke(0.0f, 1.0f, factor);
            var funcY = EasingFunction.GetEasingFunction(ScaleYEaseType);
            var blendY = funcY.Invoke(0.0f, 1.0f, factor);
            var funcZ = EasingFunction.GetEasingFunction(ScaleZEaseType);
            var blendZ = funcZ.Invoke(0.0f, 1.0f, factor);
            float newScaleX = Mathf.LerpUnclamped(startScale.x, endScale.x, blendX);
            float newScaleY = Mathf.LerpUnclamped(startScale.y, endScale.y, blendY);
            float newScaleZ = Mathf.LerpUnclamped(startScale.z, endScale.z, blendZ);
            Vector3 newScale = new Vector3(newScaleX, newScaleY, newScaleZ);
            return newScale;
        }
    }
    float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        value = Mathf.Clamp(value, fromMin, fromMax);
        float normalizedValue = (value - fromMin) / (fromMax - fromMin);
        float remappedValue = Mathf.Lerp(toMin, toMax, normalizedValue);
        return remappedValue;
    }
}

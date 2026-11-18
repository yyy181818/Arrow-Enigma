using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TexaXRFramework.Core
{
    [AddComponentMenu("Transform/Optimized Rotation")]
    [RequireComponent(typeof(Transform))]
    public class PT_ObjectRotationComponent : MonoBehaviour
    {
        [Header("整体设置")]
        [Tooltip("是否启用旋转")]
        public bool isRotating = true;

        [Header("X轴旋转设置")]
        [Tooltip("是否启用X轴旋转")]
        public bool rotateX = true;
        [Tooltip("X轴旋转速度（度/秒）")]
        [Range(0, 360)]
        public float xSpeed = 30f;
        [Tooltip("X轴旋转方向（正向/反向）")]
        public bool xInvert = false;

        [Header("Y轴旋转设置")]
        [Tooltip("是否启用Y轴旋转")]
        public bool rotateY = false;
        [Tooltip("Y轴旋转速度（度/秒）")]
        [Range(0, 360)]
        public float ySpeed = 30f;
        [Tooltip("Y轴旋转方向（正向/反向）")]
        public bool yInvert = false;

        [Header("Z轴旋转设置")]
        [Tooltip("是否启用Z轴旋转")]
        public bool rotateZ = false;
        [Tooltip("Z轴旋转速度（度/秒）")]
        [Range(0, 360)]
        public float zSpeed = 30f;
        [Tooltip("Z轴旋转方向（正向/反向）")]
        public bool zInvert = false;

        [Header("旋转空间")]
        public Space rotationSpace = Space.Self;

        // 缓存旋转增量
        private Vector3 _rotationDelta;
        // 缓存Transform组件
        private Transform _cachedTransform;
        // 标记是否有任何轴向启用旋转
        private bool _hasActiveRotation;

        private void Awake()
        {
            _cachedTransform = transform;
            UpdateRotationValues();
        }

        private void OnValidate()
        {
            UpdateRotationValues();
        }

        /// <summary>
        /// 更新旋转增量值和活动状态
        /// </summary>
        private void UpdateRotationValues()
        {
            // 计算每个轴向的实际旋转速度（包含方向）
            float xDelta = rotateX ? (xInvert ? -xSpeed : xSpeed) : 0f;
            float yDelta = rotateY ? (yInvert ? -ySpeed : ySpeed) : 0f;
            float zDelta = rotateZ ? (zInvert ? -zSpeed : zSpeed) : 0f;

            _rotationDelta = new Vector3(xDelta, yDelta, zDelta);
            _hasActiveRotation = (xDelta != 0f) || (yDelta != 0f) || (zDelta != 0f);
        }

        private void Update()
        {
            if (!isRotating || !_hasActiveRotation)
                return;

            // 计算每帧旋转量（乘以deltaTime确保帧率无关）
            Vector3 rotation = _rotationDelta * Time.deltaTime;

            // 应用旋转
            _cachedTransform.Rotate(rotation, rotationSpace);
        }

        /// <summary>
        /// 运行时刷新旋转设置
        /// </summary>
        public void RefreshRotationSettings()
        {
            UpdateRotationValues();
        }
    }
}

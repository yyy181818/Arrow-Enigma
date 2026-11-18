using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 变色物体的扩展状态数据
    /// 存储物体颜色和交互状态信息
    /// </summary>
    [System.Serializable]
    public class ColorObjectExtendedState
    {
        [Tooltip("物体当前颜色")]
        public Color CurrentColor = Color.white;

        [Tooltip("上次点击的玩家ID")]
        public string LastClickerId = "";

        [Tooltip("点击次数统计")]
        public int ClickCount = 0;

        [Tooltip("是否处于闪烁状态")]
        public bool IsBlinking = false;
    }

    /// <summary>
    /// 鼠标点击变色物体组件（网络同步版）
    /// 功能：鼠标点击后随机改变颜色，并同步到所有客户端
    /// 基于LBENetworkInteractiveSyncComponent实现多客户端状态一致
    /// </summary>
    [RequireComponent(typeof(Renderer), typeof(Collider))]
    public class ClickColorObjectComponent : LBENetworkInteractiveSyncComponent<ColorObjectExtendedState>
    {
        [Header("颜色配置")]
        [Tooltip("默认初始颜色")]
        public Color DefaultColor = Color.white;

        [Tooltip("点击后是否随机颜色（否则使用预设颜色列表）")]
        public bool UseRandomColors = true;

        [Tooltip("预设颜色列表（当不使用随机颜色时）")]
        public Color[] PresetColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };

        [Header("交互效果")]
        [Tooltip("点击时的缩放效果比例")]
        [Range(1.0f, 1.5f)]
        public float ClickScale = 1.1f;

        [Tooltip("缩放效果持续时间（秒）")]
        public float ScaleDuration = 0.2f;

        // 组件引用与状态变量
        private Renderer _objectRenderer;
        private Vector3 _originalScale;
        private float _scaleTimer;
        private bool _isScaling;

        /// <summary>
        /// 组件唤醒初始化
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // 初始化扩展状态
            _currentExtendedState = new ColorObjectExtendedState
            {
                CurrentColor = DefaultColor,
                ClickCount = 0,
                IsBlinking = false
            };

            // 配置同步模式（颜色变化是状态驱动，适合绝对模式）
            SyncMode = SyncMode.StateAndAbsoluteTransform;
            PositionSpace = PositionSyncSpace.World;

            // 缓存组件引用
            _objectRenderer = GetComponent<Renderer>();
            _originalScale = transform.localScale;

            // 初始化显示
            UpdateObjectColor();
        }

        /// <summary>
        /// 手动更新物体状态（实现抽象方法）
        /// 处理缩放动画等临时状态
        /// </summary>
        protected override void UpdateInteractiveStates()
        {
            // 处理点击缩放动画
            if (_isScaling)
            {
                _scaleTimer += Time.deltaTime;
                float t = _scaleTimer / ScaleDuration;

                // 缩放动画曲线：先放大后缩小
                float scaleFactor = Mathf.Lerp(1f, ClickScale, t) * Mathf.Lerp(ClickScale, 1f, t);
                transform.localScale = _originalScale * scaleFactor;

                if (t >= 1f)
                {
                    _isScaling = false;
                    transform.localScale = _originalScale; // 恢复原始大小
                }
            }

            // 处理闪烁效果（如果启用）
            if (_currentExtendedState.IsBlinking)
            {
                float blinkSpeed = 2f;
                float intensity = Mathf.PingPong(Time.time * blinkSpeed, 0.5f) + 0.5f;
                _objectRenderer.material.color = Color.Lerp(
                    _currentExtendedState.CurrentColor,
                    Color.white,
                    intensity
                );
            }
        }

        /// <summary>
        /// 状态变化回调（实现抽象方法）
        /// 当接收到同步数据时更新本地显示
        /// </summary>
        protected override void OnStateChanged(InteractionState newBaseState, ColorObjectExtendedState newExtendedState)
        {
            // 更新物体颜色
            UpdateObjectColor();

            // 播放点击反馈动画（所有客户端都会执行）
            if (newBaseState == InteractionState.Activated)
            {
                PlayClickFeedback();
            }
        }

        /// <summary>
        /// 鼠标点击检测（Unity内置方法）
        /// </summary>
        private void OnMouseDown()
        {
            // 只有网络连接时才处理点击
            if (LBENetworkSyncManager.Instance.IsNetworkConnected)
            {
                ChangeObjectColor();
            }
        }

        /// <summary>
        /// 改变物体颜色（核心交互逻辑）
        /// </summary>
        private void ChangeObjectColor()
        {
            // 生成新颜色
            Color newColor = UseRandomColors
                ? new Color(Random.value, Random.value, Random.value)
                : GetNextPresetColor();

            // 更新状态
            _currentExtendedState.CurrentColor = newColor;
            _currentExtendedState.ClickCount++;
            _currentBaseState = InteractionState.Activated;

            // 播放点击反馈
            PlayClickFeedback();

            // 标记为需要同步
            MarkAsDirty();
            // 手动触发状态更新
            ManualUpdateInteractiveStates();
        }

        /// <summary>
        /// 获取下一个预设颜色（循环使用）
        /// </summary>
        private Color GetNextPresetColor()
        {
            if (PresetColors.Length == 0) return Color.white;

            // 根据点击次数取模，循环切换预设颜色
            int index = _currentExtendedState.ClickCount % PresetColors.Length;
            return PresetColors[index];
        }

        /// <summary>
        /// 更新物体颜色显示
        /// </summary>
        private void UpdateObjectColor()
        {
            if (_objectRenderer != null)
            {
                _objectRenderer.material.color = _currentExtendedState.CurrentColor;
            }
        }

        /// <summary>
        /// 播放点击反馈效果
        /// </summary>
        private void PlayClickFeedback()
        {
            // 启动缩放动画
            _isScaling = true;
            _scaleTimer = 0;
        }

        /// <summary>
        /// 外部控制：设置闪烁状态
        /// </summary>
        /// <param name="blinking">是否闪烁</param>
        public void SetBlinking(bool blinking)
        {
            _currentExtendedState.IsBlinking = blinking;
            MarkAsDirty();
            ManualUpdateInteractiveStates();
        }

        /// <summary>
        /// 外部控制：重置物体状态
        /// </summary>
        public void ResetState()
        {
            _currentExtendedState.CurrentColor = DefaultColor;
            _currentExtendedState.ClickCount = 0;
            _currentExtendedState.IsBlinking = false;
            _currentBaseState = InteractionState.Idle;
            UpdateObjectColor();
            MarkAsDirty();
        }
    }
}

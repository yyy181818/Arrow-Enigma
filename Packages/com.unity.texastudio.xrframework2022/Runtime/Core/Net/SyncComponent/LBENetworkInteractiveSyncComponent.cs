using Newtonsoft.Json;
using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 互动同步组件基类（手动更新模式）
    /// 核心功能：提供跨客户端的物体状态与位置同步机制，支持两种同步模式
    /// 设计特点：移除自动状态更新，改为手动触发，增强更新时机的可控性
    /// 适用场景：所有需要在多客户端间保持状态一致的互动物体（门、道具、开关等）
    /// </summary>
    /// <typeparam name="TExtendedState">扩展状态类型（需为带无参构造函数的类）
    /// 用途：存储物体特有的自定义状态数据（如门的开关百分比、道具的持有状态）
    /// </typeparam>
    [DisallowMultipleComponent]
    public abstract class LBENetworkInteractiveSyncComponent<TExtendedState> : MonoBehaviour
        where TExtendedState : class, new()
    {
        [Header("基础配置")]
        [Tooltip("物体唯一标识（场景内必须唯一）\n留空将自动生成（格式：层级路径+随机字符串）")]
        public string ObjectId;

        [Tooltip("同步模式选择：\n" +
                 "1. StateAndAbsoluteTransform - 状态+绝对位置：直接同步完整位置旋转（适合静态物体）\n" +
                 "2. StateAndDeltaTransform - 状态+差值：同步位置旋转变化量（适合动态物体）")]
        [EnumAttirbute("当前基础状态")]
        [Space(15)]
        public SyncMode SyncMode = SyncMode.StateAndAbsoluteTransform;

        [Tooltip("坐标空间选择：\n" +
                 "1. World - 世界坐标（适合独立存在的物体，如场景中的门）\n" +
                 "2. Local - 本地坐标（适合父物体下的子物体，如角色手持的道具）")]
        [EnumAttirbute("坐标空间")]
        [Space(15)]
        public PositionSyncSpace PositionSpace = PositionSyncSpace.World;

        [Header("同步参数")]
        [Tooltip("同步频率（次/秒）：8-15之间平衡性能与响应速度\n值越高同步越及时，但网络开销越大")]
        [Header("同步频率（次/秒）")]
        [Range(8, 15)]
        public float SyncRate = 10f;

        [Tooltip("位置变化阈值（米）：仅差值模式生效\n超过此值才触发位置同步，避免微小抖动频繁同步")]
        [Header("位置变化阈值（米）")]
        public float PositionThreshold = 0.01f;

        [Tooltip("旋转变化阈值（度）：仅差值模式生效\n超过此值才触发旋转同步，避免微小抖动频繁同步")]
        [Header("旋转变化阈值（度）")]
        public float RotationThreshold = 0.5f;

        // 核心状态变量（保护级别，允许子类直接访问和修改）
        /// <summary>
        /// 当前基础状态（对应InteractionState枚举）
        /// 用途：标识物体的核心状态（如激活、锁定、空闲等）
        /// 同步策略：每次状态变化时会自动同步到其他客户端
        /// </summary>
        protected InteractionState _currentBaseState = InteractionState.Idle;

        /// <summary>
        /// 当前扩展状态（存储物体自定义的详细信息）
        /// 用途：存储模式无法覆盖的自定义状态（如门的打开百分比、道具的持有者ID）
        /// 同步策略：通过JSON序列化后同步，需在子类中定义具体结构
        /// </summary>
        protected TExtendedState _currentExtendedState;

        /// <summary>
        /// 上次发送的同步数据缓存
        /// 用途：1. 对比状态变化 2. 计算位置旋转差值（差值模式）3. 避免重复发送相同数据
        /// </summary>
        protected LBEInteractiveSyncData _lastSentData = new LBEInteractiveSyncData();

        // 差值计算基准（私有变量，组件内部维护，不随网络传输）
        /// <summary>
        /// 差值计算基准位置（仅差值模式内部使用）
        /// 用途：存储上次同步时的绝对位置，作为下次计算位置差值的基准
        /// 优势：不在网络数据中传输，减少带宽占用，且避免不同客户端基准值不一致问题
        /// </summary>
        private Vector3 _deltaBasePosition;

        /// <summary>
        /// 差值计算基准旋转（仅差值模式内部使用）
        /// 用途：存储上次同步时的绝对旋转，作为下次计算旋转差值的基准
        /// </summary>
        private Quaternion _deltaBaseRotation;

        // 同步控制变量
        /// <summary>
        /// 同步间隔（秒）= 1 / SyncRate
        /// 用途：控制同步数据发送的最小时间间隔，避免高频发送导致的性能问题
        /// </summary>
        protected float _syncInterval;

        /// <summary>
        /// 上次同步时间戳（记录上次发送数据的时间）
        /// 用途：与当前时间对比，判断是否达到下次同步的时间条件
        /// </summary>
        protected float _lastSyncTime;

        /// <summary>
        /// 脏标记（true=需要立即同步）
        /// 用途：标记物体状态已发生重要变化（如玩家互动后），确保下次达到时间间隔时优先同步
        /// 使用方式：子类在关键状态变化后调用MarkAsDirty()设为true
        /// </summary>
        protected bool _isDirty;

        /// <summary>
        /// 组件注册状态（true=已注册到同步管理器）
        /// 用途：避免未注册的组件参与同步流程，确保只有有效组件能收发数据
        /// </summary>
        protected bool _isRegistered;

        /// <summary>
        /// 缓存的Transform组件
        /// 用途：优化性能，减少每帧调用GetComponent&lt;Transform&gt;()的开销
        /// </summary>
        protected Transform _cachedTransform;

        /// <summary>
        /// 组件唤醒初始化（Unity生命周期）
        /// 执行时机：组件被加载时调用（早于Start）
        /// 主要功能：初始化基础变量、缓存组件引用、生成唯一标识
        /// </summary>
        protected virtual void Awake()
        {
            _cachedTransform = transform; // 缓存Transform组件，优化性能
            _currentExtendedState = new TExtendedState(); // 初始化扩展状态实例
            _syncInterval = 1f / SyncRate; // 计算同步间隔（秒）

            // 初始化差值计算基准（首次使用当前位置旋转）
            _deltaBasePosition = GetCurrentPosition();
            _deltaBaseRotation = GetCurrentRotation();

            // 初始化上次发送数据的状态信息
            _lastSentData.State = (int)_currentBaseState;
            _lastSentData.Data = JsonConvert.SerializeObject(_currentExtendedState);
        }

        /// <summary>
        /// 组件启动注册（Unity生命周期）
        /// 执行时机：第一帧更新前调用
        /// 主要功能：将组件注册到同步管理器，加入同步流程
        /// </summary>
        protected virtual void Start()
        {
            RegisterToManager();
        }

        /// <summary>
        /// 注册到同步管理器
        /// 功能：将组件加入同步管理器的管理列表，使其能正常收发同步数据
        /// 注意：仅在未注册状态下执行，避免重复注册
        /// </summary>
        protected virtual void RegisterToManager()
        {
            if (!_isRegistered)
            {
                LBENetworkSyncManager.Instance.InteractiveSyncService.RegisterComponent(this);
                _isRegistered = true;
            }
        }

        protected virtual void Update()
        {
            if (!_isRegistered) return; // 未注册的组件不参与同步

            // 当满足同步条件时发送数据
            if (ShouldSendSyncData())
            {
                SendSyncData(); // 发送同步数据
                _lastSyncTime = Time.time; // 更新上次同步时间
                _isDirty = false; // 发送后清除脏标记
            }
        }

        /// <summary>
        /// 手动更新互动状态（供子类调用）
        /// 功能：触发状态更新逻辑（由子类实现的UpdateInteractiveStates()）
        /// 使用场景：
        /// 1. 玩家交互后（如点击门、拾取道具）
        /// 2. 动画关键帧（如门开/关到特定角度）
        /// 3. 物理更新后（如道具停止移动时）
        /// </summary>
        public void ManualUpdateInteractiveStates()
        {
            if (_isRegistered) // 仅在已注册状态下执行
            {
                UpdateInteractiveStates();
            }
        }

        /// <summary>
        /// 判断是否需要发送同步数据
        /// 核心逻辑：综合网络状态、同步间隔、状态变化、脏标记等条件
        /// </summary>
        /// <returns>true=需要发送同步数据，false=不需要</returns>
        protected virtual bool ShouldSendSyncData()
        {
            // 网络未连接时不发送数据
            if (!LBENetworkSyncManager.Instance.IsNetworkConnected)
                return false;

            // 检查是否达到同步时间间隔
            bool isTimeReady = Time.time - _lastSyncTime >= _syncInterval;

            // 脏标记为true且时间就绪：强制发送（如玩家刚完成互动）
            if (_isDirty && isTimeReady)
                return true;

            // 检查状态是否变化（基础状态+扩展状态）
            bool isStateChanged = CheckStateChanged();

            // 根据同步模式判断是否需要发送
            switch (SyncMode)
            {
                case SyncMode.StateAndAbsoluteTransform:
                    // 绝对位置模式：仅状态变化时发送
                    return isTimeReady && isStateChanged;
                case SyncMode.StateAndDeltaTransform:
                    // 差值模式：状态变化 或 位置旋转超阈值 时发送
                    return isTimeReady && (isStateChanged || CheckTransformChanged());
                default:
                    return false;
            }
        }

        /// <summary>
        /// 检查状态是否变化（基础状态+扩展状态）
        /// 功能：对比当前状态与上次发送的状态，判断是否需要同步
        /// </summary>
        /// <returns>true=状态有变化，false=状态无变化</returns>
        protected virtual bool CheckStateChanged()
        {
            // 基础状态枚举变化检测
            bool isBaseStateChanged = _currentBaseState != (InteractionState)_lastSentData.State;

            // 扩展状态JSON序列化后对比（处理复杂自定义数据）
            string currentExtendedJson = JsonConvert.SerializeObject(_currentExtendedState);
            bool isExtendedStateChanged = currentExtendedJson != _lastSentData.Data;

            // 任一状态变化即返回true
            return isBaseStateChanged || isExtendedStateChanged;
        }

        /// <summary>
        /// 检查位置旋转是否变化（仅差值模式生效）
        /// 功能：对比当前位置旋转与内部基准值，超过阈值则判定为需要同步
        /// </summary>
        /// <returns>true=位置旋转有变化，false=无变化</returns>
        protected virtual bool CheckTransformChanged()
        {
            Vector3 currentPos = GetCurrentPosition();
            Quaternion currentRot = GetCurrentRotation();

            // 与内部维护的基准值对比（差值模式的核心逻辑）
            bool isPosChanged = Vector3.Distance(currentPos, _deltaBasePosition) > PositionThreshold;
            bool isRotChanged = Quaternion.Angle(currentRot, _deltaBaseRotation) > RotationThreshold;

            return isPosChanged || isRotChanged;
        }

        /// <summary>
        /// 发送同步数据到网络
        /// 功能：构建同步数据对象，根据同步模式填充内容，通过管理器发送
        /// </summary>
        protected virtual void SendSyncData()
        {
            // 构建同步数据对象
            var syncData = new LBEInteractiveSyncData
            {
                ObjectId = ObjectId, // 目标物体ID
                State =(int) _currentBaseState, // 基础状态
                Data = JsonConvert.SerializeObject(_currentExtendedState) // 扩展状态（JSON序列化）
            };

            // 获取当前位置旋转（两种模式都需要）
            Vector3 currentPos = GetCurrentPosition();
            Quaternion currentRot = GetCurrentRotation();

            if (SyncMode == SyncMode.StateAndAbsoluteTransform)
            {
                // 绝对位置模式：直接存储当前绝对位置旋转
                syncData.Position = currentPos;
                syncData.Rotation = currentRot;
            }
            else // 差值模式
            {
                // 使用内部维护的基准值计算差值
                syncData.Position = currentPos - _deltaBasePosition;
                syncData.Rotation = currentRot * Quaternion.Inverse(_deltaBaseRotation);

                // 更新内部基准值（下次差值计算的基础）
                _deltaBasePosition = currentPos;
                _deltaBaseRotation = currentRot;
            }

            // 通过同步管理器发送数据
            //LBEInteractiveSyncManager.Instance.SendSyncData(syncData);
            // 更新上次发送数据缓存
            _lastSentData = syncData;
        }

        /// <summary>
        /// 处理接收到的同步数据（实现ILBEInteractiveSyncComponent接口）
        /// 执行时机：当同步管理器接收到该物体的同步数据时调用
        /// 功能：解析数据并更新本地状态和位置旋转
        /// </summary>
        /// <param name="syncData">从网络接收到的同步数据</param>
        public virtual void HandleSyncData(LBEInteractiveSyncData syncData)
        {
            // 先更新本地状态数据（两种模式都需要）
            UpdateLocalStateFromSyncData(syncData);

            // 根据同步模式更新位置旋转
            if (SyncMode == SyncMode.StateAndAbsoluteTransform)
            {
                UpdateLocalTransformByAbsolute(syncData);
            }
            else
            {
                UpdateLocalTransformByDelta(syncData);
            }
        }

        /// <summary>
        /// 从同步数据更新本地状态（基础状态+扩展状态）
        /// 功能：将接收到的状态数据应用到本地，并触发状态变化回调
        /// </summary>
        /// <param name="data">从网络接收到的同步数据</param>
        protected virtual void UpdateLocalStateFromSyncData(LBEInteractiveSyncData data)
        {
            // 检查基础状态是否变化
            bool baseStateChanged = _currentBaseState != (InteractionState)data.State;
            _currentBaseState = (InteractionState)data.State;

            // 检查扩展状态是否变化
            bool extendedStateChanged = false;
            if (!string.IsNullOrEmpty(data.Data))
            {
                try
                {
                    // 反序列化扩展状态（自定义数据）
                    var newExtendedState = JsonConvert.DeserializeObject<TExtendedState>(data.Data);
                    if (newExtendedState != null)
                    {
                        _currentExtendedState = newExtendedState;
                        extendedStateChanged = true;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"解析扩展状态失败: {ex.Message}，数据: {data.Data}");
                }
            }

            // 状态变化时通知子类处理（如播放动画、更新UI）
            if (baseStateChanged || extendedStateChanged)
            {
                OnStateChanged(_currentBaseState, _currentExtendedState);
            }
        }

        /// <summary>
        /// 按绝对位置更新本地变换（绝对模式专用）
        /// 功能：直接将本地位置旋转设置为同步数据中的绝对坐标
        /// 适用场景：位置变化少的物体（如门、开关），确保状态强一致性
        /// </summary>
        protected virtual void UpdateLocalTransformByAbsolute(LBEInteractiveSyncData data)
        {
            if (PositionSpace == PositionSyncSpace.World)
            {
                _cachedTransform.position = data.Position;
                _cachedTransform.rotation = data.Rotation;
            }
            else
            {
                _cachedTransform.localPosition = data.Position;
                _cachedTransform.localRotation = data.Rotation;
            }

            // 同步更新内部基准值（确保后续差值计算准确，兼容模式切换）
            if (SyncMode == SyncMode.StateAndDeltaTransform)
            {
                _deltaBasePosition = GetCurrentPosition();
                _deltaBaseRotation = GetCurrentRotation();
            }
        }

        /// <summary>
        /// 按差值更新本地变换（差值模式专用）
        /// 功能：基于本地当前值叠加同步数据中的差值，计算新位置旋转
        /// 适用场景：频繁移动的物体（如拖拽道具），使移动更平滑
        /// </summary>
        protected virtual void UpdateLocalTransformByDelta(LBEInteractiveSyncData data)
        {
            if (PositionSpace == PositionSyncSpace.World)
            {
                // 世界坐标：当前位置 + 位置差值
                _cachedTransform.position += data.Position;
                // 世界旋转：当前旋转 * 旋转差值（四元数乘法表示旋转叠加）
                _cachedTransform.rotation = _cachedTransform.rotation * data.Rotation;
            }
            else
            {
                // 本地坐标：当前本地位置 + 位置差值
                _cachedTransform.localPosition += data.Position;
                // 本地旋转：当前本地旋转 * 旋转差值
                _cachedTransform.localRotation = _cachedTransform.localRotation * data.Rotation;
            }

            // 更新内部基准值（保持与当前状态一致）
            _deltaBasePosition = GetCurrentPosition();
            _deltaBaseRotation = GetCurrentRotation();
        }

        /// <summary>
        /// 获取当前位置（根据配置的坐标空间）
        /// 功能：统一位置获取接口，屏蔽世界/本地坐标的差异
        /// </summary>
        /// <returns>当前位置（世界坐标或本地坐标）</returns>
        protected virtual Vector3 GetCurrentPosition()
        {
            return PositionSpace == PositionSyncSpace.World
                ? _cachedTransform.position
                : _cachedTransform.localPosition;
        }

        /// <summary>
        /// 获取当前旋转（根据配置的坐标空间）
        /// 功能：统一旋转获取接口，屏蔽世界/本地坐标的差异
        /// </summary>
        /// <returns>当前旋转（世界坐标或本地坐标）</returns>
        protected virtual Quaternion GetCurrentRotation()
        {
            return PositionSpace == PositionSyncSpace.World
                ? _cachedTransform.rotation
                : _cachedTransform.localRotation;
        }

        /// <summary>
        /// 标记为需要立即同步（脏标记）
        /// 功能：将脏标记设为true，确保下次达到时间间隔时优先发送数据
        /// 使用场景：玩家互动后（如点击、拾取、使用），确保关键操作及时同步
        /// </summary>
        public virtual void MarkAsDirty() => _isDirty = true;

        /// <summary>
        /// 更新互动状态（抽象方法，子类必须实现）
        /// 功能：包含物体的核心状态更新逻辑（如门的开关动画、道具的拖拽移动）
        /// 调用时机：由子类通过ManualUpdateInteractiveStates()手动触发
        /// </summary>
        protected abstract void UpdateInteractiveStates();

        /// <summary>
        /// 状态变化回调（抽象方法，子类必须实现）
        /// 功能：处理状态变化后的业务逻辑（如播放动画、触发音效、更新UI）
        /// 调用时机：本地状态被同步数据更新时自动触发
        /// </summary>
        /// <param name="newBaseState">新的基础状态</param>
        /// <param name="newExtendedState">新的扩展状态</param>
        protected abstract void OnStateChanged(InteractionState newBaseState, TExtendedState newExtendedState);

        /// <summary>
        /// 组件销毁时注销（Unity生命周期）
        /// 功能：从同步管理器移除组件，避免组件销毁后仍接收消息导致错误
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_isRegistered)
            {
                LBENetworkSyncManager.Instance.InteractiveSyncService.UnregisterComponent(this);
            }
        }
    }
}

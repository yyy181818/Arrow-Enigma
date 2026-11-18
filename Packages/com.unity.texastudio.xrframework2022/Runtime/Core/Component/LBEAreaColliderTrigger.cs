using NS_LBEShareData;
using UnityEngine;
using UnityEngine.Playables;

namespace TexaXRFramework.Core
{
    /// <summary>
    /// 区域触发器
    /// </summary>
    public class LBEAreaColliderTrigger : MonoBehaviour
    {
   
        [EnumAttirbute("触发器类型")]
        [Space(10)]
        /// <summary>
        /// 触发器类型：
        /// 队伍触发；
        /// 抢占式触发；
        /// 定时器触发；
        /// </summary>
        public LBEAreaTriggerType TriggerType = LBEAreaTriggerType.TeamTrigger;
        
        [Header("定时触发(秒)")]
        /// <summary>
        /// 计时自动触发类型(秒)
        /// </summary>
        public int DelyTime = 180;

        [Header("附带参数")]
        /// <summary>
        /// 触发参数
        /// </summary>
        public string Param;
        
        [Header("延时激活触发区域(秒)")]
        /// <summary>
        /// 触发器延时激活时间：就是进入区域，需要等多久这个触发器才会生效；
        /// </summary>
        public int DelyShowTime;

        private LBEAreaDefine _parent;


        private PlayableDirector playableDirector;
        private bool _timelineeventTriggered = false;
        private void Awake()
        {
            if (DelyShowTime > 0)
            {
                gameObject.SetActive(false);
            }            
        }

        void Start()
        {
            _parent = GetComponentInParent<LBEAreaDefine>();
            if (null == _parent)
            {
                Debug.LogError($"[区域触发器] 无法找到父对象");
            }
        }



        public void OnTriggerEnter(Collider other)
        {
            if (_parent != null && _parent.gameObject.activeSelf)
            {
                //_eventService.Dispatch(LocalEventCommad.AreaEvent_Trigger,name,other,true);
            }
        }
        
        public void OnTriggerExit(Collider other)
        {
            if (_parent != null && _parent.gameObject.activeSelf)
            {
                //_eventService.Dispatch(LocalEventCommad.AreaEvent_Trigger,name,other,false);
            }
        }
        public void OnDestroy()
        {
            if (playableDirector != null)
            {
                //playableDirector.stopped -= OnTimelineStopped;
            }
        }
    }

}

using UnityEngine;
namespace TexaXRFramework.Core
{

    /// <summary>
    /// 区域
    /// </summary>
    public class LBEArea
    {
        /// <summary>
        /// 归属的场景
        /// </summary>
        public LBEStageLogicBase Family;
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName;
        /// <summary>
        /// 区域Mono对象
        /// </summary>
        public GameObject AreaObject;
        /// <summary>
        /// 自定义区域
        /// </summary>
        protected LBEAreaDefine _areaDefineGo;

        public LBEArea(string areaName, LBEStageLogicBase stage, GameObject gObj)
        {
            Family = stage;
            AreaObject = gObj;
            AreaName = areaName;

            if (gObj != null)
            {
                _areaDefineGo = gObj.GetComponent<LBEAreaDefine>();
            }
        }
        public void OnInit()
        {
            AreaObject.gameObject.SetActive(true);
        }


        /// <summary>
        /// 判断该点是否在内容边界内
        /// </summary>
        /// <param name="pos">点</param>
        /// <param name="areaCenterPoint">区域中心</param>
        /// <returns>判断结果</returns>
        public virtual bool OnIsContentBoundaryInside(Vector3 pos, out Vector2 areaCenterPoint)
        {
            if (_areaDefineGo != null)
            {
                return _areaDefineGo.IsContentBoundaryInside(pos, out areaCenterPoint);
            }

            areaCenterPoint = new Vector2(pos.x, pos.z);

            return false;
        }

        /// <summary>
        /// 强制显示内容区域
        /// </summary>
        public virtual void ForceShowContentArea()
        {
            _areaDefineGo.ForceShowContentArea();
        }

        /// <summary>
        /// 强制恢复内容区域显示状态
        /// </summary>
        public virtual void ForceRecoveryContentArea()
        {
            _areaDefineGo.ForceRecoveryContentArea();
        }

        public virtual void OnDispose()
        {
            if (AreaObject != null)
            {
                AreaObject.gameObject.SetActive(false);
            }
        }
    }
}

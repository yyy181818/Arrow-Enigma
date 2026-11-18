using UnityEngine;
namespace TexaXRFramework.Core
{
    public abstract class LBEIndicatorBase : MonoBehaviour 
    {
        protected virtual void Start()
        {
           LBEIndicatorManager.Instance.SetIndicator(this);
        }

        public virtual void OnShowIndicator(bool show, Vector3 startPos, Vector3 endPos) { }

    }
}

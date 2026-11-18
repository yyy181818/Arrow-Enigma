using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// UI朝向玩家位置
    /// </summary>
    public class LBEUIFacePlayerTransform : MonoBehaviour
    {
        private Transform m_PlayerTransform;
        private float m_Offset = 180f;
        private void Start()
        {
            m_PlayerTransform = Camera.main.transform;
        }
        private void FixedUpdate()
        {
            transform.LookAt(m_PlayerTransform.position, (m_PlayerTransform.position - transform.position));
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + m_Offset, 0f);
        }
    }
}

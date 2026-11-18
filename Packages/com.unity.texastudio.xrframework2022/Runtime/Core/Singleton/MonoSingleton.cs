using UnityEngine;
namespace TexaXRFramework.Core
{
    // 泛型单例基类，继承自 MonoBehaviour
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }           
        }
    }
}
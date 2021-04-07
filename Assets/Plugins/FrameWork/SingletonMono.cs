using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject {name = typeof(T).ToString()};
                
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<T>();
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
        }
    }

}
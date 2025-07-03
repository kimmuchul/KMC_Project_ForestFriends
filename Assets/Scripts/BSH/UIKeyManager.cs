using System.Collections.Generic;
using UnityEngine;
    public class UIKeyManager : MonoBehaviour
    {
        private static Stack<UIWindow> uistack = new Stack<UIWindow>();

        void Update()
        {
            InputEnterKey();
        }
        public static void RegisterUI(UIWindow window)
        {
            uistack.Push(window);
        }
        public static void UnregisterUI(UIWindow window)
        {
            if(uistack.Count > 0 && uistack.Peek() == window)
            {
                uistack.Pop();
            }
        }
        void InputEnterKey()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (uistack.Count > 0)
                {
                    uistack.Peek().OnEnterPressed();
                }
            }
        }
        void InputEscKey()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uistack.Count > 0)
                {
                    uistack.Peek().OnEscPressed();
                }
            }
        }
    }

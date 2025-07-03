using UnityEngine;
using UnityEngine.UI;
    public class UIWindow : MonoBehaviour
    {
        public Button enterButton;
        public Button escButton;

        private void OnEnable()
        {
            UIKeyManager.RegisterUI(this);
        }
        private void OnDisable()
        {
            UIKeyManager.UnregisterUI(this);
        }
        public void OnEnterPressed()
        {
            if(enterButton != null && enterButton.gameObject.activeInHierarchy)
            {
                enterButton.onClick.Invoke();
            }
        }
        public void OnEscPressed()
        {
            if(escButton != null && escButton.gameObject.activeInHierarchy)
            {
                escButton.onClick.Invoke();
            }
        }
    }

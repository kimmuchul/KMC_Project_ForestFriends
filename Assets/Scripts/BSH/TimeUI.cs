using System;
using TMPro;
using UnityEngine;
    public class TimeUI : MonoBehaviour
    {
        public TMP_Text timeText;
        void Start()
        {
            timeText.text = "";
        }
        public void PhaseCheckTime()
    {
        timeText.text = "패널을 클릭하거나 esc버튼을 누르세요";
    }
    public void TimeTexting()
    {
        timeText.text = DateTime.Now.ToString(("HH : mm : ss"));
    }
}
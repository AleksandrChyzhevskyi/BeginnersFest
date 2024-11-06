using UnityEngine;

namespace _Development.Scripts.BeginnersFest.View
{
    public class PanelDayView : MonoBehaviour
    {
        public GameObject ContentQuest;

        public int NumberDay { get; private set; }

        public void SetNumberDay(int value) => 
            NumberDay = value;
    }
}
using _Development.Scripts.BeginnersFest.Enums;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.View
{
    public class ColorButtonForQuestStatus : MonoBehaviour
    {
        public Color InProgress;
        public Color Closed;
        public Color Take;
        public Color Taken;

        public Color GetColor(StatusQuest status)
        {
            return status switch
            {
                StatusQuest.Closed => Closed,
                StatusQuest.Take => Take,
                StatusQuest.Taken => Taken,
                StatusQuest.InProgress => InProgress,
                _ => Color.black
            };
        }
    }
}
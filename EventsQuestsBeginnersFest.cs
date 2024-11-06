using UnityEngine;

namespace _Development.Scripts.BeginnersFest
{
    public class EventsQuestsBeginnersFest : MonoBehaviour
    {
        public void OpenChest(bool isADChest) => 
            GeneralEvents.Instance.OnOpenChest(isADChest);
    }
}
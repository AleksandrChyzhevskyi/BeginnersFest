using System.Collections.Generic;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Data
{
    [CreateAssetMenu(fileName = "BeginnersFestData", menuName = "BeginnersFest/Data")]
    public class BeginnersFestData : ScriptableObject
    {
        public RPGCurrency Currency;
        public List<DailyQuests> DailyQuestsList = new();
        public List<Reward> Rewards = new();
    }
}
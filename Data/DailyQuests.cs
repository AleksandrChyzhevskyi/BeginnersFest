using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Data
{
    [Serializable]
    public class DailyQuests
    {
        private readonly BeginnersFestData _beginnersFestData;

        public int NumberDay;

        public List<Quest> Quests = new List<Quest>();

    }
}
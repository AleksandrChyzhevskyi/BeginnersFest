using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;

namespace _Development.Scripts.BeginnersFest.SaveLoadData
{
    [Serializable]
    public class ProgressPlayerBeginnersFest
    {
        public SavaDataTime StartData;
        public int ProgressOnBar;
        public List<Chest> Chests = new();
        public List<Day> Days = new();
    }

    [Serializable]
    public class Chest
    {
        public StatusQuest Status;
        public Reward RewardInChest;
    }

    [Serializable]
    public class Day
    {
        public int NumberDay;
        public List<QuestSave> QuestSaves = new();
    }

    [Serializable]
    public class QuestSave
    {
        public StatusQuest Status;
        public int CompletedQuantity;
        public Quest Quest;
        public List<RPGNpc> Npcs = new();
    }
}
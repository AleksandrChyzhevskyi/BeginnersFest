using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;

namespace _Development.Scripts.BeginnersFest.Interface
{
    public interface IQuestModel
    {
        event Action<string, string> ChangedCount;
        event Action<StatusQuest> QuestCompleted;
        List<RPGNpc> GetRpgNpcs();
        StatusQuest Status { get; }
        Quest QuestData { get; }
        int Count { get; }
        void ChangeStatusQuest(StatusQuest statusQuest);
        void SubscribeQuest();
    }
}
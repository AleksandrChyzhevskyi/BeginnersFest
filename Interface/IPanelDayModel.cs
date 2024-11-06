using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Infrastructure.Services;
using _Development.Scripts.BeginnersFest.SaveLoadData;
using _Development.Scripts.BeginnersFest.View;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Interface
{
    public interface IPanelDayModel
    {
        event Action<int, int> ReceivedReward;
        event Action QuestCompleted;
        List<QuestView> QuestsViewDay { get; }
        List<IQuestModel> QuestsModelDay { get; }
        int NumberDay { get; set; }

        void CreateContent(IEnumerable<Quest> _quests, QuestView _questViewPrefab, Transform _parentTransform,
            ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest, int numberDay);
        void ActiveQuest(QuestView questView);
        void LoadContent(Day days, QuestView questViewPrefab, Transform parentTransform,
            ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest, int numberDay);
        QuestView GetQuestView(IQuestModel value);
        IQuestModel GetQuestModel(QuestView value);
    }
}
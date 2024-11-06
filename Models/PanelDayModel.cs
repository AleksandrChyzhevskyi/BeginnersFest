using System;
using System.Collections.Generic;
using System.Linq;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.Infrastructure.Services;
using _Development.Scripts.BeginnersFest.Interface;
using _Development.Scripts.BeginnersFest.SaveLoadData;
using _Development.Scripts.BeginnersFest.View;
using BLINK.RPGBuilder.Characters;
using JetBrains.Annotations;
using UnityEngine;
using ISaveLoadServiceBeginnersFest = _Development.Scripts.BeginnersFest.Interface.ISaveLoadServiceBeginnersFest;
using Object = UnityEngine.Object;

namespace _Development.Scripts.BeginnersFest.Models
{
    public class PanelDayModel : IPanelDayModel
    {
        public event Action<int, int> ReceivedReward;
        public event Action QuestCompleted;

        private Dictionary<QuestView, IQuestModel> _models;

        public List<QuestView> QuestsViewDay => _models.Keys.ToList();
        public List<IQuestModel> QuestsModelDay => _models.Values.ToList();

        public int NumberDay { get; set; }

        public void CreateContent(IEnumerable<Quest> _quests, QuestView questViewPrefab, Transform parentTransform,
            ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest, int numberDay)
        {
            _models = new Dictionary<QuestView, IQuestModel>();
            NumberDay = numberDay;

            foreach (Quest quest in _quests)
            {
                QuestView questView = Object.Instantiate(questViewPrefab, parentTransform);
                IQuestModel model = new QuestModel(quest, this, saveLoadServiceBeginnersFest);

                questView.SetInfoForQuest(quest.Quantity.ToString(), GetDescription(model),
                    SetMaxBarNumber(quest));

                _models.Add(questView, model);
            }
        }

        public void LoadContent(Day days, QuestView questViewPrefab, Transform parentTransform,
            ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest, int numberDay)
        {
            _models = new Dictionary<QuestView, IQuestModel>();
            NumberDay = numberDay;

            foreach (QuestSave questSave in days.QuestSaves)
            {
                QuestView questView = Object.Instantiate(questViewPrefab, parentTransform);

                IQuestModel model = new QuestModel(questSave.Quest, this, saveLoadServiceBeginnersFest,
                    questSave.CompletedQuantity, questSave.Npcs);

                questView.SetInfoForQuest(questSave.Quest.Quantity.ToString(), GetDescription(model),
                    SetMaxBarNumber(questSave.Quest), questSave.CompletedQuantity.ToString());

                SetStateQuest(questSave, questView, model);

                _models.Add(questView, model);
            }
        }

        private string SetMaxBarNumber(Quest quest)
        {
            return quest.TypeQuest switch
            {
                TypeTest.ChangeLevel => quest.IsDefault ? "1" : quest.Count.ToString(),
                TypeTest.KillBoss => quest.Npcs.Count.ToString(),
                _ => quest.Count.ToString()
            };
        }

        private void SetStateQuest(QuestSave questSave, QuestView questView, IQuestModel model)
        {
            switch (questSave.Status)
            {
                case StatusQuest.InProgress:
                    model.SubscribeQuest();
                    model.ChangeStatusQuest(model.Status);
                    model.ChangedCount += questView.SetProgressBar;
                    model.QuestCompleted += questView.OnCompleted;
                    model.QuestCompleted += OnQuestCompleted;
                    questView.ClickedGetReward += OnClickedGetReward;
                    questView.SetStatusButtonGoToQuest(model.Status);
                    break;

                case StatusQuest.Take:
                    model.ChangeStatusQuest(questSave.Status);
                    questView.SetProgressBar(questSave.CompletedQuantity.ToString(), SetMaxBarNumber(questSave.Quest));
                    questView.OnCompleted(questSave.Status);
                    questView.ClickedGetReward += OnClickedGetReward;
                    break;

                case StatusQuest.Taken:
                    model.ChangeStatusQuest(questSave.Status);
                    questView.OnCompleted(questSave.Status);
                    questView.SetStatusButtonGetReward(model.Status);
                    break;

                case StatusQuest.Closed:
                    model.ChangeStatusQuest(questSave.Status);
                    questView.SetStatusButtonGoToQuest(model.Status);
                    break;
            }
        }

        public void ActiveQuest(QuestView questView)
        {
            IQuestModel model = _models[questView];
            model.SubscribeQuest();
            model.ChangeStatusQuest(StatusQuest.InProgress);

            model.ChangedCount += questView.SetProgressBar;
            model.QuestCompleted += questView.OnCompleted;
            model.QuestCompleted += OnQuestCompleted;

            questView.ClickedGetReward += OnClickedGetReward;
            questView.SetStatusButtonGoToQuest(model.Status);
        }

        public QuestView GetQuestView(IQuestModel value)
        {
            foreach (KeyValuePair<QuestView, IQuestModel> valuePair in _models)
            {
                if (valuePair.Value == value)
                    return valuePair.Key;
            }

            return null;
        }

        public IQuestModel GetQuestModel(QuestView value) =>
            _models.ContainsKey(value) ? _models[value] : null;

        private void OnQuestCompleted(StatusQuest _)
        {
            QuestCompleted?.Invoke();
        }

        private void OnClickedGetReward(QuestView questView)
        {
            IQuestModel model = _models[questView];
            model.ChangeStatusQuest(StatusQuest.Taken);

            ReceivedReward?.Invoke(model.QuestData.Quantity, NumberDay);

            model.ChangedCount -= questView.SetProgressBar;
            model.QuestCompleted -= questView.OnCompleted;
            model.QuestCompleted -= OnQuestCompleted;

            questView.ClickedGetReward -= OnClickedGetReward;

            questView.SetStatusButtonGetReward(model.Status);
        }

        private string GetDescription(IQuestModel quest)
        {
            return quest.QuestData.TypeQuest switch
            {
                TypeTest.Die => $"Die {quest.QuestData.Count} times",
                TypeTest.Earn => $"Earn {quest.QuestData.Count} {quest.QuestData.Currency.entryDisplayName}",
                TypeTest.Spend => $"Spend {quest.QuestData.Count} {quest.QuestData.Currency.entryDisplayName}",
                TypeTest.Unlock => $"Unlock {quest.QuestData.Unlock}",
                TypeTest.Upgrade => $"Upgrade {quest.QuestData.Upgrade.name} - {quest.QuestData.Count} times",
                TypeTest.CompleteTasks => $"Complete Tasks {quest.QuestData.Count} times",
                TypeTest.KillEnemies when quest.QuestData.IsHaveType && quest.QuestData.IsHaveForm =>
                    $"Kill {quest.QuestData.Npc.entryDisplayName} - {quest.QuestData.Count} times, in the {quest.QuestData.Unlock} shape",
                TypeTest.KillEnemies when quest.QuestData.IsHaveType =>
                    $"Kill {quest.QuestData.Npc.entryDisplayName} - {quest.QuestData.Count} times",
                TypeTest.KillEnemies when quest.QuestData.IsHaveForm =>
                    $"Kill {quest.QuestData.Count} mobs, in the {quest.QuestData.Unlock} shape",
                TypeTest.KillEnemies => $"Kill {quest.QuestData.Count} enemies",
                TypeTest.KillBoss when quest.QuestData.IsHaveForm && quest.QuestData.Npcs.Count > 1 =>
                    $"Kill {quest.QuestData.Npcs.Count} different bosses, in the {quest.QuestData.Unlock} shape",
                TypeTest.KillBoss when quest.QuestData.IsHaveForm && quest.QuestData.Npcs.Count == 1 =>
                    $"Kill {quest.QuestData.Npcs.FirstOrDefault().entryDisplayName} different boss, in the {quest.QuestData.Unlock} shape",
                TypeTest.KillBoss => quest.QuestData.Npcs.Count > 1
                    ? $"Kill {quest.QuestData.Npcs.Count} different bosses"
                    : $"Kill {quest.QuestData.Npcs.FirstOrDefault().entryDisplayName} different boss",
                TypeTest.SelectForms => quest.QuestData.Unlock == AnimalsToEffectID.Default
                    ? $"Select any forms - {quest.QuestData.Count} times"
                    : $"Select forms {quest.QuestData.Unlock} - {quest.QuestData.Count} times",
                TypeTest.SpinRoulette => $"Spin Roulette {quest.QuestData.Count} times",
                TypeTest.UseBuster => $"Use {quest.QuestData.Buster.entryDisplayName} - {quest.QuestData.Count} times",
                TypeTest.ParticipationInInfinityMode => $"Enter in infinity mode {quest.QuestData.Count} times",
                TypeTest.OpenInventory => quest.QuestData.Count > 1
                    ? "Open inventory"
                    : $"Open inventory {quest.QuestData.Count} times",
                TypeTest.PutOnAnItem => quest.QuestData.IsDefault
                    ? "Put on any item"
                    : $"Put on an {quest.QuestData.Item.entryDisplayName}",
                TypeTest.ChangeLevel => quest.QuestData.IsDefault
                    ? "Get the next level"
                    : $"Get the {(Character.Instance.CharacterData.Level - quest.Count) + quest.QuestData.Count} level",
                TypeTest.OpenChest => quest.QuestData.IsADChest
                    ? $"Open the chest on respawn {quest.QuestData.Count} times"
                    : $"Open chest outside respawn {quest.QuestData.Count} times",
                TypeTest.DealDamage => quest.QuestData.IsDefault
                    ? $"Damage enemies {quest.QuestData.Count} times"
                    : $"Deal {quest.QuestData.Count} damage",
                TypeTest.DealCriticalDamage => quest.QuestData.IsDefault
                    ? $"Damage enemies {quest.QuestData.Count} times with critical damage"
                    : $"Deal {quest.QuestData.Count} critical damage",
                TypeTest.KillHeadshot => $"Kill {quest.QuestData.Count} enemies with a headshot",
                TypeTest.ResistDamage => quest.QuestData.IsDefault
                    ? $"Block damage {quest.QuestData.Count} times with resistance"
                    : $"Block {quest.QuestData.Count} damage with resistance",
                TypeTest.Dodge => quest.QuestData.IsDefault
                    ? $"Dodge {quest.QuestData.Count} times"
                    : $"Avoid {quest.QuestData.Count} damage with Dodge",
                TypeTest.Default => "",
            };
        }
    }
}
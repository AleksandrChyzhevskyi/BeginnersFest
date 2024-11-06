using System;
using System.Linq;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.Interface;
using _Development.Scripts.BeginnersFest.Models;
using _Development.Scripts.BeginnersFest.SaveLoadData;
using _Development.Scripts.Boot;
using _Development.Scripts.Extensions;
using _Development.Scripts.SaveLoadDatesPlayer;
using _Development.Scripts.SaveLoadDatesPlayer.InterfaceSaveDatesPlayer;
using BLINK.RPGBuilder.Characters;

namespace _Development.Scripts.BeginnersFest.Infrastructure.Services
{
    public class SaveLoadServiceBeginnersFest : Interface.ISaveLoadServiceBeginnersFest
    {
        private readonly ILoadSaveBeginnersFest _loadSaveBeginners;

        public SaveLoadServiceBeginnersFest() =>
            _loadSaveBeginners = new LoadSave();

        public void Save(ProgressPlayerBeginnersFest saveData = null) =>
            _loadSaveBeginners.SaveBeginnersFest(saveData ?? DefaultSaveBeginnersFest());

        public void SaveQuest(int NumberDay, IQuestModel questModel)
        { 
            Day findDay = Character.Instance.CharacterData.ProgressBeginnersFest.Days
                .FirstOrDefault(day => day.NumberDay == NumberDay);
            
            if(findDay == null)
                return;

            foreach (QuestSave quest in findDay.QuestSaves)
            {
                if (quest.Quest != questModel.QuestData)
                    continue;
                
                quest.CompletedQuantity = questModel.Count;
                quest.Status = questModel.Status;
                quest.Npcs = questModel.GetRpgNpcs();
            }
            
            Save(Character.Instance.CharacterData.ProgressBeginnersFest);
        }

        public void SaveProgressOnBar(int ProgressOnBar)
        {
            Character.Instance.CharacterData.ProgressBeginnersFest.ProgressOnBar = ProgressOnBar;
            Save(Character.Instance.CharacterData.ProgressBeginnersFest);
        }

        public void SaveChest(int CountCurrency, StatusQuest status)
        {
            Chest findChest = Character.Instance.CharacterData.ProgressBeginnersFest.Chests.FirstOrDefault(chest =>
                chest.RewardInChest.CountCurrency == CountCurrency);

            if(findChest == null)
                return;
            
            findChest.Status = status;
            Save(Character.Instance.CharacterData.ProgressBeginnersFest);
        }

        public ProgressPlayerBeginnersFest Load() => 
            _loadSaveBeginners.LoadBeginnersFest();

        private ProgressPlayerBeginnersFest DefaultSaveBeginnersFest()
        {
            ProgressPlayerBeginnersFest save = new ProgressPlayerBeginnersFest
            {
               StartData = DateTime.Today.ToSavaDataTime(),
               ProgressOnBar = 0,
            };

            foreach (DailyQuests dailyQuests in Game.instance.GetBeginnersFestData().DailyQuestsList)
            {
                Day day = new Day
                {
                    NumberDay = dailyQuests.NumberDay
                };

                foreach (Quest quest in dailyQuests.Quests)
                {
                    QuestSave questSave = new QuestSave
                    {
                        Quest = quest,
                        CompletedQuantity = 0,
                        Status = StatusQuest.Closed,
                        Npcs = quest.Npcs
                    };

                    day.QuestSaves.Add(questSave);
                }

                save.Days.Add(day);
            }

            int saveMaxLineBar = 0;

            foreach (Reward reward in Game.instance.GetBeginnersFestData().Rewards)
            {
                Chest chest = new Chest
                {
                    RewardInChest = reward,
                    Status = StatusQuest.InProgress
                };
                save.Chests.Add(chest);

                if (saveMaxLineBar < reward.CountCurrency)
                    saveMaxLineBar = reward.CountCurrency;
            }

            return save;
        }
    }
}
using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.Infrastructure.Services;
using _Development.Scripts.BeginnersFest.Interface;
using _Development.Scripts.BeginnersFest.SaveLoadData;
using _Development.Scripts.BeginnersFest.View.Bar;
using _Development.Scripts.BeginnersFest.View.Reward;
using _Development.Scripts.Boot;
using _Development.Scripts.Data;
using _Development.Scripts.Extensions;
using _Development.Scripts.LootLevel;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using DTT.Utils.Extensions;
using UnityEngine;
using ISaveLoadServiceBeginnersFest = _Development.Scripts.BeginnersFest.Interface.ISaveLoadServiceBeginnersFest;
using Object = UnityEngine.Object;

namespace _Development.Scripts.BeginnersFest.Models
{
    public class ProgressBarModel : IProgressBarModel
    {
        public event Action<int> ChangedRewardCount;

        private readonly Dictionary<RewardView, Reward> _chests;
        private readonly List<IPanelDayModel> _panelDayModel;
        private readonly ISaveLoadServiceBeginnersFest _saveLoadServiceBeginnersFest;
        private readonly IRewardCalculator _rewardCalculator;
        private readonly RewardView _prefabRewardView;
        private readonly RewardsProgressBarView _rewardsProgressBarView;
        private readonly float _size;

        private int _countReward;
        private float _maxCount;

        public ProgressBarModel(List<IPanelDayModel> panelDayModel, RewardsProgressBarView rewardsProgressBarView,
            RewardView prefabRewardView, ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest)
        {
            _panelDayModel = panelDayModel;
            _rewardsProgressBarView = rewardsProgressBarView;
            _prefabRewardView = prefabRewardView;
            _saveLoadServiceBeginnersFest = saveLoadServiceBeginnersFest;
            _rewardCalculator = new RewardCalculator();
            _chests = new Dictionary<RewardView, Reward>();
        }

        public void Subscribe()
        {
            foreach (IPanelDayModel panelDayModel in _panelDayModel)
                panelDayModel.ReceivedReward += CheckCountReward;
        }

        public void Unsubscribe()
        {
            foreach (IPanelDayModel panelDayModel in _panelDayModel)
                panelDayModel.ReceivedReward -= CheckCountReward;
        }

        public void CreateChase(bool isLoad = false)
        {
            if (isLoad == false)
            {
                foreach (Reward reward in Game.instance.GetBeginnersFestData().Rewards)
                    if (_maxCount < reward.CountCurrency)
                        _maxCount = reward.CountCurrency;
            }
            else
            {
                foreach (Chest chest in Character.Instance.CharacterData.ProgressBeginnersFest.Chests)
                    if (_maxCount < chest.RewardInChest.CountCurrency)
                        _maxCount = chest.RewardInChest.CountCurrency;
            }

            float multiplier = _rewardsProgressBarView.MaxBar / _maxCount;
            _rewardsProgressBarView.SetMultiplier(multiplier);

            if (isLoad == false)
            {
                foreach (Reward variable in Game.instance.GetBeginnersFestData().Rewards)
                {
                    RewardView rewardView = CreateChestView(variable, multiplier);
                    rewardView.SetInfoReward(variable.CountCurrency.ToString(),
                        GetEffectInReward(variable));
                    rewardView.ChangedStatus += OnChangedStatus;
                    rewardView.AwardReceived += OnAwardReceived;
                    rewardView.SetStateInactive();
                }
            }
            else
            {
                foreach (var chest in Character.Instance.CharacterData.ProgressBeginnersFest.Chests)
                {
                    RewardView rewardView = CreateChestView(chest.RewardInChest, multiplier);
                    rewardView.SetInfoReward(chest.RewardInChest.CountCurrency.ToString(),
                        GetEffectInReward(chest.RewardInChest));
                    SetStateChest(chest, rewardView);
                }

                _countReward = Character.Instance.CharacterData.ProgressBeginnersFest.ProgressOnBar;
            }
        }

        private static AnimalsToEffectID GetEffectInReward(Reward variable) =>
            variable.IsNewSkin ? (AnimalsToEffectID)variable.Skin.ID : AnimalsToEffectID.Default;

        private void CheckCountReward(int count, int _)
        {
            _countReward += count;
            _saveLoadServiceBeginnersFest.SaveProgressOnBar(_countReward);

            ChangedRewardCount?.Invoke(count);

            foreach (KeyValuePair<RewardView, Reward> pair in _chests)
            {
                if (pair.Value.CountCurrency > _countReward)
                    continue;

                if (pair.Key.Status != StatusQuest.Taken)
                    pair.Key.SetStateClose();
            }
        }

        private void OnAwardReceived(RewardView rewardView)
        {
            rewardView.AwardReceived -= OnAwardReceived;
            GetReward(_chests[rewardView]);
            _chests.Remove(rewardView);
        }

        private void GetReward(Reward reward)
        {
            if (reward.IsNewSkin)
            {
                if (GameState.playerEntity.IsShapeshifted())
                {
                    RPGBuilderEssentials.Instance.BeginnersLogic.PrefabPanelBeginnersFestView.gameObject
                        .SetActive(false);
                    RPGBuilderEssentials.Instance.RunnerElements.StartWaitAction(reward.WaitInSecond, ApplyEffect,
                        reward, 0.1f);
                }

                RPGEffect effect = GameDatabase.Instance.GetEffects()[reward.Skin.ID];
                GeneralEvents.Instance.OnOpenedNewSkin(effect, SkinID.DefaultSkin);
            }
            else
            {
                foreach (RPGLootTable.LOOT_ITEMS lootItems in reward.RewardInChest.lootItems)
                    _rewardCalculator.PutInInventory(lootItems);
            }
        }

        private static void ApplyEffect(Reward reward)
        {
            GameActionsManager.Instance.ApplyEffect(RPGCombatDATA.TARGET_TYPE.Caster,
                GameState.playerEntity, reward.Skin.ID);
        }

        private RewardView CreateChestView(Reward variable, float multiplier)
        {
            float positionChest = variable.CountCurrency * multiplier;

            RewardView rewardView =
                Object.Instantiate(_prefabRewardView, _rewardsProgressBarView.GetContainerForChest(positionChest));

            if (positionChest > _rewardsProgressBarView.MaxValueOneBar &&
                positionChest < _rewardsProgressBarView.MaxValueOneBar * 2)
                rewardView.GetRectTransform().anchoredPosition =
                    new Vector2(_rewardsProgressBarView.MaxValueOneBar - OffsetPosition(positionChest), 0);
            else
                rewardView.GetRectTransform().anchoredPosition =
                    new Vector2(OffsetPosition(positionChest), 0);

            _chests.Add(rewardView, variable);
            return rewardView;
        }

        private void OnChangedStatus(RewardView rewardView)
        {
            if (rewardView.Status == StatusQuest.Taken)
                rewardView.ChangedStatus -= OnChangedStatus;

            _saveLoadServiceBeginnersFest.SaveChest(_chests[rewardView].CountCurrency, rewardView.Status);
        }
        
        public float OffsetPosition(float positionChest)
        {
            if (positionChest > _rewardsProgressBarView.MaxValueOneBar * 2)
                positionChest -= _rewardsProgressBarView.MaxValueOneBar * 2;
            else if (positionChest > _rewardsProgressBarView.MaxValueOneBar)
                positionChest -= _rewardsProgressBarView.MaxValueOneBar;
            return positionChest;
        }

        private void SetStateChest(Chest chest, RewardView rewardView)
        {
            switch (chest.Status)
            {
                case StatusQuest.InProgress:
                    rewardView.SetStateInactive();
                    rewardView.ChangedStatus += OnChangedStatus;
                    rewardView.AwardReceived += OnAwardReceived;
                    break;
                case StatusQuest.Take:
                    rewardView.SetStateClose();
                    rewardView.ChangedStatus += OnChangedStatus;
                    rewardView.AwardReceived += OnAwardReceived;
                    break;
                case StatusQuest.Taken:
                    rewardView.SetStateOpen();
                    _chests.Remove(rewardView);
                    break;
                case StatusQuest.Closed:
                    return;
            }
        }
    }
}
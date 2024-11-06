using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Data;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.Infrastructure.Services;
using _Development.Scripts.BeginnersFest.Interface;
using _Development.Scripts.Data;
using _Development.Scripts.Upgrade.Initialization;
using _Development.Scripts.Upgrade.Model;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using ISaveLoadServiceBeginnersFest = _Development.Scripts.BeginnersFest.Interface.ISaveLoadServiceBeginnersFest;

namespace _Development.Scripts.BeginnersFest.Models
{
    public class QuestModel : IQuestModel
    {
        public event Action<string, string> ChangedCount;
        public event Action<StatusQuest> QuestCompleted;

        private readonly ISaveLoadServiceBeginnersFest _saveLoadServiceBeginnersFest;
        private readonly IPanelDayModel _panelDayModel;
        private readonly RPGCurrency _currency;

        private int _currencyStartCount;
        private string _description;
        private int _maxCount;
        private int _startLevel;
        private readonly int _borderCount;
        private IQuestModel _questModelImplementation;
        private List<RPGNpc> _rpgNpcs;

        public StatusQuest Status { get; private set; }

        public int Count { get; private set; }

        public Quest QuestData { get; }

        public QuestModel(Quest questData, IPanelDayModel panelDayModel,
            ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest,
            int count = 0, List<RPGNpc> npcs = null)
        {
            QuestData = questData;
            Count = count;
            
            _borderCount = questData.TypeQuest == TypeTest.KillBoss ? questData.Npcs.Count : QuestData.Count;
            _panelDayModel = panelDayModel;
            _saveLoadServiceBeginnersFest = saveLoadServiceBeginnersFest;
            _rpgNpcs = new List<RPGNpc>(npcs ?? QuestData.Npcs);
            
            if (QuestData.TypeQuest is not (TypeTest.Earn or TypeTest.Spend))
                return;

            _currency = GameDatabase.Instance.GetCurrencies()[QuestData.Currency.ID];
            _currencyStartCount = Character.Instance.getCurrencyAmount(_currency);
        }

        public void ChangeStatusQuest(StatusQuest statusQuest)
        {
            Status = statusQuest;
            _saveLoadServiceBeginnersFest.SaveQuest(_panelDayModel.NumberDay, this);
        }

        public List<RPGNpc> GetRpgNpcs() => 
            new(_rpgNpcs);

        public void SubscribeQuest()
        {
            switch (QuestData.TypeQuest)
            {
                case TypeTest.KillEnemies:
                    CombatEvents.PlayerKilledNPC += OnPlayerKilledNPC;
                    break;

                case TypeTest.KillBoss:
                    CombatEvents.PlayerKilledNPC += OnPlayerKilledBoos;
                    break;

                case TypeTest.Earn:
                    GeneralEvents.PlayerCurrencyChanged += OnPlayerCurrencyEarn;
                    GeneralEvents.AmountOfPlayersCurrencyChanged += OnPlayerCurrencyEarn;
                    break;

                case TypeTest.Spend:
                    GeneralEvents.PlayerCurrencyChanged += OnPlayerCurrencySpend;
                    GeneralEvents.AmountOfPlayersCurrencyChanged += OnPlayerCurrencySpend;
                    break;

                case TypeTest.Die:
                    CombatEvents.PlayerDied += OnPlayerDied;
                    break;

                case TypeTest.Unlock:
                    GeneralEvents.OpenedNewSkin += OnOpenedNewSkin;
                    break;

                case TypeTest.Upgrade:
                    UpgradeInitialization.OnUpgradeStat += OnUpgradeStat;
                    break;

                case TypeTest.ParticipationInInfinityMode:
                    GeneralEvents.PlayerEnteredInfinityZone += OnPlayerEnteredInfinityZone;
                    break;

                case TypeTest.SelectForms:
                    CombatEvents.ModelPrefabCreated += OnModelPrefabCreated;
                    break;

                case TypeTest.SpinRoulette:
                    GeneralEvents.SpinnedRoulette += OnSpinedRoulette;
                    break;

                case TypeTest.UseBuster:
                    CombatEvents.AppliedEffect += OnAppliedEffect;
                    break;

                case TypeTest.CompleteTasks:
                    _panelDayModel.QuestCompleted += OnQuestCompleted;
                    break;

                case TypeTest.OpenInventory:
                    UIEvents.OpenPanel += OnOpenInventory;
                    break;

                case TypeTest.PutOnAnItem:
                    GeneralEvents.PlayerUsedItem += OnPlayerUsedItem;
                    break;

                case TypeTest.ChangeLevel:
                    GameEvents.CharacterLevelChanged += OnCharacterLevelChanged;
                    _startLevel = Character.Instance.CharacterData.Level;
                    _maxCount = _startLevel + QuestData.Count;
                    break;

                case TypeTest.OpenChest:
                    GeneralEvents.OpenChest += OnOpenChest;
                    break;

                case TypeTest.DealDamage:
                    CombatEvents.DamageDealt += OnDealDamage;
                    break;
                
                case TypeTest.DealCriticalDamage:
                    CombatEvents.DamageDealt += OnDealCriticalDamage;
                    break;

                case TypeTest.KillHeadshot:
                    CombatEvents.DamageDealt += OnDealHeadshot;
                    break;

                case TypeTest.ResistDamage:
                    CombatEvents.PlayerBlockedDamage += OnPlayerResist;
                    break;

                case TypeTest.Dodge:
                    CombatEvents.PlayerBlockedDamage += OnPlayerDodge;
                    break;

                default:
                    new Exception($"{QuestData.TypeQuest} - there are no data on this quest");
                    break;
            }
        }

        private void OnDealCriticalDamage(CombatCalculations.DamageResult result)
        {
            if(result.IsCritical == false)
                return;
            
            UpdateDamage(result, OnDealCriticalDamage);
        }

        private void OnDealDamage(CombatCalculations.DamageResult result) => 
            UpdateDamage(result, OnDealDamage);

        private void OnPlayerDodge(int damage, TypeTest typeTest)
        {
            if (typeTest != TypeTest.Dodge)
                return;

            if (CheckStatusQuest())
                return;

            if (QuestData.IsDefault)
            {
                if (CheckCount())
                    return;
            }
            else
            {
                if (CheckCount(damage))
                    return;
            }

            CombatEvents.PlayerBlockedDamage -= OnPlayerDodge;
            SetCompleteQuest();
        }

        private void OnPlayerResist(int damage, TypeTest typeTest)
        {
            if (typeTest != TypeTest.ResistDamage)
                return;

            if (CheckStatusQuest())
                return;

            if (QuestData.IsDefault)
            {
                if (CheckCount())
                    return;
            }
            else
            {
                if (CheckCount(damage))
                    return;
            }

            CombatEvents.PlayerBlockedDamage -= OnPlayerResist;
            SetCompleteQuest();
        }

        private void OnDealHeadshot(CombatCalculations.DamageResult result)
        {
            if (result.caster.IsPlayer() == false)
                return;

            if (CheckStatusQuest())
                return;

            if (result.DamageActionType != "HeadShot")
                return;

            if (CheckCount())
                return;

            CombatEvents.DamageDealt -= OnDealHeadshot;
            SetCompleteQuest();
        }

        private void OnOpenChest(bool adChest)
        {
            if (CheckStatusQuest())
                return;

            if (QuestData.IsADChest != adChest)
                return;

            if (CheckCount())
                return;

            GeneralEvents.OpenChest -= OnOpenChest;
            SetCompleteQuest();
        }

        private void OnCharacterLevelChanged(int level)
        {
            if (CheckStatusQuest())
                return;

            if (QuestData.IsDefault)
            {
                if (_startLevel + 1 != level)
                    return;

                Count = _borderCount;
                ChangedCount?.Invoke(_borderCount.ToString(), _borderCount.ToString());
                _saveLoadServiceBeginnersFest.SaveQuest(_panelDayModel.NumberDay, this);
            }
            else
            {
                Count = level - _startLevel;
                ChangedCount?.Invoke((level - _startLevel).ToString(), _borderCount.ToString());
                _saveLoadServiceBeginnersFest.SaveQuest(_panelDayModel.NumberDay, this);

                if (level < _maxCount)
                    return;
            }

            GameEvents.CharacterLevelChanged -= OnCharacterLevelChanged;
            SetCompleteQuest();
        }

        private void OnPlayerUsedItem(RPGItem item)
        {
            if (CheckStatusQuest())
                return;

            if (QuestData.Item != item && QuestData.IsDefault != true)
                return;

            if (CheckCount())
                return;

            GeneralEvents.PlayerUsedItem -= OnPlayerUsedItem;
            SetCompleteQuest();
        }

        private void OnOpenInventory(string panel)
        {
            if ("Inventory" != panel)
                return;

            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            UIEvents.OpenPanel -= OnOpenInventory;
            SetCompleteQuest();
        }

        private void OnQuestCompleted()
        {
            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            _panelDayModel.QuestCompleted -= OnQuestCompleted;
            SetCompleteQuest();
        }

        private void OnAppliedEffect(int effectID)
        {
            if (CheckStatusQuest())
                return;

            if (effectID != QuestData.Buster.ID)
                return;

            if (CheckCount())
                return;

            CombatEvents.AppliedEffect -= OnAppliedEffect;
            SetCompleteQuest();
        }

        private void OnSpinedRoulette()
        {
            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            GeneralEvents.SpinnedRoulette -= OnSpinedRoulette;
            SetCompleteQuest();
        }

        private void OnModelPrefabCreated(GameObject _)
        {
            if (CheckStatusQuest())
                return;

            if ((int)QuestData.Unlock != GameState.playerEntity.ShapeshiftedEffect.ID &&
                QuestData.Unlock != AnimalsToEffectID.Default)
                return;

            if (CheckCount())
                return;

            CombatEvents.ModelPrefabCreated -= OnModelPrefabCreated;
            SetCompleteQuest();
        }

        private void OnPlayerEnteredInfinityZone()
        {
            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            GeneralEvents.PlayerEnteredInfinityZone -= OnPlayerEnteredInfinityZone;
            SetCompleteQuest();
        }

        private void OnUpgradeStat(UpgradeModel upgrade)
        {
            if (CheckStatusQuest())
                return;

            if (upgrade.ID != QuestData.Upgrade.ID)
                return;

            if (CheckCount())
                return;

            UpgradeInitialization.OnUpgradeStat -= OnUpgradeStat;
            SetCompleteQuest();
        }

        private void OnOpenedNewSkin(RPGEffect effect, SkinID skinID)
        {
            if (CheckStatusQuest())
                return;

            if (effect.ID != (int)QuestData.Unlock)
                return;

            if (CheckCount())
                return;

            GeneralEvents.OpenedNewSkin -= OnOpenedNewSkin;
            SetCompleteQuest();
        }

        private void OnPlayerDied()
        {
            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            CombatEvents.PlayerDied -= OnPlayerDied;
            SetCompleteQuest();
        }

        private void OnPlayerCurrencyEarn(RPGCurrency currency, int value) =>
            OnPlayerCurrencyEarn(currency);

        private void OnPlayerCurrencyEarn(RPGCurrency currency) =>
            CheckChangeCurrency(currency, true, OnPlayerCurrencyEarn, OnPlayerCurrencyEarn);

        private void OnPlayerCurrencySpend(RPGCurrency currency, int value) =>
            OnPlayerCurrencySpend(currency);

        private void OnPlayerCurrencySpend(RPGCurrency currency) =>
            CheckChangeCurrency(currency, false, OnPlayerCurrencySpend, OnPlayerCurrencySpend);

        private void OnPlayerKilledBoos(RPGNpc npc)
        {
            if (_rpgNpcs.Contains(npc) == false)
                return;

            if ((int)QuestData.Unlock != GameState.playerEntity.ShapeshiftedEffect.ID &&
                QuestData.Unlock != AnimalsToEffectID.Default)
                return;

            _rpgNpcs.Remove(npc);

            CheckPlayerKilledNPC(OnPlayerKilledBoos);
        }

        private void OnPlayerKilledNPC(RPGNpc npc)
        {
            if (QuestData.IsHaveType)
                if (npc.ID != QuestData.Npc.ID)
                    return;

            if ((int)QuestData.Unlock != GameState.playerEntity.ShapeshiftedEffect.ID &&
                QuestData.Unlock != AnimalsToEffectID.Default)
                return;

            CheckPlayerKilledNPC(OnPlayerKilledNPC);
        }

        private void CheckPlayerKilledNPC(Action<RPGNpc> subscription)
        {
            if (CheckStatusQuest())
                return;

            if (CheckCount())
                return;

            CombatEvents.PlayerKilledNPC -= subscription;
            SetCompleteQuest();
        }

        private bool CheckCount(int count = default)
        {
            ChangeCount(count);
            return Count < _borderCount;
        }

        private void ChangeCount(int count = default)
        {
            if (count == default)
                Count++;
            else
            {
                if (Count + count > _borderCount)
                    Count = _borderCount;
                else
                    Count += count;
            }

            _saveLoadServiceBeginnersFest.SaveQuest(_panelDayModel.NumberDay, this);
            ChangedCount?.Invoke(Count.ToString(), _borderCount.ToString());
        }

        private bool CheckStatusQuest() =>
            Status == StatusQuest.Take;

        private void SetCompleteQuest()
        {
            ChangeStatusQuest(StatusQuest.Take);
            QuestCompleted?.Invoke(Status);
        }

        private void CheckChangeCurrency(RPGCurrency currency, bool condition, Action<RPGCurrency> subscription1,
            Action<RPGCurrency, int> subscription2)
        {
            if (CheckStatusQuest())
                return;

            if (currency != _currency)
                return;

            int currentCount = Character.Instance.getCurrencyAmount(_currency) - _currencyStartCount;

            if (currentCount > 0 == condition)
            {
                if (Count + Math.Abs(currentCount) > _borderCount)
                    Count = _borderCount;
                else
                    Count += Math.Abs(currentCount);

                ChangedCount?.Invoke(Math.Abs(Count).ToString(), _borderCount.ToString());
                _saveLoadServiceBeginnersFest.SaveQuest(_panelDayModel.NumberDay, this);
            }

            _currencyStartCount = Character.Instance.getCurrencyAmount(_currency);

            if (Count < _borderCount)
                return;

            GeneralEvents.PlayerCurrencyChanged -= subscription1;
            GeneralEvents.AmountOfPlayersCurrencyChanged -= subscription2;
            SetCompleteQuest();
        }

        private void UpdateDamage(CombatCalculations.DamageResult result, Action<CombatCalculations.DamageResult> action)
        {
            if (result.caster.IsPlayer() == false)
                return;

            if (CheckStatusQuest())
                return;

            if (QuestData.IsDefault)
            {
                if (CheckCount())
                    return;
            }
            else
            {
                if (CheckCount((int)result.DamageAmount))
                    return;
            }

            CombatEvents.DamageDealt -= action;
            SetCompleteQuest();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.Interface;
using _Development.Scripts.BeginnersFest.Models;
using _Development.Scripts.BeginnersFest.View;
using _Development.Scripts.BeginnersFest.View.Reward;
using _Development.Scripts.Boot;
using _Development.Scripts.Extensions;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Infrastructure
{
    public class BeginnersFestLogic : MonoBehaviour
    {
        public PanelBeginnersFestView PrefabPanelBeginnersFestView;
        public QuestView PrefabQuestView;
        public ButtonDayView PrefabButtonDayView;
        public PanelDayView PrefabPanelDayView; 
        public RewardView prefabRewardView;
        public ImageController ImageControllerButton;

        private Dictionary<PanelDayView, IPanelDayModel> _models;
        private PanelDayView _currantActivePanel;
        private PanelDayView _currentPanel;
        private IProgressBarModel _progressBarModel;
        private List<ButtonDayView> _buttonsDay;

        private ISaveLoadServiceBeginnersFest _saveLoadServiceBeginnersFest;

        public void Initialize(ISaveLoadServiceBeginnersFest saveLoadServiceBeginnersFest) =>
            _saveLoadServiceBeginnersFest = saveLoadServiceBeginnersFest;

        private void Update()
        {
            if (GameDatabase.Instance.GetGeneralSettings().enableDevPanel == false)
                return;

            if (Input.GetMouseButtonUp(2))
                OnNextDayArrived(1);
        }

        public void Unsubscribe()
        {
            _progressBarModel.Unsubscribe();
            _progressBarModel.ChangedRewardCount -=
                PrefabPanelBeginnersFestView.PrefabRewardsProgressBarView.ChangeRewardsProgressBar;
            PrefabPanelBeginnersFestView.NextDayArrived -= OnNextDayArrived;

            foreach (ButtonDayView button in _buttonsDay)
                button.Clicked -= OnClickedButtonDayView;

            foreach (IPanelDayModel panelDayModel in _models.Values)
            {
                panelDayModel.QuestCompleted -= ImageControllerButton.ActiveImage;
                panelDayModel.QuestCompleted -= _buttonsDay
                    .FirstOrDefault(buttonDayView => buttonDayView.NumberDay == panelDayModel.NumberDay)
                    .SetActiveCompletedImage;
                panelDayModel.ReceivedReward -= CheckButton;
                panelDayModel.ReceivedReward -= CheckAllAwardsHaveBeenTaken;
            }
        }

        public void CreatePanel(bool isLoad = false)
        {
            int counterQuest = isLoad
                ? Character.Instance.CharacterData.ProgressBeginnersFest.Days.Count
                : Game.instance.GetBeginnersFestData().DailyQuestsList.Count;

            _models = new Dictionary<PanelDayView, IPanelDayModel>();
            _buttonsDay = new List<ButtonDayView>();

            for (int i = 0; i < counterQuest; i++)
            {
                ButtonDayView button =
                    Instantiate(PrefabButtonDayView, PrefabPanelBeginnersFestView.ContentButtonDays.transform);

                button.Initialize(isLoad
                    ? Character.Instance.CharacterData.ProgressBeginnersFest.Days[i].NumberDay
                    : Game.instance.GetBeginnersFestData().DailyQuestsList[i].NumberDay);

                button.Clicked += OnClickedButtonDayView;
                _buttonsDay.Add(button);

                PanelDayView panelDayView =
                    Instantiate(PrefabPanelDayView, PrefabPanelBeginnersFestView.ContentDays.transform);

                panelDayView.SetNumberDay(button.NumberDay);

                IPanelDayModel dayModel = isLoad
                    ? LoadPanelDayModel(PrefabQuestView, panelDayView, i)
                    : CreatePanelDayModel(PrefabQuestView, panelDayView, i);

                dayModel.QuestCompleted += ImageControllerButton.ActiveImage;
                dayModel.QuestCompleted += button.SetActiveCompletedImage;
                dayModel.ReceivedReward += CheckButton;
                dayModel.ReceivedReward += CheckAllAwardsHaveBeenTaken;

                _models.Add(panelDayView, dayModel);
            }

            SetCurrentDay(isLoad);
            StartWorkPanel(isLoad);
            SetCurrentButton(_currentPanel.NumberDay);

            if (isLoad)
            {
                if (CheckMonoBehaviourQuestStat())
                    ImageControllerButton.ActiveImage();

                CheckButtonDayBehaviour();
                CheckAllAwardsHaveBeenTaken();

                return;
            }

            _saveLoadServiceBeginnersFest.Save();
            SetActiveDay();
        }

        private void SetCurrentDay(bool isLoad)
        {
            if (isLoad == false)
            {
                foreach (PanelDayView panelDayView in _models.Keys) 
                    panelDayView.gameObject.SetActive(false);

                _currentPanel = _models.Keys.OrderBy(panelDayView => panelDayView.NumberDay).FirstOrDefault();
                _currantActivePanel = _currentPanel;
            }
            else
            {
                foreach (KeyValuePair<PanelDayView, IPanelDayModel> keyValuePair in _models)
                {
                    IQuestModel model =
                        keyValuePair.Value.QuestsModelDay.FirstOrDefault(questModel =>
                            questModel.Status == StatusQuest.Take);

                    if (model == null || _currentPanel != null)
                    {
                        keyValuePair.Key.gameObject.SetActive(false);
                        continue;
                    }

                    _currentPanel = keyValuePair.Key;
                    _currantActivePanel = _currentPanel;
                }

                if (_currantActivePanel == null)
                {
                    foreach (KeyValuePair<PanelDayView, IPanelDayModel> keyValuePair in _models)
                    {
                        IQuestModel model =
                            keyValuePair.Value.QuestsModelDay.FirstOrDefault(questModel =>
                                questModel.Status != StatusQuest.Closed);

                        if (model == null)
                            continue;

                        _currentPanel = keyValuePair.Key;
                        _currantActivePanel = keyValuePair.Key;
                    }
                }
            }

            _currantActivePanel.gameObject.SetActive(true);
        }

        private void CheckButton(int _, int dey)
        {
            IPanelDayModel panelDayModel = _models.Values.FirstOrDefault(dayModel => dayModel.NumberDay == dey);

            if (CheckCompletedDay(panelDayModel))
                _buttonsDay.FirstOrDefault(button => button.NumberDay == panelDayModel.NumberDay)
                    .SetInactiveCompletedImage();
        }

        private void CheckButtonDayBehaviour()
        {
            foreach (IPanelDayModel panelDayModel in _models.Values)
                if (CheckCompletedDay(panelDayModel) == false)
                    _buttonsDay.FirstOrDefault(button => button.NumberDay == panelDayModel.NumberDay)
                        .SetActiveCompletedImage();
        }


        private void CheckAllAwardsHaveBeenTaken(int _ = default, int i = default)
        {
            foreach (IPanelDayModel panelDayModel in _models.Values)
            {
                if (CheckCompletedDay(panelDayModel))
                    continue;

                ImageControllerButton.ActiveImage();
                return;
            }

            ImageControllerButton.InactiveImage();
        }

        private bool CheckCompletedDay(IPanelDayModel panelDayModel)
        {
            foreach (var questModel in panelDayModel.QuestsModelDay)
                if (questModel.Status == StatusQuest.Take)
                    return false;

            return true;
        }

        private void OnNextDayArrived(int openDay)
        {
            if (_currantActivePanel.NumberDay + openDay > _models.Keys.Count)
                return;

            for (int i = 1; i <= openDay; i++)
            {
                PanelDayView dayView = _models.Keys.FirstOrDefault(panelDayView =>
                    panelDayView.NumberDay == _currantActivePanel.NumberDay + 1);

                if (CheckActiveDay(dayView))
                    continue;

                ActiveDay(dayView);
                _currantActivePanel = dayView;
            }

            PrefabPanelBeginnersFestView.SetNextDay();
        }

        private bool CheckActiveDay(PanelDayView dayView)
        {
            foreach (IQuestModel model in _models[dayView].QuestsModelDay)
                if (model.Status != StatusQuest.Closed)
                    return true;

            return false;
        }

        private IPanelDayModel LoadPanelDayModel(QuestView QuestViewPrefab, PanelDayView panelDayView, int i)
        {
            IPanelDayModel dayModel = new PanelDayModel();
            dayModel.LoadContent(Character.Instance.CharacterData.ProgressBeginnersFest.Days[i], QuestViewPrefab,
                panelDayView.ContentQuest.transform, _saveLoadServiceBeginnersFest, panelDayView.NumberDay);

            return dayModel;
        }

        private IPanelDayModel CreatePanelDayModel(QuestView QuestViewPrefab, PanelDayView panelDayView, int i)
        {
            IPanelDayModel dayModel = new PanelDayModel();
            dayModel.CreateContent(Game.instance.GetBeginnersFestData().DailyQuestsList[i].Quests, QuestViewPrefab,
                panelDayView.ContentQuest.transform, _saveLoadServiceBeginnersFest, panelDayView.NumberDay);

            return dayModel;
        }

        private void SetActiveDay()
        {
            foreach (PanelDayView panelDayView in _models.Keys)
                SetStatusOnDay(panelDayView, StatusQuest.Closed);

            ActiveDay(_currantActivePanel);
        }

        private void ActiveDay(PanelDayView panelDay)
        {
            foreach (QuestView questView in _models[panelDay].QuestsViewDay)
                _models[panelDay].ActiveQuest(questView);

            SetStatusOnDay(panelDay, StatusQuest.InProgress);
        }

        private void OnClickedButtonDayView(int day)
        {
            _currentPanel.gameObject.SetActive(false);
            PanelDayView dayView = _models.Keys.FirstOrDefault(panelDayView => panelDayView.NumberDay == day);
            SetCurrentButton(day);
            _currentPanel = dayView;
            _currentPanel.gameObject.SetActive(true);
        }

        private void SetCurrentButton(int day)
        {
            ButtonDayView currentButton = _buttonsDay.FirstOrDefault(view => view.NumberDay == _currentPanel.NumberDay);
            currentButton.SetButtonIconDefault();
            currentButton = _buttonsDay.FirstOrDefault(view => view.NumberDay == day);
            currentButton.SetButtonIconCurrent();
        }

        private void SetStatusOnDay(PanelDayView panelDay, StatusQuest status)
        {
            foreach (IQuestModel questModel in _models[panelDay].QuestsModelDay)
            {
                questModel.ChangeStatusQuest(status);
                _models[panelDay].GetQuestView(questModel).SetStatusButtonGoToQuest(status);
            }
        }

        private void StartWorkPanel(bool isLoad = false)
        {
            _progressBarModel = new ProgressBarModel(_models.Values.ToList(),
                PrefabPanelBeginnersFestView.PrefabRewardsProgressBarView, prefabRewardView,
                _saveLoadServiceBeginnersFest);

            SetRewardsProgressBar(isLoad);

            _progressBarModel.CreateChase(isLoad);
            _progressBarModel.Subscribe();

            _progressBarModel.ChangedRewardCount +=
                PrefabPanelBeginnersFestView.PrefabRewardsProgressBarView.ChangeRewardsProgressBar;

            if (isLoad)
                PrefabPanelBeginnersFestView.PrefabRewardsProgressBarView.ChangeRewardsProgressBar(Character.Instance
                    .CharacterData.ProgressBeginnersFest.ProgressOnBar);

            PrefabPanelBeginnersFestView.SetStartData(isLoad
                ? Character.Instance.CharacterData.ProgressBeginnersFest.StartData.ToDataTime()
                : DateTime.Today);

            PrefabPanelBeginnersFestView.SetNextDay();
            PrefabPanelBeginnersFestView.NextDayArrived += OnNextDayArrived;
        }

        private bool CheckMonoBehaviourQuestStat()
        {
            foreach (IPanelDayModel dayModel in _models.Values)
            {
                IQuestModel questModel = dayModel.QuestsModelDay.FirstOrDefault(x => x.Status == StatusQuest.Take);

                if (questModel != null)
                    return true;
            }

            return false;
        }

        private void SetRewardsProgressBar(bool isLoad) =>
            PrefabPanelBeginnersFestView.PrefabRewardsProgressBarView.SetRewardsProgressBar(isLoad
                ? Character.Instance.CharacterData.ProgressBeginnersFest.ProgressOnBar
                : 0);
    }
}
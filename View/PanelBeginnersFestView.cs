using System;
using _Development.Scripts.BeginnersFest.View.Bar;
using BLINK.RPGBuilder.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace _Development.Scripts.BeginnersFest.View
{
    public class PanelBeginnersFestView : MonoBehaviour
    {
        public event Action<int> NextDayArrived;

        public RewardsProgressBarView PrefabRewardsProgressBarView;
        public GameObject ContentDays;
        public GameObject ContentButtonDays;
        public Image BeackgrundImage;
        public Button ExitButton;
        private DateTime NextDay;
        private DateTime _startData;
        private int _openDay;

        private DateTime TestData;

        private void OnEnable()
        {
            ExitButton.onClick.AddListener(ClosedPanel);

            if (NextDay < DateTime.Today)
                NextDayArrived?.Invoke(CheckDataStart());
        }

        private void OnDisable() =>
            ExitButton.onClick.RemoveListener(ClosedPanel);

        public void SetStartData(DateTime startData) =>
            _startData = startData;

        public void SetNextDay() =>
            NextDay = NextDay == default ? _startData : DateTime.Today.AddDays(1);

        public void SetBackground(Sprite sprite) =>
            BeackgrundImage.sprite = sprite;

        private void ClosedPanel() =>
            gameObject.SetActive(false);

        private int CheckDataStart()
        {
            for (int i = 1; i < Character.Instance.CharacterData.ProgressBeginnersFest.Days.Count; i++)
            {
                if (NextDay.AddDays(i) < DateTime.Today)
                    continue;

                return i;
            }

            return Character.Instance.CharacterData.ProgressBeginnersFest.Days.Count;
        }
    }
}
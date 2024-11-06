using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Development.Scripts.BeginnersFest.View.Bar
{
    public class RewardsProgressBarView : MonoBehaviour
    {
        public TMP_Text _textQuestDescription;
        public RectTransform SizeProgressBar;
        public ImageBarView UpperBar;
        public ImageBarView MiddleBar;
        public ImageBarView LowerBar;
        public Image LeftWallBar;
        public Image RightWallBar;

        private List<IPanelDayModel> _panelDayModel;
        private float _countValue;
        private float _countValueText;
        private float _multiplier;

        public float MaxValueOneBar { get; private set; }
        public float MaxBar { get; private set; }

        public void SetRewardsProgressBar(int startValue)
        {
            MaxBar = SizeProgressBar.rect.size.x * 3;
            MaxValueOneBar = SizeProgressBar.rect.size.x;
            _textQuestDescription.text = startValue.ToString();
        }

        public Transform GetContainerForChest(float value)
        {
            if (value <= MaxValueOneBar)
                return UpperBar.Container.transform;

            if (value > MaxValueOneBar && value <= MaxValueOneBar * 2)
                return MiddleBar.Container.transform;

            return LowerBar.Container.transform;
        }

        public void ChangeRewardsProgressBar(int value)
        {
            _countValueText += value;
            _countValue += value * _multiplier;

            if (_countValue < MaxValueOneBar)
            {
                SetValueBar(UpperBar.Bar, _countValue, MaxValueOneBar);
            }
            else if (_countValue > MaxValueOneBar && _countValue < MaxValueOneBar * 2)
            {
                ActiveBar();
                SetValueBar(MiddleBar.Bar, _countValue - MaxValueOneBar, MaxValueOneBar);
            }
            else
            {
                ActiveBar(true);
                SetValueBar(LowerBar.Bar, _countValue - (MaxValueOneBar * 2), MaxValueOneBar);
            }
        }

        private void ActiveBar(bool isFull = false)
        {
            if (isFull)
            {
                MiddleBar.Bar.fillAmount = 1;
                LeftWallBar.fillAmount = 1;
            }

            UpperBar.Bar.fillAmount = 1;
            RightWallBar.fillAmount = 1;
        }

        private void SetValueBar(Image bar, float value, float maxBar)
        {
            bar.fillAmount = value / maxBar;
            _textQuestDescription.text = $"{_countValueText}";
        }

        public void SetMultiplier(float multiplier) =>
            _multiplier = multiplier;
    }
}
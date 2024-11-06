using System;
using _Development.Scripts.BeginnersFest.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Development.Scripts.BeginnersFest.View
{
    public class QuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textQuestDescription;
        [SerializeField] private TMP_Text _textQuestRewardCount;
        [SerializeField] private TMP_Text _textButtonGoToQuest;
        [SerializeField] private TMP_Text _textButtonGetReward;
        [SerializeField] private TMP_Text _textBarProgress;
        [SerializeField] private Button _buttonGoToQuest;
        [SerializeField] private Button _buttonGetReward;
        [SerializeField] private Image _barProgress;
        [SerializeField] private ColorButtonForQuestStatus _colorButtonForQuestStatus;
        [SerializeField] private GameObject _inactiveQuest;

        public event Action<QuestView> ClickedGetReward;

        public void SetInfoForQuest(string textRewardCount, string textDescription, string maxCount,
            string startCount = "0")
        {
            _textQuestRewardCount.text = textRewardCount;
            _textQuestDescription.text = textDescription;
            SetProgressBar(startCount, maxCount);
        }

        public void SetProgressBar(string currentCount, string maxCount)
        {
            _textBarProgress.text = $"{currentCount} / {maxCount}";

            if (int.TryParse(currentCount, out int count) == false ||
                int.TryParse(maxCount, out int required) == false)
                return;

            if (count == 0)
                _barProgress.fillAmount = count;
            else
                _barProgress.fillAmount = (float)count / required;
        }

        public void SetStatusButtonGoToQuest(StatusQuest status)
        {
            _textButtonGoToQuest.text = status.ToString();
            _buttonGoToQuest.image.color = _colorButtonForQuestStatus.GetColor(status);
            ;
        }

        public void SetStatusButtonGetReward(StatusQuest status)
        {
            _textButtonGetReward.text = status.ToString();
            _buttonGetReward.image.color = _colorButtonForQuestStatus.GetColor(status);
            
            if(status == StatusQuest.Taken)
                _inactiveQuest.gameObject.SetActive(true);
        }

        public void OnCompleted(StatusQuest status)
        {
            _buttonGoToQuest.gameObject.SetActive(false);
            _buttonGetReward.gameObject.SetActive(true);
            SetStatusButtonGetReward(status);

            if (status == StatusQuest.Take)
                _buttonGetReward.onClick.AddListener(OnClickedGetReward);
        }

        private void OnClickedGetReward()
        {
            ClickedGetReward.Invoke(this);
            _buttonGetReward.onClick.RemoveListener(OnClickedGetReward);
        }
    }
}
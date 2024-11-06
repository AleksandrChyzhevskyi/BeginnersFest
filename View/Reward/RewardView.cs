using System;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.View.Reward.Chest;
using TMPro;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.View.Reward
{
    public class RewardView : MonoBehaviour
    {
        public event Action<RewardView> AwardReceived;
        public event Action<RewardView> ChangedStatus;

        public TMP_Text TextCount;
        public ImageRewardView Chest;
        public ImageRewardView Wolf;
        public ImageRewardView Bear;

        private ImageRewardView _imageRewardView;

        public StatusQuest Status { get; private set; }

        public void SetInfoReward(string textCount, AnimalsToEffectID effectID)
        {
            _imageRewardView = effectID switch
            {
                AnimalsToEffectID.Default => Chest,
                AnimalsToEffectID.Wolf => Wolf,
                AnimalsToEffectID.Bear => Bear,
                _ => _imageRewardView
            };

            _imageRewardView.gameObject.SetActive(true);
            _imageRewardView.SetEffect(effectID);
            TextCount.text = textCount;
        }

        public void GetReward()
        {
            if (_imageRewardView.Closed.gameObject.activeInHierarchy == false)
                return;

            SetStateOpen();
            AwardReceived?.Invoke(this);
        }

        public void SetStateInactive()
        {
            _imageRewardView.Closed.gameObject.SetActive(false);
            _imageRewardView.Opend.gameObject.SetActive(false);
            _imageRewardView.Inactive.gameObject.SetActive(true);
            Status = StatusQuest.InProgress;
            ChangedStatus?.Invoke(this);
        }

        public void SetStateClose()
        {
            _imageRewardView.Inactive.gameObject.SetActive(false);
            _imageRewardView.Opend.gameObject.SetActive(false);
            _imageRewardView.Closed.gameObject.SetActive(true);
            
            if (_imageRewardView.Effect != AnimalsToEffectID.Default)
                _imageRewardView.SetTextForIcon();
            
            Status = StatusQuest.Take;
            ChangedStatus?.Invoke(this);
        }

        public void SetStateOpen()
        {
            _imageRewardView.Inactive.gameObject.SetActive(false);
            _imageRewardView.Closed.gameObject.SetActive(false);
            _imageRewardView.Opend.gameObject.SetActive(true);

            if (_imageRewardView.Effect != AnimalsToEffectID.Default)
               _imageRewardView.SetTextForIcon();
            
            Status = StatusQuest.Taken;
            ChangedStatus?.Invoke(this);
        }
    }
}
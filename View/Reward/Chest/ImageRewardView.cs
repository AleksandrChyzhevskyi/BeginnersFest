using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Development.Scripts.BeginnersFest.View.Reward.Chest
{
    public class ImageRewardView : MonoBehaviour
    {
        public Image Inactive;
        public Image Opend;
        public Image Closed;
        public TMP_Text TextCount;
        
        public AnimalsToEffectID Effect { get; private set; }

        public void SetEffect(AnimalsToEffectID animalsToEffectID) => 
            Effect = animalsToEffectID;

        public void SetTextForIcon() => 
            TextCount.text = Effect.ToString();
    }
}
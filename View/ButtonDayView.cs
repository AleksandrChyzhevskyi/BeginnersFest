using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Development.Scripts.BeginnersFest.View
{
    public class ButtonDayView : MonoBehaviour
    {
        public event Action<int> Clicked;

        public Button DayButton;
        public TMP_Text TextButton;
        public Sprite CurrentDayIcon;
        public Image CompletedImage;
        public Sprite DefaultIcon;
        
        public int NumberDay { get; private set; }

        private void Start() => 
            DayButton.onClick.AddListener(OnClicked);

        private void OnDestroy() =>
            DayButton.onClick.RemoveListener(OnClicked);

        public void Initialize(int numberDay)
        {
            NumberDay = numberDay;
            TextButton.text = $"Day {numberDay}";
        }

        public void SetActiveCompletedImage() => 
            CompletedImage.gameObject.SetActive(true);
        
        public void SetInactiveCompletedImage() => 
            CompletedImage.gameObject.SetActive(false);

        public void SetButtonIconCurrent() => 
            DayButton.image.sprite = CurrentDayIcon;

        public void SetButtonIconDefault() => 
            DayButton.image.sprite = DefaultIcon;

        private void OnClicked() =>
            Clicked?.Invoke(NumberDay);
    }
}
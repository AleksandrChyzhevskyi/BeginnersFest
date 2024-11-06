using System;
using Sirenix.OdinInspector;

namespace _Development.Scripts.BeginnersFest.Data
{
    [Serializable]
    public class Reward
    {
        public int CountCurrency;
        public bool IsNewSkin;
        
        [ShowIf("@this.IsNewSkin == false")]
        public RPGLootTable RewardInChest;

        [ShowIf("IsNewSkin")] 
        public RPGEffect Skin;
        [ShowIf("IsNewSkin")] 
        public float WaitInSecond;
    }
}
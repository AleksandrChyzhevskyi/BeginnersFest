using System;
using System.Collections.Generic;
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.Upgrade.Data;
using Sirenix.OdinInspector;

namespace _Development.Scripts.BeginnersFest.Data
{
    [Serializable]
    public class Quest
    {
        [Title("TypeQuest: ")] [HideLabel] [EnumPaging]
        public TypeTest TypeQuest;

        [ShowIf("@this.TypeQuest != TypeTest.Default && this.TypeQuest != TypeTest.KillBoss")]
        public int Count;

        [ShowIf("@this.TypeQuest == TypeTest.PutOnAnItem " +
                "|| this.TypeQuest == TypeTest.ChangeLevel " +
                "|| this.TypeQuest == TypeTest.ResistDamage " +
                "|| this.TypeQuest == TypeTest.Dodge" +
                "|| this.TypeQuest == TypeTest.DealDamage" +
                "|| this.TypeQuest == TypeTest.DealCriticalDamage")]
        public bool IsDefault = true;

        [ShowIf("@this.TypeQuest == TypeTest.OpenChest")]
        public bool IsADChest;

        [ShowIf("@this.IsDefault == false && this.TypeQuest == TypeTest.PutOnAnItem")]
        public RPGItem Item;

        [ShowIf("TypeQuest", TypeTest.Upgrade)]
        public UpgradeData Upgrade;

        [ShowIf("TypeQuest", TypeTest.UseBuster)]
        public RPGEffect Buster;

        [ShowIf("TypeQuest", TypeTest.KillEnemies)]
        public bool IsHaveType;

        [ShowIf("@this.IsHaveType && this.TypeQuest == TypeTest.KillEnemies")]
        public RPGNpc Npc;

        [ShowIf("TypeQuest", TypeTest.KillBoss)]
        public List<RPGNpc> Npcs;

        [ShowIf("@this.TypeQuest == TypeTest.Earn || this.TypeQuest == TypeTest.Spend")]
        public RPGCurrency Currency;

        [ShowIf("@this.TypeQuest == TypeTest.KillEnemies || this.TypeQuest == TypeTest.KillBoss")]
        public bool IsHaveForm;

        [ShowIf("@this.IsHaveForm || this.TypeQuest == TypeTest.Unlock || this.TypeQuest == TypeTest.SelectForms")]
        public AnimalsToEffectID Unlock;

        [ShowIf("@this.TypeQuest != TypeTest.Default")] [Title("Reward count: ")] [HideLabel]
        public int Quantity;
    }
}
using _Development.Scripts.BeginnersFest.Enums;
using _Development.Scripts.BeginnersFest.SaveLoadData;

namespace _Development.Scripts.BeginnersFest.Interface
{
    public interface ISaveLoadServiceBeginnersFest
    {
        void Save(ProgressPlayerBeginnersFest saveData = null);
        void SaveQuest(int NumberDay, IQuestModel questModel);
        void SaveProgressOnBar(int ProgressOnBar);
        void SaveChest(int CountCurrency, StatusQuest status);
        ProgressPlayerBeginnersFest Load();
    }
}
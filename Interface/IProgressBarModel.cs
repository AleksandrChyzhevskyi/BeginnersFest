using System;

namespace _Development.Scripts.BeginnersFest.Interface
{
    public interface IProgressBarModel
    {
        event Action<int> ChangedRewardCount;
        void Subscribe();
        void Unsubscribe();
        void CreateChase(bool isLoad = false);
    }
}
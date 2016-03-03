using System;
using System.Collections.Generic;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class PlayerQuestProgress
    {
        public int questID;
        public List<int> targetProgress;

        public PlayerQuestProgress(int qID, List<int> tP)
        {
            questID = qID;
            targetProgress = tP;
        }
    }
}

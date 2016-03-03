using System;
using System.Collections.Generic;
using Common;
using Database.Static;

namespace Gameplay.Quests
{
    [Serializable]
    public class QuestChain
    {
        public string internalName;
        public SBLocalizedString localizedName;
        public EQuestArea questArea;
        public List<Quest_Type> quests = new List<Quest_Type>();
    }
}
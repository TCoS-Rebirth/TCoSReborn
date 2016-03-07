using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay.Quests.QuestTargets;
using Gameplay.RequirementSpecifier;
using UnityEngine;

namespace Gameplay.Quests
{
    [Serializable]
    public class Quest_Type
    {
        public bool Disabled;

        public bool deliverByMail;

        public SBResource finisher;
        public string internalName;
        public int money;
        public SBLocalizedString nameLocStr, summaryLocStr;
        public string note;
        public List<SBResource> preQuests;
        public SBResource provideCT, midCT, finishCT;

        public SBResource provider;

        public EQuestArea questArea;

        public QuestPoints questPoints;

        public List<Content_Requirement> requirements;

        public int resourceID, level;
        public Content_Inventory rewardItems;
        public string tag;

        [SerializeField] public List<QuestTarget> targets;
    }
}
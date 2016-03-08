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

        [SerializeField]
        public List<QuestTarget> targets;

        //Returns the array index of a target's pretarget in the targets array
        public int getPretargetIndex(int targetID, int pretargetID)
        {
            foreach (var target in targets)
            {
                //Match targetID
                if (target.resource.ID == targetID)
                {
                    foreach (var pretarget in target.Pretargets)
                    {
                        //Match pretarget
                        if (pretarget.ID == pretargetID)
                        {

                            //Find what index the pretarget ID holds in targets
                            for (int n = 0; n < targets.Count; n++)
                            {
                                if (targets[n].resource.ID == pretargetID)
                                {
                                    return n;
                                }
                            }
                            return -1;
                        }
                        
                    }
                    return -1;  //Failed to match pretarget ID
                }
                
            }
            return -1;  //Failed to match target ID
        }
 
    }
}
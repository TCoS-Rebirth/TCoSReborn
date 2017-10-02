using System;
using Gameplay.Entities;
using Database.Static;
using UnityEngine;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestFinished : Content_Requirement
    {
        public SBResource RequiredQuest; //Quest_Type
        public int TimesFinished;

        public override bool isMet(PlayerCharacter p)
        {
            //TODO: Implement properly for multiple quest finishes
            //When that value is stored for players (in DB)

            foreach (var questID in p.questData.completedQuestIDs)
            {
                if (questID == RequiredQuest.ID)
                {
                    switch (TimesFinished)
                    {
                        case 0:
                            return false;

                        case 1:
                            return true;

                    }
                    //TODO
                    Debug.Log("Req_QuestFinished : Multiple quest finishes NYI!");
                    return false;
                }
            }

            if (TimesFinished == 0) return true;
            else return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (!p) return false;
            return isMet(p);
        }
    }
}
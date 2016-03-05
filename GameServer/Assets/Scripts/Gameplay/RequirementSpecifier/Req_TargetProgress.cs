using System;
using Common;
using Gameplay.Entities;
using Database.Static;
using UnityEngine;

namespace Gameplay.RequirementSpecifier
{
    public class Req_TargetProgress : Content_Requirement
    {
        public int objective;
        public EContentOperator Operator;
        public int Progress;
        public SBResource quest; //Quest_Type

        public override bool isMet(PlayerCharacter p)
        {
            //Find quest
            foreach (var q in p.QuestData.curQuests)
            {
                if (q.questID == quest.ID)
                {
                    if (q.targetProgress != null)
                    {
                        if (objective > (q.targetProgress.Count - 1) )
                        {
                            Debug.Log("Req_TargetProgress.isMet : objective index higher than any array index");
                            return false;
                        }
                        int playerProgress = q.targetProgress[objective];
                        if (SBOperator.Operate(playerProgress, Operator, Progress)) { return true; }
                        else { return false; }
                    }
                    else {
                        Debug.Log("Req_TargetProgress.isMet : Null quest targetProgress");
                        return false;
                    }

                }
            }
            return false;          
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}
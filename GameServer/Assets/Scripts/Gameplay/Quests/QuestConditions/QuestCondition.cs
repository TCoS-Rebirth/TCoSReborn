using System.Collections.Generic;
using Gameplay.Quests.QuestTargets;
using Database.Static;
using Gameplay.Entities;

namespace Gameplay.Quests.QuestConditions
{
    public class QuestCondition : QuestTarget
    {
        public List<SBResource> FinalTargetIDs;

        public bool HasFinalTarget(SBResource targetRes)
        {
            foreach(var ftID in FinalTargetIDs)
            {
                if (targetRes.ID == ftID.ID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If all pretargets are complete and condition are fulfiled
        /// Sets progress to 1 and returns true
        /// otherwise sets progress to 0, returns false
        /// Note : when overriding, call this base method *AFTER* subclass-specific code
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="parentQuest"></param>
        /// <returns></returns>
        public virtual bool UpdateAndCheck(PlayerCharacter pc, Quest_Type parentQuest)
        {
            var qID = parentQuest.resourceID;
            var tarInd = parentQuest.getTargetIndex(resource.ID);

            if (pc.PreTargetsComplete(this, parentQuest))
            {
                pc.SetQTProgress(qID, tarInd, 1);
                return true;
            }
            else
            {
                
                pc.SetQTProgress(qID, tarInd, 0);
                return false;
            }
                
        }
    }
}
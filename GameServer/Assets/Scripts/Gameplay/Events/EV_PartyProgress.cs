using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.Events
{
    public class EV_PartyProgress : Content_Event
    {
        public int ObjectiveNr;
        public SBResource quest; //Quest_Type

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (!(subject as PlayerCharacter)) return false;
            var fullQuest = GameData.Get.questDB.GetQuest(quest);
            if (fullQuest == null) return false;
            if (ObjectiveNr < 0 || ObjectiveNr >= fullQuest.targets.Count) return false;

            return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            var team = playerSubj.Team;
            if (team == null) return;

            var fullQuest = GameData.Get.questDB.GetQuest(quest);

            foreach (var teamMember in team.Members)
            {
                
                if (teamMember == playerSubj) continue;

                if (    teamMember.HasQuest(quest.ID)
                    &&  teamMember.PreTargetsComplete(fullQuest.targets[ObjectiveNr], fullQuest)
                    &&  !teamMember.QuestTargetIsComplete(fullQuest,ObjectiveNr))
                {
                    teamMember.CompleteQT(fullQuest, ObjectiveNr);
                }
                    
            }
        }
    }
}
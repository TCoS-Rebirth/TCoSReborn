using System;
using Database.Static;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_QuestProgress : Content_Event
    {
        public int Progress;
        public SBResource quest; //Quest_Type
        public int TargetNr;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            if (!playerSubj) return false;
            if (!playerSubj.HasQuest(quest.ID)) return false;
            if (TargetNr >= playerSubj.questData.getNumTargets(quest.ID)) return false;

            return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            playerSubj.SetQTProgress(quest.ID, TargetNr, Progress);
        }
    }
}
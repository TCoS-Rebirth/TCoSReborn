using System;
using Gameplay.Entities;
using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_GiveSkill : Content_Event
    {
        public FSkill_Type skill; //"Skill"
        public int skillID;
        public string temporarySkillName;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }
    }
}
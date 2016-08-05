using System;
using Gameplay.Entities;
using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_Skill : Content_Event
    {
        public FSkill_Type Skill;
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
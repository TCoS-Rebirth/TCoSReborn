using System;
using Gameplay.Entities;
using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_SkillEffects : Content_Event
    {
        public FSkill Skill;
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
        /*
if (aObject != None) {                                                      //0000 : 07 30 00 77 00 58 C2 FF 13 2A 16
aObject.Skills.sv_DirectSkillEffects(Skill,aObject);                      //000B : 19 19 00 58 C2 FF 13 05 00 04 01 08 28 18 11 10 00 00 1C D0 C5 21 11 01 70 C3 FF 13 00 58 C2 FF 13 16
} else {                                                                    //002D : 06 52 00
aSubject.Skills.sv_DirectSkillEffects(Skill,aSubject);                    //0030 : 19 19 00 E8 C3 FF 13 05 00 04 01 08 28 18 11 10 00 00 1C D0 C5 21 11 01 70 C3 FF 13 00 E8 C3 FF 13 16
}
}
*/
    }
}
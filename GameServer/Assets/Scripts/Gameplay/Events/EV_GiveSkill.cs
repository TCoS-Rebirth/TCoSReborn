using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_GiveSkill : Content_Event
    {
        public FSkill skill; //"Skill"
        public int skillID;
        public string temporarySkillName;
    }
}
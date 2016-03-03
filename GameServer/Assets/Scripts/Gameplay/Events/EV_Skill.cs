using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_Skill : Content_Event
    {
        public FSkill Skill;
        public int skillID;
        public string temporarySkillName;
    }
}
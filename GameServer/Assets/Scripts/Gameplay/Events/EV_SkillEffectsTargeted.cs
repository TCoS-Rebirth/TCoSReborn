using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_SkillEffectsTargeted : Content_Event
    {
        public FSkill Skill;
        public int skillID;
        public string temporarySkillName;
    }
}
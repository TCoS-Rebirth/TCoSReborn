using Common;
using Gameplay.Skills;

namespace Gameplay.Items.ItemComponents
{
    public class IC_BodySlot : Item_Component
    {
        public enum IC_BodySlotType
        {
            ICBS_Spirit,
            ICBS_Soul,
            ICBS_Rune
        }

        public SkillBodySlot FakeSkill;
        public int fakeSkillID;
        public EContentClass ForClass;
        public string temporaryFakeSkillName;
        public IC_BodySlotType Type;
        public bool UserStartable;
    }
}
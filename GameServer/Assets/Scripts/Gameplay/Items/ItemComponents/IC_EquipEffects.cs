using Gameplay.Skills.Events;

namespace Gameplay.Items.ItemComponents
{
    public class IC_EquipEffects : Item_Component
    {
        public enum EEquipTattooBodyPart
        {
            ETBP_Torso,
            ETBP_LeftArm,
            ETBP_RightArm,
            ETBP_Head
        }

        public enum EEquipTattooSet
        {
            ETS_None,
            ETS_BloodWarrior1,
            ETS_BloodWarrior2,
            ETS_BloodWarrior3,
            ETS_BloodWarrior4,
            ETS_RuneMage1,
            ETS_RuneMage2,
            ETS_RuneMage3,
            ETS_RuneMage4
        }

        public SkillEventDuff EquipDuffEvent;
        public int equipDuffEventID;
        public EEquipTattooBodyPart EquipTattooBodyPart;
        public EEquipTattooSet EquipTattooSet;
        public string temporaryEquipDuffEvent;
    }
}
using System.Collections.Generic;
using Gameplay.Skills.Effects;

namespace Gameplay.Items.ItemComponents
{
    public class IC_TokenItem : Item_Component
    {
        public List<int> equipEffectIDs = new List<int>();
        public List<SkillEffectDuff> EquipEffects = new List<SkillEffectDuff>();
        public int ForgePrice;
        public int ForgeRemovePrice;
        public int ForgeReplacePrice;
        public byte SlotType;
        public List<string> temporarySkillEffectDuffNames = new List<string>();
        public int TokenRank;
    }
}
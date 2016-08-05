namespace Database.Dynamic.Internal
{
    public class DBSkill
    {
        public int ResourceId;
        public int SigilSlots;
        public int SkillDeckSlot = -1;

        public DBSkill(int resID, int numSigilSlots, int deckSlot = -1)
        {
            ResourceId = resID;
            SigilSlots = numSigilSlots;
            SkillDeckSlot = deckSlot;
        }
    }
}
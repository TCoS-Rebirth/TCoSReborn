namespace Database.Dynamic.Internal
{
    public class DBSkill
    {
        public int ResourceId;
        public int SigilSlots;

        public DBSkill(int resID, int numSigilSlots)
        {
            ResourceId = resID;
            SigilSlots = numSigilSlots;
        }
    }
}
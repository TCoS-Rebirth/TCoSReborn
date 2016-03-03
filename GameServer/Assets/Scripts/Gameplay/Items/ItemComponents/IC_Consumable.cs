namespace Gameplay.Items.ItemComponents
{
    public class IC_Consumable : Item_Component
    {
        public enum IC_ConsumableType
        {
            ICT_Food,
            ICT_Drink,
            ICT_Potion
        }

        public IC_ConsumableType Type;
    }
}
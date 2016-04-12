
namespace Gameplay.Loot
{
    /// <summary>
    /// Client representation of a loot item
    /// Loot items are represented like this for initiating loot transactions
    /// </summary>
    public class ReplicatedLootItem
    {
        public int ResourceId;
        public int LootItemID;
        public int Quantity;

        public void SetupFromDLI (LootTransaction.DroppedLootItem dli)
        {
            ResourceId = dli.Item.Item.resourceID;
            LootItemID = dli.LootItemID;
            Quantity = dli.Item.Quantity;
        }
    }
}

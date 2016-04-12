namespace Gameplay.Loot
{
    /// <summary>
    /// This class only appears in later Spellborn versions than currently used
    /// Revisit this if we upgrade to the later versions!
    /// </summary>
    public class MasterLootItem : LootItem
    {
        [ReadOnly]
        public int selectedCharacterID;

    }
}
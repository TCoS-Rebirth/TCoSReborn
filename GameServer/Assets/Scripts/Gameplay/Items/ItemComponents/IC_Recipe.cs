using System;
using System.Collections.Generic;

namespace Gameplay.Items.ItemComponents
{
    public class IC_Recipe : Item_Component
    {
        public List<RecipeComponent> Components = new List<RecipeComponent>();
        public Item_Type ProducedItem;
        public int producedItemID;
        public List<RecipeComponent> RecycleComponents = new List<RecipeComponent>();
        public string temporaryProducedItemName;

        [Serializable]
        public class RecipeComponent
        {
            public Item_Type Item;
            public int itemID;
            public int Quantity;
            public string temporaryItemName;
        }
    }
}
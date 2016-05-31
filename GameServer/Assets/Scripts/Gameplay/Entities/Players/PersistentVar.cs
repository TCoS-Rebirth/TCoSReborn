using System;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class PersistentVar
    {
        public int ContextID;
        public int VarID;
        public int Value;
    }
}

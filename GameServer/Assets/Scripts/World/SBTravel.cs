using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SBTravel : ScriptableObject
    {
        public string Tag;
        public List<SBRoute> Routes = new List<SBRoute>();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Quests
{
    [Serializable]
    public class QuestCollection : ScriptableObject
    {
        public List<Quest_Type> looseQuests = new List<Quest_Type>();
        public List<QuestChain> questChains = new List<QuestChain>();
    }
}
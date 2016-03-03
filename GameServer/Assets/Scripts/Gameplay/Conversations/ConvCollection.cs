using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Conversations
{
    public class ConvCollection : ScriptableObject
    {
        public List<ConversationTopic> topics = new List<ConversationTopic>();

        internal ConversationTopic GetTopic(int rID)
        {
            for (var i = 0; i < topics.Count; i++)
            {
                if (topics[i].resource.ID == rID)
                {
                    return topics[i];
                }
            }
            return null;
        }
    }
}
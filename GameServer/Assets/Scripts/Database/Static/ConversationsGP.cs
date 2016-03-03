using System.Collections.Generic;
using Gameplay.Conversations;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using UnityEngine;

namespace Database.Static
{
    public class ConversationsGP : ScriptableObject
    {
        public List<Content_Event> events = new List<Content_Event>();
        public List<Content_Requirement> reqs = new List<Content_Requirement>();
        public List<ConversationTopic> topics = new List<ConversationTopic>();
    }
}
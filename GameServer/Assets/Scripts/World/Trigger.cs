using Gameplay.Entities;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class Trigger : MonoBehaviour
    {
        [SerializeField, ReadOnly] public Zone owningZone;

        [ReadOnly]
        public List<Content_Requirement> Requirements;
        [ReadOnly]
        public List<Content_Event> Actions;
        [ReadOnly]
        public List<Character> CharsInside = new List<Character>();

        [ReadOnly]
        public float Radius, Height;
        [ReadOnly]
        public string Tag;

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public List<PlayerCharacter> PlayersInside
        {
            get
            {
                var list = new List<PlayerCharacter>();
                foreach (var ci in CharsInside)
                {
                    var pc = ci as PlayerCharacter;
                    if (pc != null) list.Add(pc);
                }
                return list;
            }
        }

        public int PlayerCount
        {
            get
            {
                var count = 0;
                foreach (var ch in CharsInside)
                {
                    if (ch as PlayerCharacter != null) { count++; }
                }
                return count;
            }
        }

        void Start()
        {
            var col = GetComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = Radius;
            col.height = Height;
        }

        protected virtual void OnEnteredTrigger(Character ch)
        {
            refreshChars();                        
            if (reqsMet(ch)) CharsInside.Add(ch);
        }

        protected virtual void OnLeftTrigger(Character ch)
        {            

            CharsInside.Remove(ch);
            refreshChars();

        }

        void OnTriggerEnter(Collider other)
        {
            var ch = owningZone.FindCharacter(other.gameObject);
            if (ch != null)
            {
                OnEnteredTrigger(ch);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var ch = owningZone.FindCharacter(other.gameObject);
            if (ch != null)
            {
                OnLeftTrigger(ch);
            }
        }

        protected virtual void refreshChars()
        {
            foreach (var character in CharsInside)
            {
                if (    character == null
                    ||  character.ActiveZone != owningZone)
                {
                    CharsInside.Remove(character);
                    return;
                }

                var pc = character as PlayerCharacter;
                if (pc != null)
                {
                    if (!pc.Owner.IsIngame)
                    {
                        CharsInside.Remove(character);
                    }
                }
            }
        }

        protected bool reqsMet(Character c)
        {
            var pc = c as PlayerCharacter;
            if (pc != null) { return reqsMet(pc); }
            var nc = c as NpcCharacter;
            if (nc != null) { return reqsMet(nc); }

            return false;
        }

        protected bool reqsMet(PlayerCharacter pc)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(pc))
                {
                    return false;
                }
            }
            return true;
        }

        protected bool reqsMet(NpcCharacter nc)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(nc))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
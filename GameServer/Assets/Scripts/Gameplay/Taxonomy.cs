#pragma warning disable 414
using System;
using System.Collections.Generic;
using Gameplay.Entities.NPCs;
using UnityEngine;
using Gameplay.Loot;

namespace Gameplay
{
    [Serializable]
    public class Taxonomy : ScriptableObject
    {
        [SerializeField] public int cachedColorArmor1;

        [SerializeField] public int cachedColorArmor2;

        [SerializeField] public int cachedColorCloth1;

        [SerializeField] public int cachedColorCloth2;

        [SerializeField, ReadOnly] public NPCCollection classPackage;

        [SerializeField, ReadOnly] string description;

        [SerializeField, ReadOnly] public List<Taxonomy> dislikes = new List<Taxonomy>();

        [SerializeField, ReadOnly] int id;

        [SerializeField, ReadOnly] public List<Taxonomy> likes = new List<Taxonomy>();

        [SerializeField, ReadOnly] string note;

        [SerializeField, ReadOnly] public Taxonomy parent;

        [SerializeField, ReadOnly]
        public List<LootTable> Loot;

        //[SerializeField] public List<string> temporaryLootTableNames = new List<string>();

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Description
        {
            set { description = value; }
        }

        public string Note
        {
            set { note = value; }
        }

        public bool Likes(Taxonomy otherFaction)
        {
            if (likes.Contains(otherFaction))
            {
                return true;
            }
            if (parent != null)
            {
                return parent.Likes(otherFaction);
            }
            return false;
        }

        public bool Hates(Taxonomy otherFaction)
        {
            if (dislikes.Contains(otherFaction))
            {
                return true;
            }
            if (parent != null)
            {
                return parent.Hates(otherFaction);
            }
            return false;
        }
    }
}
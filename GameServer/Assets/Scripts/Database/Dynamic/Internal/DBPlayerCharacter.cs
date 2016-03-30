using System.Collections.Generic;
using Common;
using Gameplay;
using Gameplay.Items;
using UnityEngine;

namespace Database.Dynamic.Internal
{
    /// <summary>
    ///     Used to save playercharacters to the database
    /// </summary>
    public class DBPlayerCharacter
    {
        public int AccountID;
        public CharacterAppearance Appearance;
        public int ArcheType;
        public int[] BodyMindFocus = new int[3];
        public int DBID;
        public int[] ExtraBodyMindFocusAttributePoints = new int[4];
        public int Faction;
        public int[] FamePep = new int[2];
        public float FamePoints;
        public int[] HealthMaxHealth = new int[2];
        public List<Game_Item> Items = new List<Game_Item>();
        public int LastZoneID;
        public int Money;
        public string Name;
        public int PawnState;
        public float[] PhysiqueMoraleConcentration = new float[3];
        public Vector3 Position;
        public Vector3 Rotation;
        public string SerializedSkillDeck = "0#";

        public List<DBSkill> Skills = new List<DBSkill>();

        public List<Game_Item> GetEquipmentList()
        {
            var equip = new List<Game_Item>();
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].LocationType == EItemLocationType.ILT_Equipment)
                {
                    equip.Add(Items[i]);
                }
            }
            return equip;
        }

        //Valshaaran : current/completed quests list database property
        public List<DBQuestTarget> QuestTargets = new List<DBQuestTarget>();

        
    }

}
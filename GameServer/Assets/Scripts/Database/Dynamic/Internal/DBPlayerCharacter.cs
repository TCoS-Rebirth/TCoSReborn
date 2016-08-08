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
        public DBAppearance Appearance;
        public int ArcheType;
        public int DBID;
        public int[] ExtraBodyMindFocusAttributePoints = new int[4];
        public int Faction;
        public int[] FamePep = new int[2];
        public int Health;
        public List<Game_Item> Items = new List<Game_Item>();
        public int LastZoneID;
        public int Money;
        public string Name;
        public int PawnState;
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

        //Persistent variables
        public List<DBPersistentVar> PersistentVars = new List<DBPersistentVar>();

        public class DBAppearance
        {
            public readonly int Race;
            public readonly int Gender;
            public readonly int Body;
            public readonly int Head;
            public readonly int BodyColor;
            public readonly int TattooChest;
            public readonly int TattooLeft;
            public readonly int TattooRight;
            public readonly int Hair;
            public readonly int HairColor;
            public readonly int Voice;

            public readonly int AppearanceCachePart1;
            public readonly int AppearanceCachePart2;

            public DBAppearance(int race, int gender, int body, int head, int bodyColor, int chestTattoo, int leftTattoo, int rightTattoo, int hair, int hairColor, int voice)
            {
                Race = race;
                Gender = gender;
                Body = body;
                Head = head;
                BodyColor = bodyColor;
                TattooChest = chestTattoo;
                TattooLeft = leftTattoo;
                TattooRight = rightTattoo;
                Hair = hair;
                HairColor = hairColor;
                Voice = voice;

                var a1 = 0;
                a1 = a1 | Race;
                a1 = a1 | (Gender << 1);
                a1 = a1 | (Body << 2);
                a1 = a1 | (Head << 4);
                a1 = a1 | (0 << 10);
                a1 = a1 | (BodyColor << 11);
                a1 = a1 | (TattooChest << 19);
                a1 = a1 | (TattooLeft << 23);
                a1 = a1 | (TattooRight << 27);
                a1 = a1 | ((0 & 1) << 31); //unused tattoo
                AppearanceCachePart1 = a1;

                var a2 = 0;
                a2 = a2 | (0 >> 1); //unused tattoo
                a2 = a2 | (HairColor << 3);
                a2 = a2 | (Voice << 20);
                a2 = a2 | (Hair << 23);
                AppearanceCachePart2 = a2;
            }

            public DBAppearance(int[] vals)
            {
                Race = vals[0];
                Gender = vals[1];
                Body = vals[2];
                Head = vals[3];
                BodyColor = vals[4];
                TattooChest = vals[5];
                TattooLeft = vals[6];
                TattooRight = vals[7];
                Hair = vals[8];
                HairColor = vals[9];
                Voice = vals[10];

                var a1 = 0;
                a1 = a1 | Race;
                a1 = a1 | (Gender << 1);
                a1 = a1 | (Body << 2);
                a1 = a1 | (Head << 4);
                a1 = a1 | (0 << 10);
                a1 = a1 | (BodyColor << 11);
                a1 = a1 | (TattooChest << 19);
                a1 = a1 | (TattooLeft << 23);
                a1 = a1 | (TattooRight << 27);
                a1 = a1 | ((0 & 1) << 31); //unused tattoo
                AppearanceCachePart1 = a1;

                var a2 = 0;
                a2 = a2 | (0 >> 1); //unused tattoo
                a2 = a2 | (HairColor << 3);
                a2 = a2 | (Voice << 20);
                a2 = a2 | (Hair << 23);
                AppearanceCachePart2 = a2;
            }
        }
    }

}
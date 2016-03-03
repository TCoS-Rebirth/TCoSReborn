using System;
using System.Collections.Generic;
using Common;
using Database.Dynamic;
using Database.Dynamic.Internal;
using Database.Static;
using Gameplay;
using Network;
using UnityEngine;

namespace World
{
    public static class CharacterCreationSelection
    {
        /// <summary>
        ///     Returns a list of all available characters associated with the given account
        /// </summary>
        public static List<DBPlayerCharacter> GetAccountCharacters(UserAccount acc)
        {
            return MysqlDb.CharacterDB.GetAccountCharacters(acc.UID);
        }

        /// <summary>
        ///     Checks the available characters against the maximum allowed count defined in the GameSettings
        /// </summary>
        public static bool CanCreateNewCharacter(UserAccount acc)
        {
            var numExistingCharacters = MysqlDb.CharacterDB.GetAccountCharacters(acc.UID).Count;
            return numExistingCharacters < GameConfiguration.Get.player.MaxCharactersPerWorld;
        }

        /// <summary>
        ///     Returns the character of the given account associated with the characterID or null if not found
        /// </summary>
        public static DBPlayerCharacter GetAccountCharacter(UserAccount acc, int characterID)
        {
            return MysqlDb.CharacterDB.GetAccountCharacter(characterID, acc.UID);
        }

        /// <summary>
        ///     Tries to delete a character from the given account by characterID
        /// </summary>
        public static bool DeleteAccountCharacter(UserAccount acc, int characterID)
        {
            return MysqlDb.CharacterDB.DeleteCharacter(characterID, acc);
        }

        /// <summary>
        ///     Reads the client data from the message, creates a DBPlayercharacter and tries to save it to the database. Returns
        ///     it if successful
        /// </summary>
        public static DBPlayerCharacter CreateNewCharacter(Message m)
        {
            var newCharacter = ReadPacketCreateDBCharacter(m);
            if (MysqlDb.CharacterDB.CreateNewCharacter(newCharacter.AccountID, newCharacter))
            {
                return newCharacter;
            }
            return null;
        }

        static DBPlayerCharacter ReadPacketCreateDBCharacter(Message m)
        {
            var lod0 = m.ReadByteArray();
            var lod1 = m.ReadByteArray();
            var lod2 = m.ReadByteArray();
            var lod3 = m.ReadByteArray();
            var name = m.ReadString();
            var archeType = (ClassArcheType) m.ReadInt32();
            var fixedSkill1 = new DBSkill(m.ReadInt32(), 0);
            var fixedSkill2 = new DBSkill(m.ReadInt32(), 0);
            var fixedSkill3 = new DBSkill(m.ReadInt32(), 0);
            var chosenSkill1 = new DBSkill(m.ReadInt32(), 0);
            var chosenSkill2 = new DBSkill(m.ReadInt32(), 0);
            m.ReadInt32(); //int unknownShield = 
            var newCharacter = new DBPlayerCharacter
            {
                Name = name,
                ArcheType = (int) archeType
            };

            newCharacter.Skills.Add(fixedSkill1);
            newCharacter.Skills.Add(fixedSkill2);
            newCharacter.Skills.Add(fixedSkill3);
            newCharacter.Skills.Add(chosenSkill1);
            newCharacter.Skills.Add(chosenSkill2);

            newCharacter.SerializedSkillDeck = string.Format("2#{0},{1},{2},0,0|{3},{4},0,0,0", fixedSkill1.ResourceId, fixedSkill2.ResourceId, fixedSkill3.ResourceId,
                chosenSkill1.ResourceId, chosenSkill2.ResourceId);

            //Lods parsing
            Array.Reverse(lod0);
            Array.Reverse(lod1);
            Array.Reverse(lod2);
            Array.Reverse(lod3);

            //lod0
            int rightGauntletColour1 = lod0[5];
            int rightGauntletColour2 = lod0[6];
            int leftGauntletColour1 = lod0[7];
            int leftGauntletColour2 = lod0[8];
            int rightGloveColour1 = lod0[9];
            int rightGloveColour2 = lod0[10];
            int leftGloveColour1 = lod0[11];
            int leftGloveColour2 = lod0[12];
            //Lod0 rest
            int voiceId = lod0[0]; //
            int rightArmTattooId = (byte) (lod0[3] & 0x0F);
            int chestTattooId = (byte) (lod0[4] & 0x0F);
            int leftArmTattooId = (byte) ((lod0[4] & 0xF0) >> 4);

            //Lod1
            int shinRightColour1 = lod1[0];
            int shinRightColour2 = lod1[1];
            int shinLeftColour1 = lod1[2];
            int shinLeftColour2 = lod1[3];
            int thighRightColour1 = lod1[4];
            int thighRightColour2 = lod1[5];
            int thighLeftColour1 = lod1[6];
            int thighLeftColour2 = lod1[7];
            int beltColour1 = lod1[8];
            int beltColour2 = lod1[9];
            int rightShoulderColour1 = lod1[10];
            int rightShoudlerColour2 = lod1[11];
            int leftShoulderColour1 = lod1[12];
            int leftShoulderColour2 = lod1[13];
            int helmetColour1 = lod1[14];
            int helmetColour2 = lod1[15];
            int shoesColour1 = lod1[16];
            int shoesColour2 = lod1[17];
            int pantsColour1 = lod1[18];
            int pantsColour2 = lod1[19];

            //Lod2
            int rangedWeaponId = (byte) (((lod2[1] & 0x0F) << 2) | ((lod2[2] & 0xC0) >> 6));
            int shieldId = (byte) (((lod2[2] & 0x3F) << 2) | ((lod2[3] & 0xC0) >> 6));
            int meleeWeaponId = (byte) (((lod2[3] & 0x3F) << 2) | ((lod2[4] & 0xC0) >> 6));
            int shinRightId = (byte) (lod2[4] & 0x3F);
            int shinLeftId = (byte) ((lod2[5] & 0xFC) >> 2);
            int thighRightId = (byte) (((lod2[5] & 0x03) << 4) | ((lod2[6] & 0xF0) >> 4));
            int thighLeftId = (byte) (((lod2[6] & 0x0F) << 2) | ((lod2[7] & 0xC0) >> 6));
            int beltId = (byte) (lod2[7] & 0x3F);
            int gauntletRightId = (byte) ((lod2[8] & 0xFC) >> 2);
            int gauntletLeftId = (byte) (((lod2[8] & 0x03) << 4) | ((lod2[9] & 0xF0) >> 4));
            int shoulderRightId = (byte) (((lod2[9] & 0x0F) << 2) | ((lod2[10] & 0xC0) >> 6));
            int shoulderLeftId = (byte) (lod2[10] & 0x3F);
            int helmetId = (byte) ((lod2[11] & 0xFC) >> 2);
            int shoesId = (byte) (((lod2[11] & 0x03) << 5) | ((lod2[12] & 0xF8) >> 3));
            int pantsId = (byte) (((lod2[12] & 0x07) << 4) | ((lod2[13] & 0xF0) >> 4));
            int gloveRightId = (byte) (((lod2[13] & 0x0F) << 2) | ((lod2[14] & 0xC0) >> 6));
            int gloveLeftId = (byte) (lod2[14] & 0x3F);

            //Lod3
            int chestArmourColour1 = (byte) (((lod3[0] & 0xFE) << 1) | (lod3[1] >> 7));
            int chestArmourColour2 = (byte) (((lod3[1] & 0x7F) << 1) | (lod3[2] >> 7));
            int chestArmourId = (byte) ((lod3[2] & 0x7E) >> 1);
            int torsoColour1 = (byte) ((lod3[2] << 7) | (lod3[3] >> 1));
            int torsoColour2 = (byte) ((lod3[3] << 7) | (lod3[4] >> 1));
            int torsoId = (byte) ((lod3[4] << 7) | (lod3[5] >> 1));
            int hairColour = (byte) ((lod3[5] << 7) | (lod3[6] >> 1));
            int hairStyleId = (byte) (((lod3[6] & 0x01) << 5) | (lod3[7] >> 3));
            int bodyColour = (byte) ((lod3[7] << 5) | (lod3[8] >> 3));
            int headTypeId = (byte) (((lod3[8] & 0x07) << 4) | ((lod3[9] & 0xF0) >> 4));
            int bodyTypeId = (byte) ((lod3[9] & 0x0C) >> 2);
            int genderId = (byte) ((lod3[9] & 0x02) >> 1);
            int raceId = (byte) (lod3[9] & 0x01);

            //Lods end
            newCharacter.Appearance = new CharacterAppearance(raceId, genderId, bodyTypeId, headTypeId, bodyColour, chestTattooId, leftArmTattooId, rightArmTattooId, hairStyleId,
                hairColour, voiceId);

            var it = GameData.Get.itemDB.GetSetItem(torsoId, EquipmentSlot.ES_CHEST);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_CHEST;
                it.Color1 = (byte) torsoColour1;
                it.Color2 = (byte) torsoColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(gloveLeftId, EquipmentSlot.ES_LEFTGLOVE);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_LEFTGLOVE;
                it.Color1 = (byte) leftGloveColour1;
                it.Color2 = (byte) leftGloveColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(gloveRightId, EquipmentSlot.ES_RIGHTGLOVE);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RIGHTGLOVE;
                it.Color1 = (byte) rightGloveColour1;
                it.Color2 = (byte) rightGloveColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(pantsId, EquipmentSlot.ES_PANTS);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_PANTS;
                it.Color1 = (byte) pantsColour1;
                it.Color2 = (byte) pantsColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shoesId, EquipmentSlot.ES_SHOES);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_SHOES;
                it.Color1 = (byte) shoesColour1;
                it.Color2 = (byte) shoesColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(helmetId, EquipmentSlot.ES_HELMET);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_HELMET;
                it.Color1 = (byte) helmetColour1;
                it.Color2 = (byte) helmetColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shoulderLeftId, EquipmentSlot.ES_LEFTSHOULDER);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_LEFTSHOULDER;
                it.Color1 = (byte) leftShoulderColour1;
                it.Color2 = (byte) leftShoulderColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shoulderRightId, EquipmentSlot.ES_RIGHTSHOULDER);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RIGHTSHOULDER;
                it.Color1 = (byte) rightShoulderColour1;
                it.Color2 = (byte) rightShoudlerColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(chestArmourId, EquipmentSlot.ES_CHESTARMOUR);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_CHESTARMOUR;
                it.Color1 = (byte) chestArmourColour1;
                it.Color2 = (byte) chestArmourColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(gauntletLeftId, EquipmentSlot.ES_LEFTGAUNTLET);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_LEFTGAUNTLET;
                it.Color1 = (byte) leftGauntletColour1;
                it.Color2 = (byte) leftGauntletColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(gauntletRightId, EquipmentSlot.ES_RIGHTGAUNTLET);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RIGHTGAUNTLET;
                it.Color1 = (byte) rightGauntletColour1;
                it.Color2 = (byte) rightGauntletColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(beltId, EquipmentSlot.ES_BELT);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_BELT;
                it.Color1 = (byte) beltColour1;
                it.Color2 = (byte) beltColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(thighLeftId, EquipmentSlot.ES_LEFTTHIGH);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_LEFTTHIGH;
                it.Color1 = (byte) thighLeftColour1;
                it.Color2 = (byte) thighLeftColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(thighRightId, EquipmentSlot.ES_RIGHTTHIGH);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RIGHTTHIGH;
                it.Color1 = (byte) thighRightColour1;
                it.Color2 = (byte) thighRightColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shinLeftId, EquipmentSlot.ES_LEFTSHIN);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_LEFTSHIN;
                it.Color1 = (byte) shinLeftColour1;
                it.Color2 = (byte) shinLeftColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shinRightId, EquipmentSlot.ES_RIGHTSHIN);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RIGHTSHIN;
                it.Color1 = (byte) shinRightColour1;
                it.Color2 = (byte) shinRightColour2;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(meleeWeaponId, EquipmentSlot.ES_MELEEWEAPON);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_MELEEWEAPON;
                newCharacter.Items.Add(it);
            }
            it = GameData.Get.itemDB.GetSetItem(shieldId, EquipmentSlot.ES_SHIELD);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_SHIELD;
                newCharacter.Items.Add(it);
            }
            else
            {
                Debug.Log("Shield SetItem not found: " + shieldId);
            }
            it = GameData.Get.itemDB.GetSetItem(rangedWeaponId, EquipmentSlot.ES_RANGEDWEAPON);
            if (it != null)
            {
                it.LocationSlot = (int) EquipmentSlot.ES_RANGEDWEAPON;
                newCharacter.Items.Add(it);
            }
            else
            {
                Debug.Log("RangedWeapon SetItem not found: " + rangedWeaponId);
            }

            foreach (var git in newCharacter.Items)
            {
                git.LocationType = EItemLocationType.ILT_Equipment;
            }
            var conf = GameConfiguration.Get;
            newCharacter.PawnState = (int) EPawnStates.PS_ALIVE;
            newCharacter.Money = conf.player.StartMoney;
            newCharacter.FamePep[0] = GameConfiguration.CharacterDefaults.MinFame;
            newCharacter.FamePep[1] = GameConfiguration.CharacterDefaults.MinPep;
            newCharacter.HealthMaxHealth[0] = conf.player.StartHealth;
            newCharacter.HealthMaxHealth[1] = conf.player.StartHealth;
            newCharacter.BodyMindFocus[0] = conf.player.StartBody;
            newCharacter.BodyMindFocus[1] = conf.player.StartMind;
            newCharacter.BodyMindFocus[2] = conf.player.StartFocus;
            newCharacter.Faction = conf.player.StartFaction.ID;

            newCharacter.AccountID = m.Connection.player.Account.UID;
            var startZone = GameWorld.Instance.GetZone(conf.player.StartZone);
            if (!startZone)
            {
                Debug.LogError("StartZone not found");
                return null;
            }
            var ps = startZone.FindTravelDestination(GameConfiguration.Get.player.PlayerStartDestinationTag);
            if (!ps)
            {
                Debug.LogError("PlayerStart not found");
                return null;
            }
            newCharacter.Position = ps.Position;
            newCharacter.Rotation = ps.Rotation.eulerAngles;
            newCharacter.LastZoneID = (int) startZone.ID;
            return newCharacter;
        }
    }
}
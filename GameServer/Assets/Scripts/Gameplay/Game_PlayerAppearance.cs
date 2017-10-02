using System;
using System.Collections.Generic;
using Common;
using Database.Dynamic.Internal;
using Database.Static;
using Gameplay.Entities;
using UnityEngine;
using Utility;

namespace Gameplay
{
    public class Game_PlayerAppearance : Game_Appearance
    {

        PlayerCharacter Owner;

        byte mHead;
        int mChestClothes;
        byte mLeftGlove;
        byte mRightGlove;
        byte mPants;
        byte mShoes;
        byte mHeadGearArmour;
        byte mLeftShoulderArmour;
        byte mRightShoulderArmour;
        byte mLeftGauntlet;
        byte mRightGauntlet;
        byte mChestArmour;
        byte mBelt;
        byte mLeftThigh;
        byte mRightThigh;
        byte mLeftShin;
        byte mRightShin;
        byte mMainWeapon;
        byte mOffhandWeapon;
        byte mHair;
        byte mMainSheath;
        byte mOffhandSheath;
        byte[] mTattoo = new byte[4];
        byte[] mClassTattoo = new byte[4];
        byte mBodyColor;
        byte[] mChestClothesColors = new byte[2];
        byte[] mLeftGloveColors = new byte[2];
        byte[] mRightGloveColors = new byte[2];
        byte[] mPantsColors = new byte[2];
        byte[] mShoesColors = new byte[2];
        byte[] mHeadGearArmourColors = new byte[2];
        byte[] mLeftShoulderArmourColors = new byte[2];
        byte[] mRightShoulderArmourColors = new byte[2];
        byte[] mLeftGauntletColors = new byte[2];
        byte[] mRightGauntletColors = new byte[2];
        byte[] mChestArmourColors = new byte[2];
        byte[] mBeltColors = new byte[2];
        byte[] mLeftThighColors = new byte[2];
        byte[] mRightThighColors = new byte[2];
        byte[] mLeftShinColors = new byte[2];
        byte[] mRightShinColors = new byte[2];
        byte mHairColor;
        bool mDisplayLogo;

        //public byte HairColor { get { return mHairColor; } }
        //public int BodyType { get { return mBody; } }
        //public byte HairStyle { get { return mHair; } }
        //public byte BodyColor { get { return mBodyColor; } }
        //public byte HeadType { get { return mHead; } }
        //public byte TattooChest { get { return mTattoo[0]; } }
        //public byte TattooLeft { get { return mTattoo[1]; } }
        //public byte TattooRight { get { return mTattoo[2]; } }

        int appearancePart1;
        public int AppearancePart1 { get { return appearancePart1; } }
        int appearancePart2;
        public int AppearancePart2 { get { return appearancePart2; } }

        byte[] lod0;
        byte[] lod1;
        byte[] lod2;
        byte[] lod3;

        public override void Init(Character owner)
        {
            base.Init(owner);
            Owner = owner as PlayerCharacter;
            LoadBaseData(Owner.dbRef.Appearance);
            RepackLODDataAll();
        }

        void LoadBaseData(DBPlayerCharacter.DBAppearance app)
        {
            mRace = app.Race;
            mGender = (NPCGender)app.Gender;
            mBody = app.Body;
            mHead = (byte) app.Head;
            mBodyColor = (byte) app.BodyColor;
            mTattoo[0] = (byte) app.TattooChest;
            mTattoo[1] = (byte) app.TattooLeft;
            mTattoo[2] = (byte) app.TattooRight;
            mHair = (byte) app.Hair;
            mHairColor = (byte) app.HairColor;
            voice = (byte) app.Voice;
            RecalculateAppearanceParts();
        }

        public DBPlayerCharacter.DBAppearance ToDBCache()
        {
            return new DBPlayerCharacter.DBAppearance
            (
                mRace,
                (int)mGender,
                mBody,
                mHead,
                mBodyColor,
                mTattoo[0],
                mTattoo[1],
                mTattoo[2],
                mHair,
                mHairColor,
                voice
            );
        }

        void RepackLODDataAll()
        {
            lod0 = PackLOD(0);
            lod1 = PackLOD(1);
            lod2 = PackLOD(2);
            lod3 = PackLOD(3);
        }

        public byte[] GetPackedLOD(int level)
        {
            switch (level)
            {
                case 0:
                    return lod0;
                case 1:
                    return lod1;
                case 2:
                    return lod2;
                case 3:
                    return lod3;
                default:
                    Debug.LogWarning("requested LOD level too high: " + level);
                    return lod0;
            }
        }

        byte[] PackLOD(int level)
        {
            Debug.Log("TODO use the cached values (and cache them) instead of querying the ItemManager");
            var Items = Owner.Items;
            switch (level)
            {
                case 0:
                    {
                        var lod = new LODHelper(13);
                        lod.Add(Voice, 8);
                        lod.Add(mClassTattoo[3], 4);
                        lod.Add(mClassTattoo[2], 4);
                        lod.Add(mClassTattoo[1], 4);
                        lod.Add(mClassTattoo[0], 4);
                        lod.Add(mTattoo[3], 4); //4th tattoo
                        lod.Add(mTattoo[2], 4);
                        lod.Add(mTattoo[1], 4);
                        lod.Add(mTattoo[0], 4);
                        var it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTGAUNTLET);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //gauntletRight color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //gauntletRight color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTGAUNTLET);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //it color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //gauntletleft color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTGLOVE);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //glovesRight color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //glovesRight color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTGLOVE);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //glovesLeft color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //glovesLeft color2
                        return lod.GetByteArray();
                    }
                case 1:
                    {
                        var lod = new LODHelper(20);
                        var it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTSHIN);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //shinRight color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //shinRight color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTSHIN);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //shinLeft color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //shinLeft color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTTHIGH);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //thighRight color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //thighRight color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTTHIGH);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //thighLeft color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //thighLeft color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_BELT);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //belt color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //belt color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTSHOULDER);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoulderRight color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoulderRight color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTSHOULDER);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoulderLeft color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoulderLeft color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_HELMET);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //helmet color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //helmet color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_SHOES);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoes color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoes color2
                        it = Items.GetEquippedItem(EquipmentSlot.ES_PANTS);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //pants color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //pants color2
                        return lod.GetByteArray();
                    }
                case 2:
                    {
                        var lod = new LODHelper(15);
                        lod.Add(0, 8); //unused - offhandSheath?
                        lod.Add(0, 4); //unused - mainSheath?
                        var it = Items.GetEquippedItem(EquipmentSlot.ES_RANGEDWEAPON);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //ranged weapon id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_SHIELD);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); //shield id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); //melee weapon id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTSHIN);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shinRight id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTSHIN);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shinLeft id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTTHIGH);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //thighRight id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTTHIGH);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //thighLeft id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_BELT);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //belt id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTGAUNTLET);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //gauntletRight id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTGAUNTLET);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //gauntletLeft id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTSHOULDER);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shoulderRight id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTSHOULDER);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shoulderLeft id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_HELMET);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //helmet id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_SHOES);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 7); //shoes id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_PANTS);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 7); //pants id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_RIGHTGLOVE);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //glove right id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_LEFTGLOVE);
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //glove left id
                        return lod.GetByteArray();
                    }
                case 3:
                    {
                        var lod = new LODHelper(10);
                        lod.Add(0, 1); //unused
                        var it = Items.GetEquippedItem(EquipmentSlot.ES_CHESTARMOUR);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //chest armor color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //chest armor color2
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //chest armor id
                        it = Items.GetEquippedItem(EquipmentSlot.ES_CHEST);
                        lod.Add(it != null ? (int)it.Color1 : 0, 8); //chest color1
                        lod.Add(it != null ? (int)it.Color2 : 0, 8); //chest color2
                        lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); // chest cloth id
                        lod.Add(mHairColor, 8);
                        lod.Add(mHair, 6);
                        lod.Add(mBodyColor, 8);
                        lod.Add(mDisplayLogo?1:0, 1); //displayLogo
                        lod.Add(mHead, 6);
                        lod.Add(mBody, 2);
                        lod.Add((byte)GetGender(), 1);
                        lod.Add(Race, 1);
                        return lod.GetByteArray();
                    }
            }
            return null;
        }

        void RecalculateAppearanceParts()
        {
            var a1 = 0;
            a1 = a1 | mRace;
            a1 = a1 | ((int)mGender << 1);
            a1 = a1 | (mBody << 2);
            a1 = a1 | (mHead << 4);
            a1 = a1 | (0 << 10);
            a1 = a1 | (mBodyColor << 11);
            a1 = a1 | (mTattoo[0] << 19);
            a1 = a1 | (mTattoo[1] << 23);
            a1 = a1 | (mTattoo[2] << 27);
            a1 = a1 | ((0 & 1) << 31); //unused tattoo
            appearancePart1 = a1;

            var a2 = 0;
            a2 = a2 | (0 >> 1); //unused tattoo
            a2 = a2 | (mHairColor << 3);
            a2 = a2 | (voice << 20);
            a2 = a2 | (mHair << 23);
            appearancePart2 = a2;
        }

        public void sv2Rel_SetValue(AppearancePart part, int newValue, byte index = 0)
        {
            switch (part)
            {
                case AppearancePart.AP_ChestClothes:
                    mChestClothes = (byte) newValue;
                    break;
                case AppearancePart.AP_LeftGlove:
                    mLeftGlove = (byte)newValue;
                    break;
                case AppearancePart.AP_RightGlove:
                    mRightGlove = (byte)newValue;
                    break;
                case AppearancePart.AP_Pants:
                    mPants = (byte)newValue;
                    break;
                case AppearancePart.AP_Shoes:
                    mShoes = (byte)newValue;
                    break;
                case AppearancePart.AP_HeadGearArmour:
                    mHeadGearArmour = (byte)newValue;
                    break;
                case AppearancePart.AP_LeftShoulderArmour:
                    mLeftShoulderArmour = (byte)newValue;
                    break;
                case AppearancePart.AP_RightShoulderArmour:
                    mRightShoulderArmour = (byte)newValue;
                    break;
                case AppearancePart.AP_LeftGauntlet:
                    mLeftGauntlet = (byte)newValue;
                    break;
                case AppearancePart.AP_RightGauntlet:
                    mRightGauntlet = (byte)newValue;
                    break;
                case AppearancePart.AP_ChestArmour:
                    mChestArmour = (byte)newValue;
                    break;
                case AppearancePart.AP_Belt:
                    mBelt = (byte)newValue;
                    break;
                case AppearancePart.AP_LeftThigh:
                    mLeftThigh = (byte)newValue;
                    break;
                case AppearancePart.AP_RightThigh:
                    mRightThigh = (byte)newValue;
                    break;
                case AppearancePart.AP_LeftShin:
                    mLeftShin = (byte)newValue;
                    break;
                case AppearancePart.AP_RightShin:
                    mRightShin = (byte)newValue;
                    break;
                case AppearancePart.AP_MainWeapon:
                    mMainWeapon = (byte)newValue;
                    break;
                case AppearancePart.AP_OffhandWeapon:
                    mOffhandWeapon = (byte)newValue;
                    break;
                case AppearancePart.AP_Hair:
                    mHair = (byte)newValue;
                    break;
                case AppearancePart.AP_MainSheath:
                    mMainSheath = (byte)newValue;
                    break;
                case AppearancePart.AP_OffhandSheath:
                    mOffhandSheath = (byte)newValue;
                    break;
                case AppearancePart.AP_Body:
                    mBody = (byte)newValue;
                    break;
                case AppearancePart.AP_Head:
                    mHead = (byte)newValue;
                    break;
                case AppearancePart.AP_Tattoo:
                    mTattoo[index] = (byte)newValue;
                    break;
                case AppearancePart.AP_ClassTattoo:
                    mClassTattoo[index] = (byte)newValue;
                    break;
                default:
                    Debug.LogWarning("Error setting appearance value for part: " + part);
                    break;
            }
            RecalculateAppearanceParts();
        }

        public void sv2rel_SetColorValue(AppearancePart aPart, byte aNewValue, byte aIndex = 0)
        {
            switch (aPart)
            {
                case AppearancePart.AP_ChestClothes:
                    mChestClothesColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_LeftGlove:
                    mLeftGloveColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_RightGlove:
                    mRightGloveColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_Pants:
                    mPantsColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_Shoes:
                    mShoesColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_HeadGearArmour:
                    mHeadGearArmourColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_LeftShoulderArmour:
                    mLeftShoulderArmourColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_RightShoulderArmour:
                    mRightShoulderArmourColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_LeftGauntlet:
                    mLeftGauntletColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_RightGauntlet:
                    mRightGauntletColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_ChestArmour:
                    mChestArmourColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_Belt:
                    mBeltColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_LeftThigh:
                    mLeftShinColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_RightThigh:
                    mRightThighColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_LeftShin:
                    mLeftShinColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_RightShin:
                    mRightShinColors[aIndex] = aNewValue;
                    break;
                case AppearancePart.AP_Hair:
                    mHairColor = aNewValue;
                    break;
                case AppearancePart.AP_Body:
                    mBodyColor = aNewValue;
                    break;
                default:
                    Debug.LogWarning("Error setting color value for part: " + aPart);
                    break;
            }
            RecalculateAppearanceParts();
        }
    }
}
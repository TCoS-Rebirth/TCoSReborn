using Common;
using Database.Static;
using Network;
using UnityEngine;

namespace Gameplay.Entities
{
    public class Game_PlayerStats : Game_CharacterStats
    {

        PlayerCharacter Owner;

        int famePoints;
        int pepPoints;
        byte remainingAttributePoints;
        bool mayChoseClass;

        public int FamePoints
        {
            get { return famePoints; }
        }     

        public int PepPoints
        {
            get { return pepPoints; }
        }    

        public byte RemainingAttributePoints
        {
            get { return remainingAttributePoints; }
        }

        public bool MayChoseClass
        {
            get { return mayChoseClass; }
        }

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as PlayerCharacter;
            Debug.Log("TODO Calculate Body Mind and Focus + assigned points");
            remainingAttributePoints = (byte)Owner.dbRef.ExtraBodyMindFocusAttributePoints[3];
            famePoints = Owner.dbRef.FamePep[0];
            pepPoints = Owner.dbRef.FamePep[1];
            var levelData = LevelProgression.Get.GetLevelbyFamepoints(famePoints);
            mBaseMaxHealth = levelData.level*100 + 100; //Test
            mRecord.MaxHealth = mBaseMaxHealth;
            mHealth = Owner.dbRef.Health;
            mRecord.CopyHealth = mHealth;
            mRecord.FameLevel = levelData.level;
            var pepData = LevelProgression.Get.GetPepLevelByPePpoints(pepPoints);
            mRecord.PePRank = pepData.Level;
        }

        public override void GiveFame(int amount)
        {
            famePoints += amount;
            var mUpdateFamePoints = PacketCreator.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS(Owner);
            Owner.SendToClient(mUpdateFamePoints);

            Owner.ReceiveChatMessage("", "Gained " + amount + " fame", EGameChatRanges.GCR_SYSTEM);

            //Return if max level
            if (GetFameLevel() >= GameConfiguration.CharacterDefaults.MaxFame) return;

            //Check for levelup
            var nextLevelData = GameData.Get.levelProg.GetDataForLevel(GetFameLevel() + 1);
            while (FamePoints >= nextLevelData.requiredFamePoints)
            {
                mRecord.FameLevel += 1;
                nextLevelData = GameData.Get.levelProg.GetDataForLevel(GetFameLevel() + 1);
            }
        }


        protected override void OnFameChanged()
        {
            //TODO: grant attribute points, update stats, etc.
            Owner.SendToClient(PacketCreator.S2R_GAME_PLAYERSTATS_SV2CLREL_ONLEVELUP(Owner));
        }

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();
            Owner.SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(Owner));
        }

        protected override void OnMaxHealthChanged()
        {
            base.OnMaxHealthChanged();
            Owner.SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(Owner));
        }

        protected override void OnPhysiqueChanged()
        {
            base.OnPhysiqueChanged();
            Owner.SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(Owner));
        }

        protected override void OnMoraleChanged()
        {
            base.OnMoraleChanged();
            Owner.SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(Owner));
        }

        protected override void OnConcentrationChanged()
        {
            base.OnConcentrationChanged();
            Owner.SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(Owner));
        }
    }
}
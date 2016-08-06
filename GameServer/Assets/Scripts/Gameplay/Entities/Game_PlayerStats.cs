using Common;
using Database.Static;
using Network;

namespace Gameplay.Entities
{
    public class Game_PlayerStats : Game_CharacterStats
    {

        PlayerCharacter Owner;

        int famePoints;

        public int FamePoints
        {
            get { return famePoints; }
            set { famePoints = value; }
        }

        int pepPoints;

        public int PepPoints
        {
            get { return pepPoints; }
            set { pepPoints = value; }
        }

        byte remainingAttributePoints;

        public byte RemainingAttributePoints
        {
            get { return remainingAttributePoints; }
            set { remainingAttributePoints = value; }
        }

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as PlayerCharacter;
            Body = Owner.dbRef.BodyMindFocus[0];
            Mind = Owner.dbRef.BodyMindFocus[1];
            Focus = Owner.dbRef.BodyMindFocus[2];
            ExtraBodyPoints = (byte)Owner.dbRef.ExtraBodyMindFocusAttributePoints[0];
            //{ can be left out too (calculate from body,mind,focus + levelprogression->leftover-points)
            ExtraMindPoints = (byte)Owner.dbRef.ExtraBodyMindFocusAttributePoints[1];
            ExtraFocusPoints = (byte)Owner.dbRef.ExtraBodyMindFocusAttributePoints[2];
            FameLevel = Owner.dbRef.FamePep[0];
            PepRank = Owner.dbRef.FamePep[1];
            FamePoints = Owner.dbRef.FamePoints;
            MaxHealth = Owner.dbRef.HealthMaxHealth[1]; //TODO: calculate value instead, later (from level & items etc)
            Health = Owner.dbRef.HealthMaxHealth[0];
        }

        public override void GiveFame(int amount)
        {
            FamePoints += amount;
            var mUpdateFamePoints = PacketCreator.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS(Owner);
            Owner.SendToClient(mUpdateFamePoints);

            Owner.ReceiveChatMessage("", "Gained " + amount + " fame", EGameChatRanges.GCR_SYSTEM);

            //Return if max level
            if (FameLevel >= GameConfiguration.CharacterDefaults.MaxFame) return;

            //Check for levelup
            var nextLevelData = GameData.Get.levelProg.GetDataForLevel(FameLevel + 1);
            while (FamePoints >= nextLevelData.requiredFamePoints)
            {
                SetFame(FameLevel + 1);
                nextLevelData = GameData.Get.levelProg.GetDataForLevel(FameLevel + 1);
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
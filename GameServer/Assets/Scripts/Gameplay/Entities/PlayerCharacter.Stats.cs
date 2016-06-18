using Common;
using Network;

namespace Gameplay.Entities
{
    public sealed partial class PlayerCharacter
    {

        /// <summary>
        ///     <see cref="Character.SetPawnState" />
        /// </summary>
        public override void SetPawnState(EPawnStates newState)
        {
            base.SetPawnState(newState);
            SendToClient(PacketCreator.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(this));
        }

        /// <summary>
        ///     Notifies the client through combat message log, Sheates the weapon and sets the combatstate to idle TODO cleanup
        /// </summary>
        /// <param name="source"></param>
        protected override void OnDiedThroughDamage(Character source)
        {
            base.OnDiedThroughDamage(source);
            CombatMode = ECombatMode.CBM_Idle;
            SheatheWeapon();
            SendToClient(PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH(source));
        }

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(this));
        }

        protected override void OnMaxHealthChanged()
        {
            base.OnMaxHealthChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(this));
        }

        protected override void OnPhysiqueChanged()
        {
            base.OnPhysiqueChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(this));
        }

        protected override void OnMoraleChanged()
        {
            base.OnMoraleChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(this));
        }

        protected override void OnConcentrationChanged()
        {
            base.OnConcentrationChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(this));
        }

        //protected override void OnMovementSpeedBonusChanged()
        //{
        //    base.OnMovementSpeedBonusChanged();
        //    SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(this));
        //}

    }
}

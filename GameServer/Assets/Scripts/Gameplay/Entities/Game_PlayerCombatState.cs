using System;
using Common;
using Network;

namespace Gameplay.Entities
{
    public class Game_PlayerCombatState : Game_CombatState
    {

        PlayerCharacter Owner;

        public override void Init(Character owner)
        {
            base.Init(owner);
            Owner = owner as PlayerCharacter;
            mMainWeapon = Owner.Items.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON).Type.resourceID;
            var offH = Owner.Items.GetEquippedItem(EquipmentSlot.ES_SHIELD);
            if (offH != null) mOffhandWeapon = offH.Type.resourceID;
        }

        public override bool sv_DrawWeapon(ECombatMode aInitialMode)
        {
            if (base.sv_DrawWeapon(aInitialMode))
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON(Owner));
                return true;
            }
            return false;
        }

        public override bool sv_SheatheWeapon()
        {
            if (base.sv_SheatheWeapon())
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON());
                return true;
            }
            return false;
        }

        public override bool SwitchToWeaponType(EAppMainWeaponType aWeaponType)
        {
            if (base.SwitchToWeaponType(aWeaponType))
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(Owner));
                return true;
            }
            return false;
        }
    }
}
using System;
using Common;
using Gameplay.Items;
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

        public void cl2sv_DrawSheatheWeapon()
        {
            //added
            var aMode = ECombatMode.CBM_Melee;
            var lastSkill = Owner.Skills.GetActiveTierSlotSkill(Owner.Skills.LastActiveSkillIndex);
            if (lastSkill != null)
            {
                switch (lastSkill.requiredWeapon)
                {
                    case EWeaponCategory.EWC_Melee:
                        aMode = ECombatMode.CBM_Melee;
                        break;
                    case EWeaponCategory.EWC_Ranged:
                        aMode = ECombatMode.CBM_Ranged;
                        break;
                    default:
                        aMode = ECombatMode.CBM_Cast;
                        break;
                }
            }
            //added end

            //if (Outer.mPvPSettings != null
            //  && !Outer.mPvPSettings.AllowDrawWeapon)
            //{
            //    sv_SheatheWeapon();   
            //}
            if (mCombatMode == ECombatMode.CBM_Idle)
            {
                if (sv_DrawWeapon(aMode))
                {
                    return;
                }
                if (aMode != ECombatMode.CBM_Melee && sv_DrawWeapon(ECombatMode.CBM_Melee))
                {
                    return;
                }
                if (aMode != ECombatMode.CBM_Ranged && sv_DrawWeapon(ECombatMode.CBM_Ranged))
                {
                    return;
                }
                if (aMode != ECombatMode.CBM_Cast)
                {
                    sv_DrawWeapon(ECombatMode.CBM_Cast);
                }
            }
            else
            {
                sv_SheatheWeapon();
            }
        }

        public void sv2cl_SetWeapon(ECombatMode aMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            GetMainWeapon().OnSheathe(Owner);
            GetOffhandWeapon().OnSheathe(Owner);
            ResolveWeapons(aMode, out newMainWeapon, out newOffhandWeapon);
            mMainWeapon = newMainWeapon != null ? newMainWeapon.resourceID : 0;
            mOffhandWeapon = newOffhandWeapon != null ? newOffhandWeapon.resourceID : 0;
            mWeaponFlag = ResolveWeaponFlag(aMode, newMainWeapon, newOffhandWeapon);
            if (aMode != ECombatMode.CBM_Cast)
            {
                if (newMainWeapon != null)
                {
                    newMainWeapon.OnDraw(Owner);
                }
                if (newOffhandWeapon != null)
                {
                    newOffhandWeapon.OnDraw(Owner);
                }
            }
            else
            {
                GetMainWeapon().OnSheathe(Owner);
                GetOffhandWeapon().OnSheathe(Owner);
                mMainWeapon = 0;
                mOffhandWeapon = 0;
            }
            //Owner.ClearAnimsByType(1, 0.30000001);
            mCombatMode = aMode;
        }

        public void cl2sv_SwitchWeaponType(EWeaponCategory aWeaponType)
        {
            sv_SwitchToWeaponType(aWeaponType);
        }

        public override bool sv_DrawWeapon(ECombatMode aInitialMode)
        {
            if (base.sv_DrawWeapon(aInitialMode))
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON(aInitialMode));
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

        protected override bool sv_SwitchToMode(ECombatMode aMode)
        {
            if (aMode == mCombatMode) return true;
            if (!base.sv_SwitchToMode(aMode)) return false;
            Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(aMode));
            return true;
        }
    }
}
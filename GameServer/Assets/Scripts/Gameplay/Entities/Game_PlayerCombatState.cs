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

        public void cl2sv_DrawSheatheWeapon()
        {
            ECombatMode aMode = ECombatMode.CBM_Melee;
            var lastSkill = Owner.Skills.GetActiveTierSlotSkill(Owner.Skills.LastActiveSkillIndex);
            if (lastSkill != null)
            {
                switch (lastSkill.requiredWeapon)
                {            
                    case (EWeaponCategory)1:                                                           
                        aMode = (ECombatMode)1;                                                   
                        break;                                                          
                    case (EWeaponCategory)2:                                                           
                        aMode = (ECombatMode)2;                                                 
                        break;                                                             
                    default:                                                            
                        aMode = (ECombatMode)3;                                              
                        break;    
                }
            }   
            //if (Outer.mPvPSettings != None
            //  && !Outer.mPvPSettings.AllowDrawWeapon)
            //{
            //    sv_SheatheWeapon();   
            //}
            if (mCombatMode == (ECombatMode)0)
            {
                if (sv_DrawWeapon(aMode))
                {
                    return;
                }
                if (aMode != (ECombatMode)1 && sv_DrawWeapon((ECombatMode)1))
                {
                    return;
                }
                if (aMode != (ECombatMode)2 && sv_DrawWeapon((ECombatMode)2))
                {
                    return;
                }
                if (aMode != (ECombatMode)3 && sv_DrawWeapon((ECombatMode)3))
                {
                    return;
                }
            }
            else
            {
                sv_SheatheWeapon();
            }
        }

        public override bool sv_DrawWeapon(ECombatMode aInitialMode)
        {
            if (base.sv_DrawWeapon(aInitialMode))
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON((int)GetWeaponFlag()));
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

        public override bool sv_SwitchToWeaponType(EWeaponCategory aWeaponType)
        {
            if (base.sv_SwitchToWeaponType(aWeaponType))
            {
                Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON((int)aWeaponType));
                return true;
            }
            return false;
        }
    }
}
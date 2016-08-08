using Common;
using Database.Static;
using Gameplay.Items;
using Network;
using UnityEngine;

namespace Gameplay.Entities
{
    public class Game_CombatState : ScriptableObject
    {

        Character Owner;

        protected ECombatMode mCombatMode = ECombatMode.CBM_Idle;
        public ECombatMode CombatMode { get { return mCombatMode; } }
        protected int mMainWeapon;
        protected int mOffhandWeapon;
        //bool mDrawing;
        //bool mReDraw;
        //bool mSheathing;
        //bool mReSheathe;
        //byte mReDrawMode;
        //int mReDrawNewMainWeapon;
        //int mReDrawNewOffhandWeapon;
        //float mDrawSheatheTimer;
        //float cDrawSheatheTime;
        protected SBAnimWeaponFlags mWeaponFlag;
        //bool mExecutingBodySlotSkill;
        bool mPreparedBonusGiven;

        public virtual void Init(Character owner)
        {
            Owner = owner;
        }

        public int MainWeapon { get { return mMainWeapon; } }
        public int OffhandWeapon { get { return mOffhandWeapon; } }

        protected Item_Type GetOffhandWeapon()
        {
            return mOffhandWeapon != 0 ? GameData.Get.itemDB.GetItemType(mOffhandWeapon) : null;
        }


        protected Item_Type GetMainWeapon()
        {
            return mMainWeapon != 0 ? GameData.Get.itemDB.GetItemType(mMainWeapon) : null;
        }

        void RemovePreparedBonusConditional()
        {
            if (!mPreparedBonusGiven) return;
            mPreparedBonusGiven = false;                                       
            Owner.Stats.IncreaseMeleeResistanceDelta(-0.05f);           
            Owner.Stats.IncreaseRangedResistanceDelta(-0.05f);          
            Owner.Stats.IncreaseMagicResistanceDelta(-0.05f);
        }


        void GivePreparedBonusConditional()
        {
            if (mPreparedBonusGiven) return;
            mPreparedBonusGiven = true;                                             
            Owner.Stats.IncreaseMeleeResistanceDelta(0.05f);        
            Owner.Stats.IncreaseRangedResistanceDelta(0.05f);           
            Owner.Stats.IncreaseMagicResistanceDelta(0.05f);
        }

        public SBAnimWeaponFlags ResolveWeaponFlag(ECombatMode aMode, Item_Type aMainWeapon, Item_Type aOffhandWeapon)
        {
            switch (aMode)
            {
                case 0:
                    return SBAnimWeaponFlags.AnimWeapon_None;
                case (ECombatMode)1:
                    if (aOffhandWeapon != null)
                    {
                        return aOffhandWeapon.GetWeaponType();
                    }
                    if (aMainWeapon != null)
                    {
                        return aMainWeapon.GetWeaponType();
                    }
                    return (SBAnimWeaponFlags)1;
                case (ECombatMode)2:
                    return aMainWeapon.GetWeaponType();
                case (ECombatMode)3:
                    return (SBAnimWeaponFlags)1;
            }
            if (aMainWeapon != null)
            {
                return aMainWeapon.GetWeaponType();
            }
            return (SBAnimWeaponFlags)1;
        }

        public SBAnimWeaponFlags GetWeaponFlag()
        {
            //if (mExecutingBodySlotSkill)
            //{                                              
            //    return (SBAnimWeaponFlags)1;                                                            
            //}
            return mWeaponFlag;
        }

        public virtual bool sv_SwitchToWeaponType(EWeaponCategory aWeaponType)
        {
            Debug.Log("Switching weapon request: " + aWeaponType);
            switch (aWeaponType)
            {
                default:                                         
                case 0:                                                                 
                    return true;                                                         
                case (EWeaponCategory)1:                                                                 
                    return SwitchToMode((ECombatMode)1);                                          
                case (EWeaponCategory)2:                                                                 
                    return SwitchToMode((ECombatMode)2);                                             
                case (EWeaponCategory)3:                                                                 
                    return SwitchToMode((ECombatMode)3);                                             
                case (EWeaponCategory)4:                                                        
                    if (mCombatMode != (ECombatMode)1 && mCombatMode != (ECombatMode)3)
                    {                            
                        return SwitchToMode((ECombatMode)3);                                       
                    }
                    return true;
            }
        }

        void ResolveWeapons(ECombatMode mode, out Item_Type mainWeapon, out Item_Type offhandWeapon)
        {
            if (mode == ECombatMode.CBM_Melee)
            {
                var w = Owner.Items.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
                var o = Owner.Items.GetEquippedItem(EquipmentSlot.ES_SHIELD);
                if (w != null)
                {
                    mainWeapon = w.Type;
                }
                else
                {
                    mainWeapon = GetMainWeapon();
                }
                if (o != null)
                {
                    offhandWeapon = w.Type;
                }
                else
                {
                    offhandWeapon = GetOffhandWeapon();
                }
                return;
            }
            if (mode == ECombatMode.CBM_Ranged)
            {
                var w = Owner.Items.GetEquippedItem(EquipmentSlot.ES_RANGEDWEAPON);
                if (w != null)
                {
                    mainWeapon = w.Type;
                }
                else
                {
                    mainWeapon = GetMainWeapon();
                }
                offhandWeapon = null;
                return;
            }
            if (mode == ECombatMode.CBM_Cast)
            {
                mainWeapon = null;
                offhandWeapon = null;
                return;
            }
            mainWeapon = GetMainWeapon();
            offhandWeapon = GetOffhandWeapon();
        }

        bool SwitchToMode(ECombatMode aMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            ResolveWeapons(aMode, out newMainWeapon, out newOffhandWeapon);
            if (mCombatMode == aMode) return true;
            if (mCombatMode == 0) return sv_DrawWeapon(aMode);
            if (aMode == 0) return sv_SheatheWeapon();
            if (aMode != (ECombatMode) 3 && newMainWeapon == null) return false;
            if (GetMainWeapon() != null)
            {                                  
                GetMainWeapon().OnSheathe(Owner);                            
            }
            if (GetOffhandWeapon() != null)
            {                                 
                GetOffhandWeapon().OnSheathe(Owner);                          
            }
            mCombatMode = aMode;                                         
            mWeaponFlag = ResolveWeaponFlag(mCombatMode, newMainWeapon, newOffhandWeapon);
            if (newMainWeapon != null)
            {                                       
                newMainWeapon.OnDraw(Owner);                                   
                mMainWeapon = newMainWeapon.resourceID;                   
            }
            else
            {                                                           
                mMainWeapon = 0;                                           
            }
            if (newOffhandWeapon != null)
            {                                    
                newOffhandWeapon.OnDraw(Owner);                                
                mOffhandWeapon = newOffhandWeapon.resourceID;            
            }
            else
            {                                                           
                mOffhandWeapon = 0;                                             
            }
            //sv2rel_DrawWeapon_CallStub(mCombatMode, mMainWeapon, mOffhandWeapon); 
            return true;
        }

       public virtual bool sv_SheatheWeapon()
        {
           if (Owner.Stats.IsMovementFrozen()
              || mCombatMode == 0
              /*|| mSheathing*/)
            {
                return false;                                                       
            }
            //if (mDrawing)
            //{                                                          
            //    mReSheathe = !mReSheathe;                                             
            //    return false;                                                           
            //}
            mCombatMode = 0;                                                           
            //mSheathing = true;                                                       
            //mDrawSheatheTimer = cDrawSheatheTime;                                      
            var oldMainWeapon = GetMainWeapon();                                   
            if (oldMainWeapon != null)
            {                                               
                oldMainWeapon.OnSheathe(Owner);                                         
            }
            var oldOffhandWeapon = GetOffhandWeapon();                                 
            if (oldOffhandWeapon != null)
            {                                            
                oldOffhandWeapon.OnSheathe(Owner);                                  
            }
            mMainWeapon = 0;                                                        
            mOffhandWeapon = 0;                                                       
            mWeaponFlag = ResolveWeaponFlag(mCombatMode, null, null);                    
            //sv2rel_SheatheWeapon_CallStub();                                           
            Owner.Skills.FireCondition(null, (EDuffCondition)3);
            RemovePreparedBonusConditional();
            //Owner.Stats.UnsetStatsState(1);   
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Owner));
            return true; 
        }

        public virtual bool sv_DrawWeapon(ECombatMode aInitialMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            if (Owner.Stats.IsMovementFrozen())
            {                           
                return false;                                                           
            }
            if (mCombatMode != 0 /*|| mDrawing*/)
            {                                      
                return false;                                                        
            }
            //if (mSheathing)
            //{                                                        
            //    return false;                                                   
            //}
            //if (Owner.sv_IsResting())
            //{                                       
            //    Owner.sv_Sit(false);                                                 
            //}
            ResolveWeapons(aInitialMode, out newMainWeapon, out newOffhandWeapon);        
            if (newMainWeapon == null && aInitialMode != (ECombatMode)3)
            {                       
                return false;                                                         
            }
            mCombatMode = aInitialMode;                                                
            var oldMainWeapon = GetMainWeapon();                                  
            if (oldMainWeapon != null)
            {                                             
                oldMainWeapon.OnSheathe(Owner);                                         
            }
            var oldOffhandWeapon = GetOffhandWeapon();                                      
            if (oldOffhandWeapon != null)
            {                                            
                oldOffhandWeapon.OnSheathe(Owner);                                       
            }
            //mDrawing = true;                                                         
            //mDrawSheatheTimer = cDrawSheatheTime;                              
            mWeaponFlag = ResolveWeaponFlag(mCombatMode, newMainWeapon, newOffhandWeapon);
            if (newMainWeapon != null)
            {                                              
                newMainWeapon.OnDraw(Owner);                                       
                mMainWeapon = newMainWeapon.resourceID;                        
            }
            else
            {                                                                   
                mMainWeapon = 0;                                                        
            }
            if (newOffhandWeapon != null)
            {                                           
                newOffhandWeapon.OnDraw(Owner);                                         
                mOffhandWeapon = newOffhandWeapon.resourceID;                  
            }
            else
            {                                                                 
                mOffhandWeapon = 0;                                                     
            }
            //sv2rel_DrawWeapon_CallStub(aInitialMode, mMainWeapon, mOffhandWeapon);    
            //Owner.SetCollision(True, CombatCollision);                               
            Owner.Skills.FireCondition(null, (EDuffCondition)4);
            GivePreparedBonusConditional();
            //Owner.CharacterStats.SetStatsState(1); 
                                              
            //if (Owner.IsShifted())
            //{                                                 
            //    Owner.Unshift();                                                  
            //}
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Owner));
            return true;
        }

    }

}

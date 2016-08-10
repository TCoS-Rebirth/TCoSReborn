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
        bool mDrawing;
        bool mReDraw;
        bool mSheathing;
        bool mReSheathe;
        byte mReDrawMode;
        int mReDrawNewMainWeapon;
        int mReDrawNewOffhandWeapon;
        float mDrawSheatheTimer;
        float cDrawSheatheTime = 1f; //possibly set by weapon/animation
        protected SBAnimWeaponFlags mWeaponFlag;
        bool mExecutingBodySlotSkill;
        bool mPreparedBonusGiven;

        //HACK
        EWeaponCategory equippedWeaponType;

        public EWeaponCategory EquippedWeaponCategory { get { return equippedWeaponType; } }

        public int MainWeapon { get { return mMainWeapon; } }
        public int OffhandWeapon { get { return mOffhandWeapon; } }

        public virtual void Init(Character owner)
        {
            Owner = owner;
        }

        public void cl_OnFrame(float DeltaTime)
        {
            if (mDrawSheatheTimer > 0)
            {                                     
                mDrawSheatheTimer -= DeltaTime;                                  
                if (mDrawSheatheTimer <= 0)
                {                                
                    if (mDrawing)
                    {                                                  
                        mDrawing = false;                                                   
                    }
                    if (mSheathing)
                    {                                                      
                        mSheathing = false;                                               
                    }
                }
            }
        }      

        protected Item_Type GetOffhandWeapon()
        {
            return mOffhandWeapon != 0 ? GameData.Get.itemDB.GetItemType(mOffhandWeapon) : null;
        }

        public Item_Type GetMainWeapon()
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

        public bool IsDrawing()
        {
            return mDrawing;
        }

        public bool IsSheathing()
        {
            return mSheathing; 
        }

        //clientside?
        public void OnDoneSheathing(bool aMainHand)
        {
            if (aMainHand)
            {                                                        
                GetMainWeapon().OnSheathe(Owner);                                         
                mMainWeapon = 0;                                                        
                mSheathing = false;                                                      
            }
            else
            {                                                                    
                GetOffhandWeapon().OnSheathe(Owner);                                   
                mOffhandWeapon = 0;                                                      
            }
            //Owner.ClearAnimsByType(1, 0.00000000);                                     
            if (mReDraw)
            {                                                         
                mReDraw = false;                                                         
                //cl_DrawWeapon(mReDrawMode, mReDrawNewMainWeapon, mReDrawNewOffhandWeapon);  
            }
        }

        //clientside?
        public void OnDoneDrawing(bool aMainHand)
        {
            if (aMainHand)
            {                                          
                GetMainWeapon().OnDraw(Owner);                                          
                mDrawing = false;                                                       
            }
            else
            {                                                            
                GetOffhandWeapon().OnDraw(Owner);                                       
            }
            //Owner.ClearAnimsByType(1, 0.00000000);                                      
            if (mReSheathe)
            {                                                       
                mReSheathe = false;                                               
                //cl_SheatheWeapon();                                                 
            }
        }

        public SBAnimWeaponFlags ResolveWeaponFlag(ECombatMode aMode, Item_Type aMainWeapon, Item_Type aOffhandWeapon)
        {
            switch (aMode)
            {
                case 0:
                    return SBAnimWeaponFlags.AnimWeapon_None;
                case ECombatMode.CBM_Melee:
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
            if (mExecutingBodySlotSkill)
            {
                return (SBAnimWeaponFlags)1;
            }
            return mWeaponFlag;
        }

        public bool sv_SwitchToWeaponType(EWeaponCategory aWeaponType)
        {
            equippedWeaponType = aWeaponType;
            switch (aWeaponType)
            {
                default:                                         
                case EWeaponCategory.EWC_None:                                                                 
                    return true;                                                         
                case EWeaponCategory.EWC_Melee:                                                                 
                    return sv_SwitchToMode(ECombatMode.CBM_Melee);                                          
                case EWeaponCategory.EWC_Ranged:                                                                 
                    return sv_SwitchToMode(ECombatMode.CBM_Ranged);                                             
                case EWeaponCategory.EWC_Unarmed:                                                                 
                    return sv_SwitchToMode(ECombatMode.CBM_Cast);                                             
                case EWeaponCategory.EWC_MeleeOrUnarmed:                                                        
                    if (mCombatMode != ECombatMode.CBM_Melee && mCombatMode != ECombatMode.CBM_Cast)
                    {                            
                        return sv_SwitchToMode(ECombatMode.CBM_Cast);                                       
                    }
                    return true;
            }
        }

        protected void ResolveWeapons(ECombatMode mode, out Item_Type mainWeapon, out Item_Type offhandWeapon)
        {
            var w = Owner.Items.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
            var o = Owner.Items.GetEquippedItem(EquipmentSlot.ES_SHIELD);
            if (mode == ECombatMode.CBM_Ranged)
            {
                w = Owner.Items.GetEquippedItem(EquipmentSlot.ES_RANGEDWEAPON);
            }
            mainWeapon = w != null ? w.Type : GetMainWeapon();
            offhandWeapon = o != null ? o.Type : GetOffhandWeapon();
        }

        protected virtual bool sv_SwitchToMode(ECombatMode aMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            ResolveWeapons(aMode, out newMainWeapon, out newOffhandWeapon);
            if (mCombatMode == aMode) return true;
            if (mCombatMode == ECombatMode.CBM_Idle) return sv_DrawWeapon(aMode);
            if (aMode == ECombatMode.CBM_Idle) return sv_SheatheWeapon();
            if (aMode != ECombatMode.CBM_Cast && newMainWeapon == null) return false;
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
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Owner));
            return true;
        }

       public virtual bool sv_SheatheWeapon()
        {
           if (Owner.Stats.IsMovementFrozen()
              || mCombatMode == 0
              || mSheathing)
            {
                return false;                                                       
            }
            if (mDrawing)
            {
                mReSheathe = !mReSheathe;
                return false;
            }
            mCombatMode = 0;
            mSheathing = true;
            mDrawSheatheTimer = cDrawSheatheTime;
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
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Owner));
            Owner.Skills.FireCondition(null, EDuffCondition.EDC_OnSheatheWeapon);
            RemovePreparedBonusConditional();
            //Owner.Stats.UnsetStatsState(1);   
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
            if (mCombatMode != 0 || mDrawing)
            {                                      
                return false;                                                        
            }
            if (mSheathing)
            {
                return false;
            }
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
            mDrawing = true;
            mDrawSheatheTimer = cDrawSheatheTime;
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
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Owner));
            //Owner.SetCollision(True, CombatCollision);                               
            Owner.Skills.FireCondition(null, EDuffCondition.EDC_OnDrawWeapon);
            GivePreparedBonusConditional();
            //Owner.CharacterStats.SetStatsState(1); 

            //if (Owner.IsShifted())
            //{                                                 
            //    Owner.Unshift();                                                  
            //}
            return true;
        }

    }

}

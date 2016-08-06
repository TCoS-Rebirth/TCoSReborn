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
        float cDrawSheatheTime;
        protected SBAnimWeaponFlags mWeaponFlag;
        bool mExecutingBodySlotSkill;
        bool mPreparedBonusGiven;

        public virtual void Init(Character owner)
        {
            Owner = owner;
        }

        protected Item_Type GetOffhandWeapon()
        {
            if (mOffhandWeapon != 0)
            {
                return GameData.Get.itemDB.GetItemType(mOffhandWeapon);
            }
            return null;
        }


        protected Item_Type GetMainWeapon()
        {
            if (mMainWeapon != 0)
            {
                return GameData.Get.itemDB.GetItemType(mMainWeapon);
            }
            return null;
        }

        //public virtual void SheatheWeapon()
        //{
        //    mCombatMode = 0;
        //    mWeaponFlag = ResolveWeaponFlag(mCombatMode, null, null);
        //    Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Owner));
        //}

        //public void DrawWeapon(ECombatMode aMode, int aNewMainWeapon, int aNewOffhandWeapon)
        //{
        //    var newMainWeapon = aNewMainWeapon != 0 ? GameData.Get.itemDB.GetItemType(aNewMainWeapon) : null;
        //    var newOffhandWeapon = aNewOffhandWeapon != 0 ? GameData.Get.itemDB.GetItemType(aNewOffhandWeapon) : null;
        //    GetMainWeapon().OnSheathe(Owner);                                        
        //    GetOffhandWeapon().OnSheathe(Owner);                                 
        //    mWeaponFlag = ResolveWeaponFlag(aMode, newMainWeapon, newOffhandWeapon);
        //    if (aMode == (ECombatMode) 3 || mCombatMode != 0)
        //    {
        //        if (newMainWeapon != null)
        //        {
        //            newMainWeapon.OnDraw(Owner);
        //        }
        //        if (newOffhandWeapon != null)
        //        {
        //            newOffhandWeapon.OnDraw(Owner);
        //        }
        //    }
        //    mCombatMode = aMode;                                                     
        //    mMainWeapon = newMainWeapon != null ? newMainWeapon.resourceID : 0;
        //    mOffhandWeapon = newOffhandWeapon != null ? newOffhandWeapon.resourceID : 0;
        //    Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Owner));
        //}

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
            if (mExecutingBodySlotSkill)
            {                                              
                return (SBAnimWeaponFlags)1;                                                            
            }
            return mWeaponFlag;
        }

        public virtual bool SwitchToWeaponType(EAppMainWeaponType aWeaponType)
        {
            switch (aWeaponType)
            {
                default:                                         
                case 0:                                                                 
                    return true;                                                         
                case (EAppMainWeaponType)1:                                                                 
                    return SwitchToMode((ECombatMode)1);                                          
                case (EAppMainWeaponType)2:                                                                 
                    return SwitchToMode((ECombatMode)2);                                             
                case (EAppMainWeaponType)3:                                                                 
                    return SwitchToMode((ECombatMode)3);                                             
                case (EAppMainWeaponType)4:                                                        
                    if (mCombatMode != (ECombatMode)1 && mCombatMode != (ECombatMode)3)
                    {                            
                        return SwitchToMode((ECombatMode)3);                                       
                    }
                    return true;
            }
        }

        void ResolveWeapons(ECombatMode mode, out Item_Type mainWeapon, out Item_Type offhandWeapon)
        {
            mainWeapon = GetMainWeapon();
            offhandWeapon = GetOffhandWeapon();
        }

        bool SwitchToMode(ECombatMode aMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            ResolveWeapons(aMode, out newMainWeapon, out newOffhandWeapon);
            if (mCombatMode != aMode)
            {
                if (mCombatMode != 0)
                {
                    if (aMode != 0)
                    {
                        if (aMode == (ECombatMode)3 || newMainWeapon != null)
                        {                          
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
                        return false;
                    }
                    return sv_SheatheWeapon();
                }
                return sv_DrawWeapon(aMode);
            }
            return true;
        }

       public virtual bool sv_SheatheWeapon()
        {
            Item_Type oldMainWeapon;
            Item_Type oldOffhandWeapon;
            if (Owner.Stats.FreezePosition
              || mCombatMode == 0
              || mSheathing)
            {
                return false;                                                       
            }
            //if (mDrawing)
            //{                                                          
            //    mReSheathe = !mReSheathe;                                             
            //    return false;                                                           
            //}
            mCombatMode = 0;                                                           
            mSheathing = true;                                                       
            mDrawSheatheTimer = cDrawSheatheTime;                                      
            oldMainWeapon = GetMainWeapon();                                   
            if (oldMainWeapon != null)
            {                                               
                oldMainWeapon.OnSheathe(Owner);                                         
            }
            oldOffhandWeapon = GetOffhandWeapon();                                 
            if (oldOffhandWeapon != null)
            {                                            
                oldOffhandWeapon.OnSheathe(Owner);                                  
            }
            mMainWeapon = 0;                                                        
            mOffhandWeapon = 0;                                                       
            mWeaponFlag = ResolveWeaponFlag(mCombatMode, null, null);                    
            //sv2rel_SheatheWeapon_CallStub();                                           
            Owner.Skills.FireCondition(null, (EDuffCondition)3);
            //RemovePreparedBonusConditional();                                       
            //Owner.Stats.UnsetStatsState(1);   
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Owner));
            return true; 
        }

        public virtual bool sv_DrawWeapon(ECombatMode aInitialMode)
        {
            Item_Type newMainWeapon;
            Item_Type newOffhandWeapon;
            Item_Type oldMainWeapon;
            Item_Type oldOffhandWeapon;
            if (Owner.Stats.FreezePosition)
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
            oldMainWeapon = GetMainWeapon();                                  
            if (oldMainWeapon != null)
            {                                             
                oldMainWeapon.OnSheathe(Owner);                                         
            }
            oldOffhandWeapon = GetOffhandWeapon();                                      
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
            //Owner.SetCollision(True, CombatCollision);                               
            Owner.Skills.FireCondition(null, (EDuffCondition)4);
            //GivePreparedBonusConditional();                                            
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

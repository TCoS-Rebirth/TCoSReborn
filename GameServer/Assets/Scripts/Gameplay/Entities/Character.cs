using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities.NPCs;
using Gameplay.Items;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using Gameplay.Skills.Events;
using Network;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities
{
    public abstract partial class Character : Entity
    {
        const float MOVESPEED_MULTIPLIER = 1f;

        int _groundSpeed = 200;

        float _groundSpeedModifier = MOVESPEED_MULTIPLIER;

        [SerializeField, ReadOnly] EPawnStates _pawnState = EPawnStates.PS_ALIVE;

        [ReadOnly] EControllerStates _controllerState = EControllerStates.CPS_PAWN_ALIVE;

        EPhysics _physics = EPhysics.PHYS_Walking;

        int _shiftableAppearance;
        public bool IsShifted { get { return ShiftableAppearance > 0; } }

        Vector3 _velocity;

        [SerializeField, ReadOnly] ClassArcheType archeType = ClassArcheType.Warrior;

        /// <summary>
        ///     Used to calculate grounded position if pathfinding
        /// </summary>
        [NonSerialized] public float BodyCenterHeight = 1f;

        [SerializeField, ReadOnly] Taxonomy faction;

        protected Rigidbody PhysicsBody;

        /// <summary>
        ///     The class type of this character
        /// </summary>
        public ClassArcheType ArcheType
        {
            get { return archeType; }
            set { archeType = value; }
        }

        /// <summary>
        ///     The state of this character
        /// </summary>
        public EPawnStates PawnState
        {
            get { return _pawnState; }
            set { _pawnState = value; }
        }

        /// <summary>
        ///     Faction reference of this character
        /// </summary>
        public Taxonomy Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        /// <summary>
        ///     temporary, resourceID of the <see cref="NPC_Type" /> that this character is shifted to
        /// </summary>
        public int ShiftableAppearance
        {
            get { return _shiftableAppearance; }
            set { _shiftableAppearance = value; }
        }

        /// <summary>
        ///     the current velocity of this character
        /// </summary>
        public Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        /// <summary>
        ///     not sure why so many different speed variables are needed TODO investigate
        /// </summary>
        public float GroundSpeedModifier
        {
            get { return _groundSpeedModifier; }
            set { _groundSpeedModifier = value; }
        }

        /// <summary>
        ///     the speed of this character on ground
        /// </summary>
        public int GroundSpeed
        {
            get { return _groundSpeed; }
            set { _groundSpeed = value; }
        }

        /// <summary>
        ///     The current physics of this character (climbing, flying, walking etc)
        /// </summary>
        public EPhysics Physics
        {
            get { return _physics; }
            set { _physics = value; }
        }

        /// <summary>
        ///     Initialize the physical representation of this character. Used for physics queries like certain spells, or raycasts
        /// </summary>
        protected void SetupCollision()
        {
            PhysicsBody = GetComponent<Rigidbody>();
            if (!PhysicsBody)
            {
                PhysicsBody = gameObject.AddComponent<Rigidbody>();
            }
            PhysicsBody.isKinematic = true;
            PhysicsBody.useGravity = false;
            var c = GetComponent<CapsuleCollider>();
            if (!c)
            {
                c = gameObject.AddComponent<CapsuleCollider>();
            }
            c.radius = 0.4f;
            c.height = 1.8f;
            BodyCenterHeight = c.height*0.5f;
            c.center = new Vector3(0, 0f, 0f);
        }

        /// <summary>
        ///     Calculates and returns the movement speed from base speed + physique modifier TODO it's likely that there are more sources to include and a better way to represent it
        /// </summary>
        public int GetEffectiveMoveSpeed()
        {
            var baseSpeed = _groundSpeed*GroundSpeedModifier;
            return (int) (baseSpeed + _groundSpeed*0.01f*MovementSpeedBonus);
        }

        /// <summary>
        ///     Changes the ground speed of this character and broadcasts the changes to the characters relevance. Overriding
        ///     classes must call this base method too
        /// </summary>
        public virtual void SetMoveSpeed(int newSpeed)
        {
            _groundSpeed = newSpeed;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(this));
        }

        /// <summary>
        ///     Forces this character into the State <see cref="newState" /> and broadcasts the change to the characters relevance.
        ///     Overriding classes must call this base method too
        /// </summary>
        /// <param name="newState"></param>
        public virtual void SetPawnState(EPawnStates newState)
        {
            _pawnState = newState;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(this));
        }

        public override void UpdateEntity()
        {
            base.UpdateEntity();
            UpdateDuffs();
            RegenRoutine(1f);
        }

        #region Emotes

        public virtual void DoEmote(EContentEmote emote)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_EMOTES_SV2REL_EMOTE(this, emote));
        }

        public void Sit(bool sitDown, bool onChair = false)
        {
            if (sitDown && SitDown(onChair))
            {
                GoToState(EControllerStates.CPS_PAWN_SITTING);
            }
            else if (Physics == EPhysics.PHYS_SitGround || Physics == EPhysics.PHYS_SitChair)
            {
                GoToState(EControllerStates.CPS_PAWN_ALIVE);
            }
            else return;
        }

        //TODO : Not functioning yet
        public virtual bool SitDown(bool onChairFlag = false)
        {
            if (Physics == EPhysics.PHYS_Walking)
            {

                Velocity = new Vector3();
                if (onChairFlag)
                {
                    Physics = EPhysics.PHYS_SitChair;
                }
                else
                {
                    Physics = EPhysics.PHYS_SitGround;
                }
                
                return true;
            }
            return false;
        }

        public void GoToState(EControllerStates state) {

            string stateString;

            switch (state)
            {
                case EControllerStates.CPS_PAWN_ALIVE:
                    _controllerState = state;
                    stateString = "PawnAlive";
                    break;

                case EControllerStates.CPS_PAWN_DEAD:
                    _controllerState = state;
                    stateString = "PawnDead";
                    break;

                case EControllerStates.CPS_PAWN_SITTING:
                    _controllerState = state;
                    stateString = "PawnSitting";
                    break;

                case EControllerStates.CPS_PAWN_FROZEN:
                    _controllerState = state;
                    stateString = "PawnFrozen";
                    break;

                default:
                    return;
            }

            Message m = PacketCreator.S2R_BASE_PAWN_SV2CL_GOTOSTATE(this, stateString);
            BroadcastRelevanceMessage(m);
            var pc = this as PlayerCharacter;
            if (pc) pc.SendToClient(m);
        }

        #endregion

        #region Effects

        List<AudioVisualSkillEffect> effects = new List<AudioVisualSkillEffect>();

        public List<AudioVisualSkillEffect> Effects
        {
            get { return effects; }
        }

        #endregion

        #region Duffs

        List<DuffInfoData> duffs = new List<DuffInfoData>();

        public List<DuffInfoData> Duffs
        {
            get { return duffs; }
        }

        public DuffInfoData AddDuff(SkillEventDuff newDuff, float duration, EStackType stackType, int stackCount,
            bool visible)
        {
            var duffInfo = new DuffInfoData(Instantiate(newDuff), duration, visible);
            duffInfo.applyTime = Time.time;
            duffInfo.ApplyEffects(this);
            duffs.Add(duffInfo);
            OnDuffsChanged();
            return duffInfo;
        }

        void RemoveDuff(DuffInfoData duff)
        {
            duff.RemoveEffects(this);
            duffs.Remove(duff);
        }

        void UpdateDuffs()
        {
            var changed = false;
            for (var i = duffs.Count; i-- > 0;)
            {
                if (Time.time - duffs[i].applyTime >= duffs[i].duration)
                {
                    RemoveDuff(duffs[i]);
                    changed = true;
                }
                else
                {
                    duffs[i].Update();
                }
            }
            if (changed)
            {
                OnDuffsChanged();
            }
        }

        protected virtual void OnDuffsChanged()
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_UPDATEDUFFS(this, duffs));
        }

        #endregion

        #region Damage/Healing

        public SkillApplyResult Damage(Character source, FSkill s, int amount, Action<SkillApplyResult> callback)
        {
            var result = new SkillApplyResult(source, this, s) {damageCaused = Mathf.Abs((int) SetHealth(Health - amount))};
            callback(result);

            //Valshaaran - added not already dead condition
            if ((PawnState != EPawnStates.PS_DEAD) && Mathf.Approximately(Health, 0))
            {
                SetPawnState(EPawnStates.PS_DEAD);
                OnDiedThroughDamage(source);
            }
            return result;
        }

        //Callback for skillcasts
        protected virtual void OnDamageReceived(SkillApplyResult sap)
        {
            if (sap.damageCaused != 0)
            {
                if (RelevanceContainsPlayers)
                {
                    var m = PacketCreator.S2R_BASE_PAWN_SV2CLREL_DAMAGEACTIONS(this, 1f);
                    BroadcastRelevanceMessage(m);
                }
            }

            IsInteracting = false;
        }

        public virtual void OnDamageCaused(SkillApplyResult sap)
        {
        }

        public SkillApplyResult Heal(Character source, FSkill s, int amount)
        {
            var result = new SkillApplyResult(source, this, s);
            result.healCaused = (int) SetHealth(Health + amount);
            OnHealReceived(result);
            return result;
        }

        protected virtual void OnDiedThroughDamage(Character source)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PAWN_SV2REL_COMBATMESSAGEDEATH(this, source));
        }

        protected virtual void OnHealReceived(SkillApplyResult sap)
        {
        }

        public virtual void OnHealingCaused(SkillApplyResult sap)
        {
        }

        public virtual void OnStatChangeCaused(SkillApplyResult sap)
        {
        }

        #endregion

        #region Effects

        public virtual void PlayEffect(EPawnEffectType effectType)
        {
            if (!RelevanceContainsPlayers) return;
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT(this, effectType);
            BroadcastRelevanceMessage(m);
        }

        public virtual void PlayEffectDirect(int effectID)
        {
            if (!RelevanceContainsPlayers) return;
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECTDIRECT(this, effectID);
            BroadcastRelevanceMessage(m);
        }

        public virtual void PlaySound(EPawnSound soundEffect, float volume)
        {
            if (!RelevanceContainsPlayers) return;
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_STATICPLAYSOUND(this, soundEffect, volume);
            BroadcastRelevanceMessage(m);
        }

        #endregion

        #region Misc

        readonly Dictionary<int, float> animationDurations = new Dictionary<int, float>
        {
            {2, 2f}
        };

        public float GetDurationForAnimation(int animNr)
        {
            float value;
            return animationDurations.TryGetValue(animNr, out value) ? value : 1.5f;
        }

        #endregion

        #region Combat

        [NonSerialized] public EWeaponCategory equippedWeaponType; //TODO: temporary, equip real weapon

        Item_Type MainHandWeapon;
        Item_Type OffHandWeapon;

        [NonSerialized] ECombatMode combatMode;

        public ECombatMode CombatMode
        {
            get { return combatMode; }
            set { combatMode = value; }
        }

        public virtual void SheatheWeapon()
        {
            equippedWeaponType = EWeaponCategory.EWC_None;
            combatMode = ECombatMode.CBM_Idle;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(this));
        }

        public virtual void DrawWeapon()
        {
            var s = activeSkillDeck.GetSkillFromLastActiveSlot();
            if (s != null)
            {
                equippedWeaponType = s.requiredWeapon;
            }
            combatMode = ECombatMode.CBM_Melee;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(this));
        }

        public virtual void SwitchWeapon(EWeaponCategory newWeapon)
        {
            switch (newWeapon)
            {
                case EWeaponCategory.EWC_Unarmed:
                    CombatMode = ECombatMode.CBM_Melee;
                    break;
                case EWeaponCategory.EWC_MeleeOrUnarmed:
                    CombatMode = ECombatMode.CBM_Cast;
                    break;
                case EWeaponCategory.EWC_Melee:
                    CombatMode = ECombatMode.CBM_Melee;
                    break;
                case EWeaponCategory.EWC_Ranged:
                    CombatMode = ECombatMode.CBM_Ranged;
                    break;
            }
            equippedWeaponType = newWeapon;
            if (combatMode != ECombatMode.CBM_Idle)
            {
                if (!RelevanceContainsPlayers) return;
                BroadcastRelevanceMessage(PacketCreator.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(this));
            }
        }

        #endregion

        #region Interaction

        public bool IsInteracting;

        public virtual void Interact(PlayerCharacter source, ERadialMenuOptions menuOption)
        {
            DoEmote(EContentEmote.ECE_no);
        }

        public virtual void ReactTo(Character ch) //maybe this function is used to update or load quest objectives
        {
            if (faction.Likes(ch.faction))
            {
                if (25 > Random.Range(0, 100)) //greeting emote chance 
                {
                    DoEmote(EContentEmote.ECE_wave);
                }
            }
            else
            {
                //if (75 > Random.Range(0, 100)) //greeting emote chance 
                //{
                DoEmote(EContentEmote.ECE_battle);
                //}
            }
        }

        #endregion
    }
}
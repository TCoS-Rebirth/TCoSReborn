using System;
using System.Collections.Generic;
using Common;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using Gameplay.Skills.Events;
using Network;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Gameplay.Entities
{
    public abstract partial class Character : Entity
    {
        const float MOVESPEED_MULTIPLIER = 1f;

        bool _freezePosition;

        bool _freezeRotation;

        int _groundSpeed = 200;

        float _groundSpeedModifier = MOVESPEED_MULTIPLIER;

        [SerializeField, ReadOnly] EPawnStates _pawnState = EPawnStates.PS_ALIVE;

        EPhysics _physics = EPhysics.PHYS_Walking;

        int _shiftableAppearance;

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
        ///     whether the movement of this character is currently frozen
        /// </summary>
        public bool FreezePosition
        {
            get { return _freezePosition; }
            set { _freezePosition = value; }
        }

        /// <summary>
        ///     whether the rotation of this character is currently frozen
        /// </summary>
        public bool FreezeRotation
        {
            get { return _freezeRotation; }
            set { _freezeRotation = value; }
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
        ///     Calculates and returns the movement speed from base speed + physique modifier TODO it's likely that there are more
        ///     sources to include
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
            HandleSkillCasting();
            UpdateDuffs();
        }

        #region Emotes

        public virtual void DoEmote(EContentEmote emote)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_EMOTES_SV2REL_EMOTE(this, emote));
        }

        #endregion

        #region Effects

        List<AudioVisualSkillEffect> effects = new List<AudioVisualSkillEffect>();

        public List<AudioVisualSkillEffect> Effects
        {
            get { return effects; }
        }

        #endregion

        #region Skills

        List<FSkill> skills = new List<FSkill>();

        public List<FSkill> Skills
        {
            get { return skills; }
            set { skills = value; }
        }

        [SerializeField, ReadOnly] SkillDeck activeSkillDeck;

        public SkillDeck ActiveSkillDeck
        {
            get { return activeSkillDeck; }
            protected set { activeSkillDeck = value; }
        }

        //temporary
        float lastBarRollTime;
        float comboTimeLimit = 10f;

        void CheckResetSkillbar()
        {
            if (activeSkillDeck == null)
            {
                return;
            }
            if (activeSkillDeck.GetActiveTierIndex() != 0)
            {
                if (Time.time - lastBarRollTime > comboTimeLimit)
                {
                    activeSkillDeck.ResetRoll();
                    if (combatMode != ECombatMode.CBM_Idle)
                    {
                        var s = activeSkillDeck.GetSkillFromLastActiveSlot();
                        if (s != null)
                        {
                            SwitchWeapon(s.requiredWeapon);
                        }
                    }
                }
            }
        }

        void RollDeck()
        {
            activeSkillDeck.RollDeck();
            lastBarRollTime = Time.time;
            if (combatMode != ECombatMode.CBM_Idle)
            {
                var s = activeSkillDeck.GetSkillFromLastActiveSlot();
                if (s != null)
                {
                    SwitchWeapon(s.requiredWeapon);
                }
            }
        }

        public FSkill GetSkill(int id)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                if (skills[i].resourceID == id)
                {
                    return skills[i];
                }
            }
            return null;
        }

        protected bool HasSkill(int id)
        {
            return GetSkill(id) != null;
        }

        public SkillLearnResult LearnSkill(FSkill skill)
        {
            if (skill == null)
            {
                return SkillLearnResult.Invalid;
            }
            if (HasSkill(skill.resourceID))
            {
                return SkillLearnResult.AlreadyKnown;
            }
            skills.Add(skill);
            OnLearnedSkill(skill);
            return SkillLearnResult.Success;
        }

        protected virtual void OnLearnedSkill(FSkill s)
        {
        }

        public ESkillStartFailure UseSkill(int skillID, int targetID, Vector3 targetPosition, float time,
            Vector3 camPos = default(Vector3))
        {
            var s = GetSkill(skillID);
            return UseSkill(s, targetID, targetPosition, time, camPos);
        }

        public ESkillStartFailure UseSkillIndex(int index, int targetID, Vector3 targetPosition, float time,
            Vector3 camPos = default(Vector3))
        {
            if (activeSkillDeck == null)
            {
                return ESkillStartFailure.SSF_INVALID_SKILL;
            }
            var s = activeSkillDeck.GetSkillFromActiveTier(index);
            return UseSkill(s, targetID, targetPosition, time, camPos);
        }

        ESkillStartFailure UseSkill(FSkill s, int targetID, Vector3 targetPosition, float time,
            Vector3 camPos = default(Vector3))
        {
            if (_pawnState == EPawnStates.PS_DEAD)
            {
                return ESkillStartFailure.SSF_DEAD;
            }
            if (IsCasting)
            {
                return ESkillStartFailure.SSF_STILL_EXECUTING_SKILL;
            }
            if (s == null)
            {
                return ESkillStartFailure.SSF_INVALID_SKILL;
            }
            if (!s.IsCooldownReady(time))
            {
                return ESkillStartFailure.SSF_COOLING_DOWN;
            }
            var skillTarget = GetRelevantEntity<Character>(targetID);
            s.Reset();
            var skillInfo = new SkillContext(s, this, targetPosition, camPos, skillTarget, Time.time);
            OnStartCastSkill(skillInfo);
            activeSkill = skillInfo;
            activeSkill.ExecutingSkill.LastCast = Time.time;
            return ESkillStartFailure.SSF_ALLOWED;
        }

        protected virtual void OnStartCastSkill(SkillContext s)
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL(this, s));
            //TODO: modify attackSpeed with buffs, stats etc
        }

        protected virtual void OnEndCastSkill(SkillContext s)
        {
        }

        public List<Character> QueryMeleeSkillTargets(SkillEffectRange range)
        {
            var queriedTargets = new List<Character>();
            var cols =
                UnityEngine.Physics.OverlapSphere(transform.position + UnitConversion.ToUnity(range.locationOffset),
                    range.maxRadius*UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c != null && c != this && relevantObjects.Contains(c))
                {
                    if (IsFacing(c.Position, range.angle))
                        queriedTargets.Add(c);
                }
            }
            return queriedTargets;
        }

        public List<Character> QueryRangedSkillTargets(Vector3 point, SkillEffectRange range)
        {
            var queriedTargets = new List<Character>();
            var cols = UnityEngine.Physics.OverlapSphere(point, range.maxRadius*UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c != null && c != this && relevantObjects.Contains(c))
                {
                    if (IsFacing(c.Position, range.angle))
                        queriedTargets.Add(c);
                }
            }
            return queriedTargets;
        }

        #region Casting/Execution

        SkillContext activeSkill;

        public bool IsCasting
        {
            get { return activeSkill != null; }
        }

        void HandleSkillCasting()
        {
            CheckResetSkillbar();
            if (activeSkill == null)
            {
                return;
            }
            var executingSkill = activeSkill.ExecutingSkill;
            activeSkill.currentSkillTime = Time.time - activeSkill.StartTime;
            if (executingSkill.keyFrames.Count == 0)
            {
                OnEndCastSkill(activeSkill);
                RollDeck();
                activeSkill.Cleanup();
                activeSkill = null;
            }
            else
            {
                executingSkill.RunEvents(activeSkill);
            }
            if (activeSkill.currentSkillTime > executingSkill.GetSkillDuration(this))
                //TODO: use correct varNr (specific to weapon?)
            {
                OnEndCastSkill(activeSkill);
                RollDeck();
                activeSkill.Cleanup();
                activeSkill = null;
            }
        }

        #endregion

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

        public SkillApplyResult Damage(Character source, FSkill s, int amount)
        {
            var result = new SkillApplyResult(source, this, s);
            result.damageCaused = Mathf.Abs((int) SetHealth(Health - amount));

            //Valshaaran - added not already dead condition
            if ((PawnState != EPawnStates.PS_DEAD) && Mathf.Approximately(Health, 0))
            {
                SetPawnState(EPawnStates.PS_DEAD);
                OnDiedThroughDamage(source);
            }
            OnDamageReceived(result);
            return result;
        }

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

        Dictionary<int, float> animationDurations = new Dictionary<int, float>
        {
            {2, 2f}
        };

        public float GetDurationForAnimation(int animNr)
        {
            var value = 1.25f;
            if (animationDurations.TryGetValue(animNr, out value))
            {
                return value;
            }
            return 1.25f;
        }

        public virtual void RunEvent(SkillContext sInfo, SkillEventFX fxEvent, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn,
                sInfo.currentSkillTime));
        }

        public virtual void RunEventL(SkillContext sInfo, SkillEventFX fxEvent, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn, location,
                sInfo.currentSkillTime));
        }

        #endregion

        #region Combat

        [NonSerialized] public EWeaponCategory equippedWeaponType; //TODO: temporary, equip real weapon

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
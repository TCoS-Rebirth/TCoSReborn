using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using Gameplay.Skills.Events;
using Network;
using UnityEditor;
using UnityEngine;
using Utility;
using Object = UnityEngine.Object;

namespace Gameplay.Entities
{
    public abstract partial class Character
    {

        const float MaxComboTime = 10f;

        List<FSkill> skills = new List<FSkill>();

        public List<FSkill> Skills
        {
            get { return skills; }
            set { skills = value; }
        }

        [SerializeField, ReadOnly]
        SkillDeck activeSkillDeck;

        public SkillDeck ActiveSkillDeck
        {
            get { return activeSkillDeck; }
            protected set { activeSkillDeck = value; }
        }

        void RollDeck()
        {
            if (combatMode == ECombatMode.CBM_Idle)
            {
                activeSkillDeck.Reset();
                StopComboTimer();
                return;
            }
            activeSkillDeck.RollDeck();
            if (activeSkillDeck.GetActiveTierIndex() > 0)
            {
                StartComboTimer();
            }
            else
            {
                StopComboTimer();
            }
            var s = activeSkillDeck.GetSkillFromLastActiveSlot();
            if (s != null)
            {
                SwitchWeapon(s.requiredWeapon);
            }
        }

        void StartComboTimer()
        {
            //TODO log combo string
            if (skillbarResetRoutine == null)
            {
                skillbarResetRoutine = StartCoroutine(ResetSkillbarTimed());
            }
        }

        void StopComboTimer()
        {
            //TODO clear combo string
            if (skillbarResetRoutine != null)
            {
                Debug.Log("stopping skillbar reset routine");
                StopCoroutine(skillbarResetRoutine);
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

        public ESkillStartFailure UseSkill(int skillID, int targetID, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
        {
            var s = GetSkill(skillID);
            return UseSkill(s, targetID, targetPosition, time, camPos);
        }

        public ESkillStartFailure UseSkillIndex(int index, int targetID, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
        {
            if (activeSkillDeck == null)
            {
                return ESkillStartFailure.SSF_INVALID_SKILL;
            }
            var s = activeSkillDeck.GetSkillFromActiveTier(index);
            return UseSkill(s, targetID, targetPosition, time, camPos);
        }

        ESkillStartFailure UseSkill(FSkill s, int targetID, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
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
            var skillTarget = GetRelevantEntity<Character>(targetID); //temporary
            s.Reset();
            var skillInfo = new RunningSkillContext(s, s.GetSkillDuration(this), this, targetPosition, camPos, skillTarget, Time.time);
            OnStartCastSkill(skillInfo);
            activeSkill = skillInfo;
            activeSkill.ExecutingSkill.LastCast = Time.time;
            StartCoroutine(RunSkill(activeSkill)); //Starts the casting
            return ESkillStartFailure.SSF_ALLOWED;
        }

        protected virtual void OnStartCastSkill(RunningSkillContext s)
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL(this, s));
            //TODO: modify attackSpeed with buffs, stats etc
        }

		/// <summary>
        /// Used in PlayerCharacter to clear the last active skill
        /// </summary>
        /// <param name="s"></param>
        protected virtual void OnEndCastSkill(RunningSkillContext s)
        {
        }

        public List<Character> QueryMeleeSkillTargets(SkillEffectRange range)
        {
            var queriedTargets = new List<Character>();
            var cols = UnityEngine.Physics.OverlapSphere(transform.position + UnitConversion.ToUnity(range.locationOffset), range.maxRadius * UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c != null
                    && c != this
                    && c.PawnState != EPawnStates.PS_DEAD
                    && !faction.Likes(c.Faction)
                    && relevantObjects.Contains(c))
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
            var cols = UnityEngine.Physics.OverlapSphere(point, range.maxRadius * UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c != null
                    && c != this
                    && c.PawnState != EPawnStates.PS_DEAD
                    && !faction.Likes(c.Faction)
                    && relevantObjects.Contains(c))
                {
                    if (IsFacing(c.Position, range.angle))
                        queriedTargets.Add(c);
                }
            }
            return queriedTargets;
        }

        #region Casting/Execution

        RunningSkillContext activeSkill;
        Coroutine skillbarResetRoutine;

        public bool IsCasting
        {
            get { return activeSkill != null; }
        }

        IEnumerator RunSkill(RunningSkillContext context)
        {
            var currentEventIndex = 0;
            while (currentEventIndex < context.ExecutingSkill.keyFrames.Count)
            {
                var keyFrame = context.ExecutingSkill.keyFrames[currentEventIndex];
                if (keyFrame.EventGroup == null)
                {
                    currentEventIndex++;
                    continue;
                }
                Debug.Log(string.Format("[{0}] - waiting for keyframetime of {1}", Time.time, context.ExecutingSkill));
                while (context.GetCurrentSkillTime() < keyFrame.Time)
                {
                    if (context.Aborted)
                    { 
                        OnEndCastSkill(context);
                        activeSkill.ExecutingSkill.Reset();
                        activeSkill = null;
                        yield break;
                    }
                    yield return null;
                }
                context.CurrentEventStart = Time.time;
                Debug.Log(string.Format("[{0}] - executing events of {1}", Time.time, context.ExecutingSkill));
                var pendingEvents = new List<SkillEvent>();
                for (var i = 0; i < keyFrame.EventGroup.events.Count; i++)
                {
                    var ev = keyFrame.EventGroup.events[i];
                    if (ev != null)
                    {
                        pendingEvents.Add(ev);
                    }
                }
                while (pendingEvents.Count > 0)
                {
                    for (var i = pendingEvents.Count; i-- > 0;)
                    {
                        if (pendingEvents[i].Execute(context))
                        {
                            pendingEvents.RemoveAt(i);
                        }
                    }
                    yield return null;
                }
                currentEventIndex++;
            }
            //Debug.Log(string.Format("[{0}] - waiting for skillanimation to end of {1}", Time.time, context.ExecutingSkill));
            while (Time.time - context.StartTime < context.Duration) yield return null; //wait for animation to finish (will have to be timed with real animation durations, attackspeed etc)
            Debug.Log(string.Format("[{0}] - skill ended: {1}", Time.time, context.ExecutingSkill));
            OnEndCastSkill(context);
            activeSkill.ExecutingSkill.Reset();
            activeSkill = null;
            RollDeck();
        }

        //----------------
        [MenuItem("Spellborn/EnumNames")]
        static void EnumerateSkillEventTypes()
        {
            var typeNames = new HashSet<string>();
            int evaluated = 0;
            var collections = GetAtPath<SkillCollection>("GameData/Skills");
            foreach (var skillCollection in collections)
            {
                foreach (var skill in skillCollection.skills)
                {
                    foreach (var keyFrame in skill.keyFrames)
                    {
                        if (keyFrame.EventGroup == null) continue;
                        foreach (var skillEvent in keyFrame.EventGroup.events)
                        {
                            
                            RecurseGetEventNames(skillEvent, typeNames, ref evaluated);
                            //typeNames.Add(skillEvent.GetType().Name);
                        }
                    }
                }
            }
            Debug.Log(evaluated + " evaluated");
            foreach (var typeName in typeNames)
            {
                Debug.Log(typeName);
            }
            typeNames.Clear();
        }

        static void RecurseGetEventNames(ScriptableObject s, HashSet<string> names, ref int evaluated)
        {
            evaluated++;
            names.Add(s.GetType().Name);
            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields)
            {
                if (!fieldInfo.FieldType.IsSubclassOf(typeof (ScriptableObject))) continue;
                var child = fieldInfo.GetValue(s) as ScriptableObject;
                if (child != null)
                {
                    RecurseGetEventNames(child, names, ref evaluated);
                }
            }
        }

        public static T[] GetAtPath<T>(string path)
        {

            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
            foreach (string fileName in fileEntries)
            {
                int assetPathIndex = fileName.IndexOf("Assets");
                string localPath = fileName.Substring(assetPathIndex);

                Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }
            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }
        //----------------

        IEnumerator ResetSkillbarTimed()
        {
            yield return new WaitForSeconds(MaxComboTime);
            while (activeSkill != null)
            {
                yield return null;
            }
            if (activeSkillDeck == null || activeSkillDeck.GetActiveTierIndex() == 0)
            {
                skillbarResetRoutine = null;
                yield break;
            }
            activeSkillDeck.ResetRoll();
            if (combatMode == ECombatMode.CBM_Idle)
            {
                skillbarResetRoutine = null;
                yield break;
            }
            var s = activeSkillDeck.GetSkillFromLastActiveSlot();
            if (s != null)
            {
                SwitchWeapon(s.requiredWeapon);
            }
        }

        public virtual void RunEvent(RunningSkillContext sInfo, SkillEventFX fxEvent, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn,
                sInfo.GetCurrentSkillTime()));
        }

        public virtual void RunEventL(RunningSkillContext sInfo, SkillEventFX fxEvent, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn, location,
                sInfo.GetCurrentSkillTime()));
        }

        #endregion

    }
}

using Gameplay.Skills;
using UnityEngine;
using World.Paths;

namespace Gameplay.Entities.NPCs.Behaviours
{
    public abstract class NPCBehaviour : MonoBehaviour
    {
        protected NpcCharacter owner;

        public abstract string Description { get; }

        void Start()
        {
            owner = GetComponent<NpcCharacter>();
            if (!owner)
            {
                Destroy(this);
                return;
            }
            owner.AttachBehaviour(this);
            OnStart();
        }

        public void UpdateBehaviour()
        {
            OnUpdate();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        public virtual void OnLearnedRelevance(Entity rel)
        {
        }

        public virtual void OnReleasedRelevance(Entity rel)
        {
        }

        public virtual void OnDamage(Character source, FSkill_Type s, int amount)
        {
        }

        public virtual void OnHeal(Character source, int amount)
        {
        }

        public virtual void OnTeleported()
        {
        }

        public virtual void OnEndedCast(FSkill_Type s)
        {
        }

        protected PatrolPoint GetHasLinkedPatrolPoint()
        {
            return owner.RespawnInfo.linkedPatrolPoint;
        }
    }
}
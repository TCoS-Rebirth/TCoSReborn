using UnityEngine;

namespace Gameplay.Entities.Interactives
{
    public class InteractionProgress : InteractionComponent
    {

        public static float defaultSeconds = 5.0f;
        [ReadOnly]
        public float ProgressSeconds;

        [SerializeField]
        float timer;

        [SerializeField]
        bool aborted;

        [SerializeField]
        PlayerCharacter instigatorPlayer;

        [SerializeField]
        Vector3 location;

        [SerializeField] float health;
        [SerializeField] bool isShifted;
        [SerializeField] int activeSkillsCount;


        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);
            instigator.IsInteracting = true;
            if (owner)
            {
                if (!reverse)
                {
                    instigatorPlayer = instigator;
                    location = instigatorPlayer.Position;
                    health = instigatorPlayer.Health;
                    isShifted = instigatorPlayer.IsShifted;
                    //ActiveSkillsCount = InstigatorPlayer.Skills.GetActiveSkillCount(); TODO
                    if (ProgressSeconds > 0.0f) timer = ProgressSeconds;
                    else timer = defaultSeconds;
                    aborted = false;
                    owner.StartClientSubAction();
                }
            }           
        }

        public override void onCancel(PlayerCharacter instigator)
        {
            base.onCancel(instigator);
            instigator.IsInteracting = false;

            if (owner)
            {
                owner.CancelClientSubAction();
            }
        }

        public override void onEnd(PlayerCharacter instigator, bool reverse)
        {            

            if (owner)
            {
                if (!reverse)
                {
                    timer = 0.0f;
                    instigatorPlayer = null;
                    if(aborted)
                    {
                        owner.CancelOptionActions();
                    }
                    else
                    {
                        owner.EndClientSubAction();
                    }
                }
            }

            instigator.IsInteracting = false;

            base.onEnd(instigator, reverse);
        }

        public override void OnZoneUpdate()
        {
            base.OnZoneUpdate();

            if (instigatorPlayer && instigatorPlayer.IsInteracting)
            {
                if (    instigatorPlayer.Position != location
                    ||  instigatorPlayer.Health < health
                    ||  instigatorPlayer.IsShifted != isShifted
                    )   //TODO: if active skill count changed
                {
                    aborted = true;
                    owner.CancelOptionActions();
                    return;
                }

                timer -= Time.deltaTime;
                if (timer <= 0) { owner.NextSubAction(); }         
            }
        }
    }
}
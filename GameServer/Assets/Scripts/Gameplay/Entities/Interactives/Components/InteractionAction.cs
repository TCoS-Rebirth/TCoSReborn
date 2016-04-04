using Gameplay.Events;
using System.Collections.Generic;

namespace Gameplay.Entities.Interactives
{

    public class InteractionAction : InteractionComponent
    {
        [ReadOnly]
        public List<Content_Event> Actions;

        public override void onStart(PlayerCharacter instigator, bool reverse)
        {
            base.onStart(instigator, reverse);

            if (!reverse)
            {
                foreach(var ce in Actions)
                {
                    if (ce)
                    {
                        ce.TryExecute(instigator, instigator);
                    }

                }
                //TODO: do any actions need to be waited for?
                owner.NextSubAction();
            }            
        }
    }
}
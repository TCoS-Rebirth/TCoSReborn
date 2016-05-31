using System;
using Gameplay.Entities;
using World;

namespace Gameplay.Events
{
    public class EV_Teleport : Content_Event
    {
        public bool IsInstance;
        public string Parameter;
        public int TargetWorld;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (subject as PlayerCharacter) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            var dest = GameWorld.Instance.FindTravelDestination(Parameter);
            GameWorld.Instance.TravelPlayer(playerSubj, dest);
        }
    }
}
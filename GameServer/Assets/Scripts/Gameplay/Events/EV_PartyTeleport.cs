using Gameplay.Entities;
using World;

namespace Gameplay.Events
{
    public class EV_PartyTeleport : Content_Event
    {
        public string portalName;
        public int TargetWorld;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (subject as PlayerCharacter) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            var team = playerSubj.Team;
            var dest = GameWorld.Instance.FindTravelDestination(portalName);

            if (team == null)
            {
                GameWorld.Instance.TravelPlayer(playerSubj, dest);
                return;
            }

            team.StartPartyTravel(TargetWorld, portalName);
        }
    }
}
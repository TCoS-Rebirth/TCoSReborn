using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Claustroport : Content_Event
    {
        public string DestinationTag;
        public float MaxDistance;
        public bool UseOrientation;

        public override void Execute(PlayerCharacter p)
        {
            base.Execute(p);

            //Match dest tag
            var dest = p.ActiveZone.FindTravelDestination(DestinationTag);

            //port player to dest
            if (UseOrientation)
                p.TeleportTo(dest.Position, dest.Rotation);
            else
                p.TeleportTo(dest.Position, p.Rotation);
        }
    }
}
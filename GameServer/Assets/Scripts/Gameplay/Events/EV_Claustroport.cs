using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Claustroport : Content_Event
    {
        public string DestinationTag;
        public float MaxDistance;
        public bool UseOrientation;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (subject as Character && subject.ActiveZone.FindTravelDestination(DestinationTag)) return true;
            else return false;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var charSubj = subject as Character;

            //Match dest tag
            var dest = charSubj.ActiveZone.FindTravelDestination(DestinationTag);

            //port player to dest
            if (UseOrientation)
                charSubj.TeleportTo(dest.Position, dest.Rotation);
            else
                charSubj.TeleportTo(dest.Position, charSubj.Rotation);
        }
    }
}
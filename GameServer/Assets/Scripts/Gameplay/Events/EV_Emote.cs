using Common;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Emote : Content_Event
    {
        public EContentEmote Emote;

        public override void Execute(Character source)
        {
            base.Execute(source);
            source.DoEmote(Emote);
        }
    }
}
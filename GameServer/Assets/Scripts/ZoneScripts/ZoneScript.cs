using Common;
using Gameplay.Entities;
using Gameplay.Entities.Interactives;
using World;

namespace ZoneScripts
{
    public abstract class ZoneScript
    {
        protected Zone AttachedZone;

        public void Attach(Zone attachToZone)
        {
            AttachedZone = attachToZone;
        }

        public virtual void OnAfterLoaded()
        {
        }

        public virtual void OnBeforeShutDown()
        {
        }

        public virtual void OnPlayerEntered(PlayerCharacter pc)
        {
        }

        public virtual void OnPlayerLeaves(PlayerCharacter pc)
        {
        }

        public virtual void OnInteractiveElementAdded(InteractiveLevelElement el)
        {
            
        }

        public virtual void OnInteractiveElementRemoved(InteractiveLevelElement el)
        {
        }

        public virtual void OnStartEvent(SBEvent ev) { }

        public virtual void OnStopEvent(SBEvent ev) { }

        public virtual void Update()
        {
        }

        public static ZoneScript GetScript(MapIDs map)
        {
            switch (map)
            {
                case MapIDs.PT_HAWKSMOUTH:
                    return new ZSHawksmouth();
                default:
                    return new DefaultZoneScript();
            }
        }
    }
}
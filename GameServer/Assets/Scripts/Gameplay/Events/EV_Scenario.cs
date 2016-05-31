using System;
using Gameplay.Entities;

namespace Gameplay.Events
{
    public class EV_Scenario : Content_Event
    {
        public string ScenarioTag;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }
        /*
function sv_Execute(Game_Pawn anObject,Game_Pawn aSubject) {
local export editinline Scenario aScenario;
aScenario = Scenario(static.DynamicLoadObject(ScenarioTag,Class'Scenario'));//0000 : 0F 00 20 B7 6A 22 2E 68 75 92 01 1C 60 E7 69 0F 01 30 52 4C 23 20 68 75 92 01 16
aScenario.ForwardEvents(anObject);
}
*/
    }
}
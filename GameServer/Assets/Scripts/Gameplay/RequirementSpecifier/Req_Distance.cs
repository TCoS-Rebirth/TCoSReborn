using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Distance : Content_Requirement
    {
        public string ActorTag;
        public int Distance;
        public EContentOperator Operator;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet(p);
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet(n);
        }

        public bool isMet(Character c)
        {
            //TODO: find target by actortag, find distance from player, do operator

            /*
            if (SBOperator.Operate( Vector3.Distance(a.Position, b.Position),
                                    Operator, 
                                    UnitConversion.UnrUnitsToMeters * Distance)
                )
            {
                return true;
            }
            else { return false; }
             */
            throw new NotImplementedException();
        }

        public override bool CheckPawn(Character character)
        {
            throw new NotImplementedException();
        }
    }
}
using System.Collections.Generic;

namespace Gameplay.Entities.NPCs
{
    public class NS_Rotator : NPC_StatTable
    {
        public int DefaultBody;
        public int DefaultFocus;
        public int DefaultMind;
        public int Hitpoints;
        public int HpPerLevel;

        public List<byte> Rotation;

        protected int GetPointsForStat(int aLevel, ERotStatPriority aStat)
        {
            int totalPoints;
            int i;
            int ret;
            ret = 0;
            totalPoints = PointsAtLevel(aLevel);
            if (Rotation.Count != 0)
            {
                i = 0;
                while (i < totalPoints)
                {
                    if (Rotation[i%Rotation.Count] == (byte) aStat)
                    {
                        ret += PointsMultiplier;
                    }
                    i++;
                }
            }
            return ret;
        }

        public override int GetFocus(int aLevel)
        {
            return DefaultFocus + GetPointsForStat(aLevel, ERotStatPriority.ERSP_Focus);
        }

        public override int GetMind(int aLevel)
        {
            return DefaultMind + GetPointsForStat(aLevel, ERotStatPriority.ERSP_Mind);
        }

        public override int GetBody(int aLevel)
        {
            return DefaultBody + GetPointsForStat(aLevel, ERotStatPriority.ERSP_Body);
        }

        public override int GetHitpointsPerLevel(int aLevel)
        {
            return HpPerLevel;
        }

        public override int GetBaseHitpoints(int aLevel)
        {
            return Hitpoints;
        }

        protected enum ERotStatPriority
        {
            ERSP_Body,
            ERSP_Focus,
            ERSP_Mind
        }
    }
}
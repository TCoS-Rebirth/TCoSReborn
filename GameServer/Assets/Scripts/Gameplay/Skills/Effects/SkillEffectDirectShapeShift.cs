using Gameplay.Entities.NPCs;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectShapeShift : SkillEffectDirect
    {
        [ReadOnly] public NPC_Type shape;

        [ReadOnly] public ValueSpecifier shapeShiftValue;

        [ReadOnly] public string temporaryShapeName;
    }
}
using System;
using Common;

namespace Gameplay.RequirementSpecifier
{
    public static class SBOperator
    {
        public static bool Operate(float var1, EContentOperator op, float var2)
        {
            switch (op)
            {
                case EContentOperator.ECO_EqualOrLess:
                    return var1 <= var2;

                case EContentOperator.ECO_EqualOrMore:
                    return var1 >= var2;

                case EContentOperator.ECO_Equals:
                    return var1 == var2;

                case EContentOperator.ECO_Less:
                    return var1 < var2;

                case EContentOperator.ECO_More:
                    return var1 > var2;

                case EContentOperator.ECO_NotEquals:
                    return var1 != var2;

                //TODO: Mask operators
                case EContentOperator.ECO_Mask:
                    throw new NotImplementedException();

                case EContentOperator.ECO_NotMask:
                    throw new NotImplementedException();

                default:
                    return false;
            }
        }

        public static bool Operate(int var1, EContentOperator op, int var2)
        {
            switch (op)
            {
                case EContentOperator.ECO_EqualOrLess:
                    return var1 <= var2;

                case EContentOperator.ECO_EqualOrMore:
                    return var1 >= var2;

                case EContentOperator.ECO_Equals:
                    return var1 == var2;

                case EContentOperator.ECO_Less:
                    return var1 < var2;

                case EContentOperator.ECO_More:
                    return var1 > var2;

                case EContentOperator.ECO_NotEquals:
                    return var1 != var2;

                //TODO: Mask operators
                case EContentOperator.ECO_Mask:
                    throw new NotImplementedException();

                case EContentOperator.ECO_NotMask:
                    throw new NotImplementedException();

                default:
                    return false;
            }
        }
    }
}
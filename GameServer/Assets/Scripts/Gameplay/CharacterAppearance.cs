using Common;

namespace Gameplay
{
    public class CharacterAppearance
    {
        public int AppearancePart1;
        public int AppearancePart2;
        int bodyColor;
        int bodyType;
        int chestTattoo;
        CharacterGender gender;
        int hairColor;
        int hairStyle;
        int headType;
        int race;
        int tattooLeft;
        int tattooRight;
        int voice;

        public CharacterAppearance(params int[] vals)
        {
            race = vals[0];
            gender = (CharacterGender) vals[1];
            bodyType = vals[2];
            headType = vals[3];
            bodyColor = vals[4];
            chestTattoo = vals[5];
            tattooLeft = vals[6];
            tattooRight = vals[7];
            hairStyle = vals[8];
            hairColor = vals[9];
            voice = vals[10];
            RecalculateAppearanceParts();
        }

        public int Voice
        {
            get { return voice; }
            set
            {
                voice = value;
                RecalculateAppearanceParts();
            }
        }

        public int Race
        {
            get { return race; }
            set
            {
                race = value;
                RecalculateAppearanceParts();
            }
        }

        public CharacterGender Gender
        {
            get { return gender; }
            set
            {
                gender = value;
                RecalculateAppearanceParts();
            }
        }

        public int HeadType
        {
            get { return headType; }
            set
            {
                headType = value;
                RecalculateAppearanceParts();
            }
        }

        public int HairStyle
        {
            get { return hairStyle; }
            set
            {
                hairStyle = value;
                RecalculateAppearanceParts();
            }
        }

        public int HairColor
        {
            get { return hairColor; }
            set
            {
                hairColor = value;
                RecalculateAppearanceParts();
            }
        }

        public int BodyType
        {
            get { return bodyType; }
            set
            {
                bodyType = value;
                RecalculateAppearanceParts();
            }
        }

        public int BodyColor
        {
            get { return bodyColor; }
            set
            {
                bodyColor = value;
                RecalculateAppearanceParts();
            }
        }

        public int ChestTattoo
        {
            get { return chestTattoo; }
            set
            {
                chestTattoo = value;
                RecalculateAppearanceParts();
            }
        }

        public int TattooLeft
        {
            get { return tattooLeft; }
            set
            {
                tattooLeft = value;
                RecalculateAppearanceParts();
            }
        }

        public int TattooRight
        {
            get { return tattooRight; }
            set
            {
                tattooRight = value;
                RecalculateAppearanceParts();
            }
        }

        void RecalculateAppearanceParts()
        {
            var a1 = 0;
            a1 = a1 | race;
            a1 = a1 | ((int) gender << 1);
            a1 = a1 | (bodyType << 2);
            a1 = a1 | (headType << 4);
            a1 = a1 | (0 << 10);
            a1 = a1 | (bodyColor << 11);
            a1 = a1 | (chestTattoo << 19);
            a1 = a1 | (tattooLeft << 23);
            a1 = a1 | (tattooRight << 27);
            a1 = a1 | ((0 & 1) << 31); //unused tattoo
            AppearancePart1 = a1;

            var a2 = 0;
            a2 = a2 | (0 >> 1); //unused tattoo
            a2 = a2 | (hairColor << 3);
            a2 = a2 | (voice << 20);
            a2 = a2 | (hairStyle << 23);
            AppearancePart2 = a2;
        }
    }
}
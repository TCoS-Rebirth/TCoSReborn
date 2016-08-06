using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay
{
    public class Game_Appearance: ScriptableObject
    {

        Character Owner;

        public virtual void Init(Character owner)
        {
            Owner = owner;
        }

        public const int RACE_HUMAN = 0;
        public const int RACE_DAEVIE = 1;

        protected int mRace;
        protected NPCGender mGender;
        protected int mBody;
        protected int voice;
        protected float Scale;
        protected int statue;
        protected int ghost;

        public int Voice
        {
            get { return voice; }
        }

        public int Race
        {
            get { return mRace; }
        }

        public virtual NPCGender GetGender()
        {
            return mGender;
        }

        public bool GetStatue()
        {
            return statue > 0;
        }

        public void SetStatue(bool aOn)
        {
            if (aOn)
            {                                                         
                statue++;                                                                
            }
            else
            {
                if (statue > 0)
                {
                    statue--;
                }
            }
        }

        public bool GetGhost()
        {
            return ghost > 0;
        }

        public void SetGhost(bool aOn)
        {
            if (aOn)
            {                                                                 
                ghost++;                                                              
            }
            else
            {                                                                 
                if (ghost > 0)
                {
                    ghost--;
                }                                                            
            }
        }

    }
}
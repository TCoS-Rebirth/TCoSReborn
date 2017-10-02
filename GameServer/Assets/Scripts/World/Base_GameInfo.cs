using UnityEngine;

namespace World
{
    public class Base_GameInfo : ScriptableObject
    {
        public int HackFlags;
        public float mFixedRelativeTimeOfDay;
        public string /*class<Base_Controller>*/ mPlayerControllerClass;
        public string /*class<Base_Controller>*/ mTestBotControllerClass;
        public string /*class<Base_Controller>*/ mGameMasterControllerClass;
    }
}
namespace Database.Dynamic.Internal
{
    public class DBQuestTarget
    {
        public int ResourceId;
        public bool isCompleted;
        public int targetIndex;
        public int targetProgress;

        public DBQuestTarget(int resID, bool compl, int tarInd)
        {
            ResourceId = resID;
            if (compl)
            {
                isCompleted = true;
                targetIndex = -1;
                targetProgress = -1;
            }
            else {
                isCompleted = false;
                targetIndex = tarInd;
                targetProgress = 0;
            }
        }
    }
}
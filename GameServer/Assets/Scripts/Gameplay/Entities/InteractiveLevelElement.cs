using Common;

namespace Gameplay.Entities
{
    public class InteractiveLevelElement : Entity
    {
        ECollisionType collisionType;

        bool isEnabled;
        int levelObjectID;

        public int LevelObjectID
        {
            get { return levelObjectID; }
            set { levelObjectID = value; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public ECollisionType CollisionType
        {
            get { return collisionType; }
            set { collisionType = value; }
        }
    }
}
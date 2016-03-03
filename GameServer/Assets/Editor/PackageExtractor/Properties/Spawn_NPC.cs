using Common.UnrealTypes;
using UnityEngine;

namespace PackageExtractor.Serialization
{
    public struct PointRegion
    {
        public string Zone;
        public int iLeaf;
        public byte ZoneNumber;
    }


    public class Spawn_NPC : ISBSerializable
    {
        public string Level;
        public Vector3 Location;
        public string NPCType;
        public string PhysicsVolume;
        public PointRegion Region;
        public Rotator Rotation;
        public float SunlightsBrightness;
        public string Tag;
        public string Texture;

        public void Deserialize(SBFileReader reader)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Internal;
using UnityEngine;

namespace PackageExtractor
{
    public class PackageReadConfig : ScriptableObject
    {
        [SerializeField] List<SBClassInfo> classInfos = new List<SBClassInfo>();

        public string[] skippableObjects =
        {
            "Engine.TerrainInfo",
            "Engine.TerrainSector",
            "Engine.Emitter",
            "Engine.StaticMeshActor",
            "Engine.StaticMeshInstance",
            "Engine.Light",
            "Engine.SBSunlight",
            "Engine.Polys",
            "Engine.SpriteEmitter",
            "Engine.MeshEmitter",
            "Engine.Projector",
            "Engine.SBProjector",
            "Engine.Texture",
            "Engine.Model",
            "Engine.Camera",
            "SBGamePlay.TooltipActor",
            "Engine.SBAudioPlayer",
            "SBGamePlay.SBAudioPlayer",
            "SBGamePlay.SBAudioDamper",
            "SBEditor.GraphState"
        };

        public bool FindArrayType(string className, string propertyName, out PropertyType pType, out string insideName)
        {
            className = className.Replace("\0", string.Empty);
            propertyName = propertyName.Replace("\0", string.Empty);
            foreach (var classInfo in classInfos)
            {
                if (classInfo.className.Equals(className, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var info in classInfo.arrayTypeDefinitions)
                    {
                        if (info.arrayPropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            pType = info.type;
                            insideName = info.insideNameIfAvailable;
                            return true;
                        }
                    }
                }
            }
            pType = PropertyType.UnknownProperty;
            insideName = "";
            return false;
        }

        [ContextMenu("Sort definition array")]
        void SortArrayDefs()
        {
            classInfos = classInfos.OrderBy(x => x.className, new AlphanumComparer()).ToList();
        }

        [Serializable]
        public class SBClassInfo
        {
            public List<SBArrayTypeInfo> arrayTypeDefinitions = new List<SBArrayTypeInfo>();
            public string className = "";
        }

        [Serializable]
        public class SBArrayTypeInfo
        {
            public string arrayPropertyName = "";
            public string insideNameIfAvailable = "";
            public PropertyType type = PropertyType.UnknownProperty;
        }
    }
}
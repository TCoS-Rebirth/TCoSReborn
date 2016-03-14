using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay
{
    public class LevelProgression : ScriptableObject
    {
        [ReadOnly] public List<ProgressData> progressData = new List<ProgressData>();

        public ProgressData GetDataForLevel(int level)
        {
            if (level <= 100 && level >= 1)
            {
                return progressData[level - 1];
            }
            throw new ArgumentOutOfRangeException("level", "Level must be between 1 and 100");
        }

#if UNITY_EDITOR
        //[MenuItem("Spellborn/Parse LevelProgression File")]
        public static void ParseFile()
        {
            var path = EditorUtility.OpenFilePanel("Select LevelProgression file", "", "s");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("Path invalid");
                return;
            }
            var lp = CreateInstance<LevelProgression>();
            var savePath = EditorUtility.SaveFilePanel("Save parsed file", "", "LevelProgression", "asset");
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path invalid");
                return;
            }
            if (savePath.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
            {
                savePath = "Assets" + savePath.Substring(Application.dataPath.Length);
            }
            AssetDatabase.CreateAsset(lp, savePath);
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                reader.ReadInt32(); //header
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var pd = new ProgressData
                    {
                        level = reader.ReadByte(),
                        skillTier = reader.ReadByte(),
                        combatTierRows = reader.ReadByte(),
                        combatTierColumns = reader.ReadByte(),
                        totalSkills = reader.ReadByte(),
                        skillUpgrades = reader.ReadByte(),
                        statPoints = reader.ReadByte(),
                        bodySlots = reader.ReadByte(),
                        decks = reader.ReadByte(),
                        special = reader.ReadByte(),
                        requiredFamePoints = reader.ReadInt32(),
                        killFame = reader.ReadInt32(),
                        questFame = reader.ReadInt32()
                    };
                    lp.progressData.Add(pd);
                }
            }
            EditorUtility.SetDirty(lp);
        }
#endif
    }

    [Serializable]
    public class ProgressData
    {
        [ReadOnly] public int bodySlots;

        [ReadOnly] public int combatTierColumns;

        [ReadOnly] public int combatTierRows;

        [ReadOnly] public int decks;

        [ReadOnly] public int killFame;

        [ReadOnly] public int level;

        [ReadOnly] public int questFame;

        [ReadOnly] public int requiredFamePoints;

        [ReadOnly] public int skillTier;

        [ReadOnly] public int skillUpgrades;

        [ReadOnly] public int special;

        [ReadOnly] public int statPoints;

        [ReadOnly] public int totalSkills;
    }
}
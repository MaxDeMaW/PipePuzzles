using UnityEditor;
using UnityEngine;

namespace Pipes.Tests
{
    internal static class TestDifficultyProfiles
    {
        public static DifficultyProfile Create(int columns, int rows, int seed)
        {
            var profile = ScriptableObject.CreateInstance<DifficultyProfile>();
            var so = new SerializedObject(profile);
            so.FindProperty("_columns").intValue = columns;
            so.FindProperty("_rows").intValue = rows;
            so.FindProperty("_useFixedSeed").boolValue = true;
            so.FindProperty("_seed").intValue = seed;
            so.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        public static DifficultyProfile CreateWithWeights(
            int columns,
            int rows,
            int seed,
            params PipeSpawnWeight[] weights)
        {
            DifficultyProfile profile = Create(columns, rows, seed);
            var so = new SerializedObject(profile);
            SerializedProperty weightsProperty = so.FindProperty("_spawnWeights");
            weightsProperty.arraySize = weights.Length;
            for (int i = 0; i < weights.Length; i++)
            {
                SerializedProperty element = weightsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Type").enumValueIndex = (int)weights[i].Type;
                element.FindPropertyRelative("Weight").floatValue = weights[i].Weight;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }
    }
}

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pipes.Editor
{
    public static class WebGLPagesBuilder
    {
        private const string OutputFolder = "docs";

        [MenuItem("Build/WebGL for GitHub Pages")]
        public static void BuildFromMenu()
        {
            Build(exitEditor: false);
        }

        public static void BuildFromCommandLine()
        {
            Build(exitEditor: true);
        }

        private static void Build(bool exitEditor)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("No enabled scenes in Build Settings.");
                if (exitEditor)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string outputPath = Path.Combine(projectRoot, OutputFolder);

            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);
                Debug.Log($"WebGL build succeeded: {outputPath} ({summary.totalSize} bytes)");
                if (exitEditor)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"WebGL build failed: {summary.result}");
                if (exitEditor)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor
{
    public static class WebBuild
    {
        private const string DefaultOutputPath = "Build/WebGL";

        [MenuItem("Build/WebGL Build")]
        public static void PerformBuild()
        {
            try
            {
                BuildWebGL();

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }

        private static void BuildWebGL()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputPath = GetArgumentValue("-buildOutput", "--build-output") ?? DefaultOutputPath;
            string resolvedOutputPath = ResolvePath(projectRoot, outputPath);
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("WebGL build requires at least one enabled scene in EditorBuildSettings.");
            }

            Directory.CreateDirectory(resolvedOutputPath);

            BuildPlayerOptions buildOptions = new()
            {
                scenes = scenes,
                locationPathName = resolvedOutputPath,
                target = BuildTarget.WebGL,
                options = HasArgument("-development", "--development") ? BuildOptions.Development : BuildOptions.None
            };

            Debug.Log($"Starting WebGL build. Output: {resolvedOutputPath}. Scenes: {string.Join(", ", scenes)}. Options: {buildOptions.options}");

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build finished with result {summary.result}. Errors: {summary.totalErrors}, warnings: {summary.totalWarnings}.");
            }

            Debug.Log(
                $"WebGL build succeeded. Output: {resolvedOutputPath}. Duration: {summary.totalTime}. " +
                $"Size: {summary.totalSize} bytes. Warnings: {summary.totalWarnings}. Errors: {summary.totalErrors}.");
        }

        private static string ResolvePath(string projectRoot, string path)
        {
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(projectRoot, path));
        }

        private static string GetArgumentValue(params string[] names)
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int index = 0; index < args.Length - 1; index++)
            {
                if (names.Contains(args[index], StringComparer.Ordinal))
                {
                    return args[index + 1];
                }
            }

            return null;
        }

        private static bool HasArgument(params string[] names)
        {
            string[] args = Environment.GetCommandLineArgs();
            return args.Any(arg => names.Contains(arg, StringComparer.Ordinal));
        }
    }
}

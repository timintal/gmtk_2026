using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Code.Editor
{
    [InitializeOnLoad]
    public class AutoPlayModeSceneSetup
    {
        static AutoPlayModeSceneSetup()
        {
            EditorBuildSettings.sceneListChanged += SceneListChanged;
            SceneListChanged();
        }

        [MenuItem("Tools/Set Current Scene As Master", false, 0)]
        public static void SetAsFirstScene()
        {
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var scenePaths = editorBuildSettingsScenes.Select(i => i.path).ToList();

            //Add the scene to build settings if not already there; place as the first scene
            if (!scenePaths.Contains(SceneManager.GetActiveScene().path))
            {
                editorBuildSettingsScenes.Insert(0, new EditorBuildSettingsScene(SceneManager.GetActiveScene().path, true));
            }
            else
            {
                //Reference the current scene
                var scene = new EditorBuildSettingsScene(SceneManager.GetActiveScene().path, true);

                var index = -1;

                //Loop and find index from scene; we are doing this way cause IndexOf returns a -1 index for some reason
                for (var i = 0; i < editorBuildSettingsScenes.Count; i++)
                {
                    if (editorBuildSettingsScenes[i].path == scene.path)
                    {
                        index = i;
                    }
                }

                if (index != 0)
                {
                    //Remove from current index
                    editorBuildSettingsScenes.RemoveAt(index);

                    //Then place as first scene in build settings
                    editorBuildSettingsScenes.Insert(0, scene);
                }
            }

            //copy arrays back to build setting scenes
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

        private static void SceneListChanged()
        {
            // Ensure at least one build scene exist.
            if (EditorBuildSettings.scenes.Length == 0)
            {
                return;
            }

            //Reference the first scene
            var theScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);

            // Set Play Mode scene to first scene defined in build settings.
            EditorSceneManager.playModeStartScene = theScene;
        }
    }
}
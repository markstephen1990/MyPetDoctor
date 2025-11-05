using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class LevelAutoLoaded
    {
        static LevelAutoLoaded()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            RewriteStartScene();
        }

        private static void RewriteStartScene()
        {
            Level level = GameObject.FindFirstObjectByType<Level>();
            if(level != null)
            {
                LevelsDatabase levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
                if(levelsDatabase != null)
                {
                    int levelIndex = levelsDatabase.GetLevelIndexByName(level.gameObject.scene.name);
                    if(levelIndex != -1)
                    {
                        GlobalSave globalSave = SaveController.GetGlobalSave();
                        GlobalLevelSave globalLevelSaveData = globalSave.GetSaveObject<GlobalLevelSave>("globalLevelSave");
                        globalLevelSaveData.CurrentLevelID = levelIndex;

                        SaveController.SaveCustom(globalSave);
                    }
                }

                SceneAsset gameScene = EditorUtils.GetAsset<SceneAsset>("Game");
                if (gameScene != null)
                {
                    EditorSceneManager.playModeStartScene = gameScene;
                }

                return;
            }

            EditorSceneManager.playModeStartScene = null;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            RewriteStartScene();
        }
    }
}

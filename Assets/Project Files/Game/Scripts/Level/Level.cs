using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class Level : MonoBehaviour, ISceneSavingCallback
    {
        [SerializeField] Transform spawnPoint;

        [ReadOnly]
        [SerializeField] ExtraLevelBehavior[] levelBehaviors;
        public ExtraLevelBehavior[] LevelBehaviors => levelBehaviors;

        [ReadOnly]
        [SerializeField] Zone[] zones;
        public Zone[] Zones => zones;

        public void Start()
        {
            LevelController.OnLevelCreated(this);
        }

        public void OnLevelLoaded(LevelSave levelSave)
        {
            // Check if save is exists (should be null on first launch)
            if (levelSave != null && !levelSave.ZoneSaves.IsNullOrEmpty())
            {
                for (int i = 0; i < levelSave.ZoneSaves.Length; i++)
                {
                    for (int j = 0; j < zones.Length; j++)
                    {
                        if (zones[j].ID == levelSave.ZoneSaves[i].ID)
                        {
                            zones[j].Load(levelSave.ZoneSaves[i]);

                            break;
                        }
                    }
                }
            }

            levelSave.LinkZones(zones);
        }

        public void InitialisePlayer(PlayerBehavior playerBehavior)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].Initialise(playerBehavior);
            }

            for(int i = 0; i < levelBehaviors.Length; i++)
            {
                if (levelBehaviors[i] != null)
                    levelBehaviors[i].Initialise(this);
            }
        }

        public void OnGameLoaded()
        {
            for (int i = 0; i < levelBehaviors.Length; i++)
            {
                if (levelBehaviors[i] != null)
                    levelBehaviors[i].OnGameLoaded();
            }
        }

        public void RecalculateNurses()
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].RecalculateNurses();
            }
        }

        public void Unload()
        {
            if(!zones.IsNullOrEmpty())
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    zones[i].Unload();
                }
            }

            Currency[] currencies = CurrencyController.Currencies;
            foreach (Currency currency in currencies)
            {
                currency.Data.DestroyStackPool();
            }
        }

        public Vector3 GetSpawnPoint()
        {
            return spawnPoint.position;
        }

        public void OnSceneSaving()
        {
            zones = FindObjectsByType<Zone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            levelBehaviors = FindObjectsByType<ExtraLevelBehavior>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            RuntimeEditorUtils.SetDirty(this);
        }
    }
}
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class NurseZoneSettings
    {
        [SerializeField] NurseSettings[] nurseSettings;
        public NurseSettings[] NurseSettings => nurseSettings;

        [System.NonSerialized] int openedNurses;
        public int OpenedNurses => openedNurses;

        private Zone zone;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            // Spawn nurses
            for (int i = 0; i < openedNurses; i++)
            {
                zone.SpawnNurse();
            }
        }

        public void UnlockNurse()
        {
            openedNurses++;
        }

        public NurseSettings GetNextNurseSettings()
        {
            if (nurseSettings.IsInRange(openedNurses))
                return nurseSettings[openedNurses];

            return null;
        }

        public void Load(SaveData saveData)
        {
            if (saveData == null)
                return;

            openedNurses = saveData.OpenedNurses;
        }

        public SaveData Save()
        {
            return new SaveData(openedNurses);
        }

        [System.Serializable]
        public class SaveData
        {
            [SerializeField] int openedNurses;
            public int OpenedNurses => openedNurses;

            public SaveData(int openedNurses)
            {
                this.openedNurses = openedNurses;
            }
        }
    }
}
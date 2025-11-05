using UnityEngine;

namespace Watermelon
{
    public class NurseSpawner : MonoBehaviour
    {
        [SerializeField] Vector2 nurseIdleZone;

        [SerializeField] NurseSettings[] nurseSettings;
        public NurseSettings[] NurseSettings => nurseSettings;

        private int openedNurses;
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

        public Vector3 GetIdlePosition()
        {
            return transform.position + new Vector3(Random.Range(-nurseIdleZone.x, nurseIdleZone.x), 0, Random.Range(-nurseIdleZone.y, nurseIdleZone.y));
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

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(nurseIdleZone.x, 1, nurseIdleZone.y));
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
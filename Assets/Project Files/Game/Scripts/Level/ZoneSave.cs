using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ZoneSave
    {
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        [SerializeField] bool isOpened;
        public bool IsOpened => isOpened;

        [SerializeField] TableZoneBehaviour.SaveData[] tableZones;
        public TableZoneBehaviour.SaveData[] TableZones => tableZones;

        [SerializeField] NurseSpawner.SaveData nurseSettings;
        public NurseSpawner.SaveData NurseSettings => nurseSettings;

        public ZoneSave(string id, int placedCurrencyAmount, bool isOpened, TableZoneBehaviour.SaveData[] tableZones, NurseSpawner.SaveData nurseSettings)
        {
            this.id = id;
            this.placedCurrencyAmount = placedCurrencyAmount;
            this.isOpened = isOpened;
            this.tableZones = tableZones;
            this.nurseSettings = nurseSettings;
        }
    }
}
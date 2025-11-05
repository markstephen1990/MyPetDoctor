using System;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class GlobalLevelSave : ISaveObject
    {
        public int CurrentLevelID;

        public void Flush()
        {

        }
    }

    [Serializable]
    public class LevelSave : ISaveObject
    {
        public ZoneSave[] ZoneSaves;

        [NonSerialized] Zone[] activeZones;

        public LevelSave()
        {
            ZoneSaves = null;
        }

        public void LinkZones(Zone[] zones)
        {
            activeZones = zones;
        }

        public void Flush()
        {
            if (activeZones == null) return;

            // Get save data from all zones
            ZoneSaves = new ZoneSave[activeZones.Length];
            for (int i = 0; i < activeZones.Length; i++)
            {
                ZoneSaves[i] = activeZones[i].Save();
            }
        }

        public void Disable()
        {
            activeZones = null;
        }
    }
}
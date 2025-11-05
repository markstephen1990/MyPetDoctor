using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class AnimalSpawner : MonoBehaviour
    {
        private const float MIN_SPAWN_DURATION = 2.0f;
        private const float MAX_SPAWN_DURATION = 4.0f;

        private const int MAX_WAITING_ANIMALS = 7;

        [SerializeField] Transform spawnPoint;
        [SerializeField] float spawnRadius;

        [Space]
        [SerializeField] Vector3 spawnZoneCenter;
        [SerializeField] Vector3 spawnZoneSize;

        [Space]
        [SerializeField] Vector3 walkingZoneCenter;
        [SerializeField] Vector3 walkingZoneSize;

        [Space]
        [SerializeField] SpawnType[] spawnTypes;

        private Coroutine spawnCoroutine;

        private int spawnedVisitorsCount;
        private List<VisitorBehaviour> spawnedVisitors = new List<VisitorBehaviour>();

        private Zone zone;
        public Zone Zone => zone;

        private TweenCase delaySpawnCoroutine;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            StartAutoSpawn(3f);
        }

        public void DisableAutoSpawn()
        {
            if (delaySpawnCoroutine != null && !delaySpawnCoroutine.IsCompleted)
                delaySpawnCoroutine.Kill();

            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
        }

        public void StartAutoSpawn(float delay)
        {
            if (spawnCoroutine != null)
                return;

            delaySpawnCoroutine = Tween.DelayedCall(delay, delegate
            {
                spawnCoroutine = StartCoroutine(SpawnCoroutine());
            });
        }

        private IEnumerator SpawnCoroutine()
        {
            while (true)
            {
                if (zone.WaitingAnimalsAmount < zone.GetFreeTablesAmount() && zone.WaitingAnimalsAmount < MAX_WAITING_ANIMALS)
                {
                    SpawnVisitorWithRandomAnimal();
                }

                yield return new WaitForSeconds(Random.Range(MIN_SPAWN_DURATION, MAX_SPAWN_DURATION));
            }
        }

        public VisitorBehaviour SpawnVisitor()
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));

            VisitorBehaviour visitorBehaviour = LevelController.SpawnVisitor();
            visitorBehaviour.Initialise(this, spawnPoint.position + spawnOffset);

            return visitorBehaviour;
        }

        public VisitorBehaviour SpawnVisitorWithRandomAnimal(Animal.Type animalType, SicknessType sicknessType)
        {
            Animal animal = LevelController.GetAnimal(animalType);
            if (animal != null)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));

                GameObject animalObject = animal.Pool.GetPooledObject();
                animalObject.transform.ResetGlobal();

                AnimalBehaviour animalBehaviour = animalObject.GetComponent<AnimalBehaviour>();
                animalBehaviour.Initialise(animal, zone, this);
                animalBehaviour.SetSickness(LevelController.GetSickness(sicknessType));
                animalBehaviour.transform.localScale = Vector3.one;

                animalObject.SetActive(true);

                VisitorBehaviour visitorBehaviour = LevelController.SpawnVisitor();
                visitorBehaviour.Initialise(this, spawnPoint.position + spawnOffset);
                visitorBehaviour.CarryAnimal(animalBehaviour);

                Vector3 targetPoint = GetRandomPositionSpawnPosition();
                animalBehaviour.SetSpawnPoint(targetPoint);
                visitorBehaviour.SetPlacePosition(targetPoint);

                // Add animal to zone list
                zone.AddWaitingAnimal(animalBehaviour);

                spawnedVisitorsCount++;
                spawnedVisitors.Add(visitorBehaviour);

                visitorBehaviour.StartDelivering();

                return visitorBehaviour;
            }

            return null;
        }

        private SpawnType GetRandomWeighterAnimal()
        {
            int totalWeight = spawnTypes.Sum(x => x.Weight);
            int currentWeight = 0;
            float randomValue = Random.Range(0.0f, totalWeight);

            foreach (var item in spawnTypes)
            {
                currentWeight += item.Weight;

                if (currentWeight >= randomValue)
                {
                    return item;
                }
            }

            return spawnTypes[0];
        }

        private void SpawnVisitorWithRandomAnimal()
        {
            if (!NavMeshController.IsNavMeshCalculated)
                return;

            SpawnType spawnType = GetRandomWeighterAnimal();

            Animal animal = LevelController.GetAnimal(spawnType.AnimalType);
            if (animal != null)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));

                GameObject animalObject = animal.Pool.GetPooledObject();
                animalObject.transform.ResetGlobal();

                AnimalBehaviour animalBehaviour = animalObject.GetComponent<AnimalBehaviour>();
                animalBehaviour.Initialise(animal, zone, this);
                animalBehaviour.SetSickness(LevelController.GetSickness(spawnType.SicknessType));
                animalBehaviour.transform.localScale = Vector3.one;

                animalObject.SetActive(true);

                VisitorBehaviour visitorBehaviour = LevelController.SpawnVisitor();
                visitorBehaviour.Initialise(this, spawnPoint.position + spawnOffset);
                visitorBehaviour.CarryAnimal(animalBehaviour);

                Vector3 targetPoint = GetRandomPositionSpawnPosition();
                animalBehaviour.SetSpawnPoint(targetPoint);
                visitorBehaviour.SetPlacePosition(targetPoint);

                // Add animal to zone list
                zone.AddWaitingAnimal(animalBehaviour);

                spawnedVisitorsCount++;
                spawnedVisitors.Add(visitorBehaviour);

                visitorBehaviour.StartDelivering();
            }
        }

        public void OnVisitorDisabled(VisitorBehaviour visitorBehaviour)
        {
            int visitorIndex = spawnedVisitors.FindIndex(x => x == visitorBehaviour);
            if (visitorIndex != -1)
            {
                spawnedVisitors.RemoveAt(visitorIndex);
                spawnedVisitorsCount--;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + spawnZoneCenter, spawnZoneSize * 2);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + walkingZoneCenter, walkingZoneSize * 2);
        }

        public Vector3 GetRandomPositionSpawnPosition()
        {
            return transform.position + spawnZoneCenter + new Vector3(Random.Range(-spawnZoneSize.x, spawnZoneSize.x), 0, Random.Range(-spawnZoneSize.z, spawnZoneSize.z));
        }

        public Vector3 GetRandomPositionWalkPosition()
        {
            return transform.position + walkingZoneCenter + new Vector3(Random.Range(-walkingZoneSize.x, walkingZoneSize.x), 0, Random.Range(-walkingZoneSize.z, walkingZoneSize.z));
        }
    }
}
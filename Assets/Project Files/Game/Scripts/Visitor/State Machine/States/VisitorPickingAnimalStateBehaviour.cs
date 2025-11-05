using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class VisitorPickingAnimalStateBehaviour : VisitorStateBehaviour
    {
        private static readonly int HEART_PARTICLE_HASH = "Heart".GetHashCode();

        private const float PATH_RECALCULATION_DELAY = 0.3f;

        private AnimalBehaviour animalBehaviour;

        private NavMeshAgent navMeshAgent;
        private Transform transform;

        private float pathRecalculationTime;
        private bool isAnimalPicked = false;

        public VisitorPickingAnimalStateBehaviour(VisitorStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
            transform = stateMachineController.ParentBehaviour.transform;
        }

        public override void OnStateActivated()
        {
            if (animalBehaviour == null)
            {
                Debug.LogError("Animal isn't linked!");

                stateMachineController.SetState(VisitorStateMachineController.State.Waiting);

                return;
            }

            isAnimalPicked = false;

            RecalculatePath();
        }

        private void RecalculatePath()
        {
            stateMachineController.ParentBehaviour.SetTargetPosition(animalBehaviour.transform.position, delegate
            {
                isAnimalPicked = true;

                ParticlesController.PlayParticle(HEART_PARTICLE_HASH).SetPosition(transform.position + new Vector3(0, 4, 0));

                animalBehaviour.PickAnimal(stateMachineController.ParentBehaviour);
                animalBehaviour.OnAnimalCuredAndPicked();

                Tween.DelayedCall(0.5f, delegate
                {
                    stateMachineController.SetState(VisitorStateMachineController.State.Leaving);
                });
            });

            pathRecalculationTime = Time.time + PATH_RECALCULATION_DELAY;
        }

        public override void OnStateDisabled()
        {
            animalBehaviour = null;
        }

        public override void Update()
        {
            if (isAnimalPicked)
                return;

            if (Time.time > pathRecalculationTime)
                RecalculatePath();
        }

        public void SetAnimal(AnimalBehaviour animalBehaviour)
        {
            this.animalBehaviour = animalBehaviour;
        }

        public override void OnTriggerEnter(Collider other)
        {

        }

        public override void OnTriggerExit(Collider other)
        {

        }
    }
}
using Unity.AI.Navigation;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public static class NavMeshController
    {
        private static NavMeshSurface navMeshSurface;
        public static NavMeshSurface NavMeshSurface => navMeshSurface;

        private static bool isNavMeshCalculated;
        public static bool IsNavMeshCalculated => isNavMeshCalculated;

        public static event SimpleCallback OnNavMeshRecalculated;

        private static TweenCase navMeshTweenCase;
        private static bool navMeshRecalculating;

        public static void Initialise(NavMeshSurface navMeshSurface)
        {
            NavMeshController.navMeshSurface = navMeshSurface;
        }

        public static void CalculateNavMesh(SimpleCallback simpleCallback)
        {
            if (navMeshRecalculating)
                return;

            navMeshRecalculating = true;

            navMeshTweenCase = new NavMeshSurfaceTweenCase(navMeshSurface).OnComplete(delegate
            {
                isNavMeshCalculated = true;
                navMeshRecalculating = false;

                navMeshTweenCase = null;

                simpleCallback?.Invoke();

                OnNavMeshRecalculated?.Invoke();
                OnNavMeshRecalculated = null;
            }).StartTween();
        }

        public static void RecalculateNavMesh(SimpleCallback simpleCallback)
        {
            if (navMeshRecalculating)
                return;

            navMeshRecalculating = true;

            navMeshTweenCase = new NavMeshSurfaceTweenCase(navMeshSurface).OnComplete(delegate
            {
                navMeshRecalculating = false;

                simpleCallback?.Invoke();

                navMeshTweenCase = null;
            }).StartTween();
        }

        public static void InvokeOrSubscribe(SimpleCallback callback)
        {
            if (isNavMeshCalculated)
            {
                callback?.Invoke();
            }
            else
            {
                OnNavMeshRecalculated += callback;
            }
        }

        public static void Reset()
        {
            if (navMeshTweenCase != null)
            {
                navMeshTweenCase.Kill();
                navMeshTweenCase = null;
            }

            navMeshRecalculating = false;
            isNavMeshCalculated = false;

            OnNavMeshRecalculated = null;
        }
    }
}

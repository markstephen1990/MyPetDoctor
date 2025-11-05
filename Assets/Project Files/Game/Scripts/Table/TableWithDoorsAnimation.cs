using UnityEngine;

namespace Watermelon
{
    public class TableWithDoorsAnimation : TableCallbackReciever
    {
        [SerializeField] Transform doorTransform;

        public override void OnAnimalCured(SimpleCallback animalCureCallback)
        {
            animalCureCallback?.Invoke();

            if (doorTransform != null)
            {
                Tween.DelayedCall(0.5f, delegate
                {
                    doorTransform.DOLocalRotate(Quaternion.Euler(0, 140, 0), 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                    {
                        Tween.DelayedCall(1.0f, delegate
                        {
                            doorTransform.DOLocalRotate(Quaternion.identity, 0.2f).SetEasing(Ease.Type.CubicIn);
                        });
                    });
                });
            }
        }
    }
}

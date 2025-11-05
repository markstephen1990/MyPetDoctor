using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class MoneyFloatingTextBehavior : FloatingTextBaseBehavior
    {
        [SerializeField] TMP_Text floatingText;
        [SerializeField] Image iconImage;

        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] AnimationCurve scaleAnimationCurve;

        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public void SetIcon(Sprite sprite)
        {
            if(sprite != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = sprite; 
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        public override void Activate(string text, float scale, Color color)
        {
            floatingText.text = text;

            transform.localScale = Vector3.zero;
            transform.DOScale(defaultScale * scale, scaleTime).SetCurveEasing(scaleAnimationCurve);

            transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}
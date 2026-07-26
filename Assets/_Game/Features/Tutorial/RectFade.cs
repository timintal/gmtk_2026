using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Tutorial
{
    public class RectFade : MonoBehaviour
    {
        [SerializeField] RectTransform _left;
        [SerializeField] RectTransform _right;
        [SerializeField] RectTransform _top;
        [SerializeField] RectTransform _bottom;

        [Button]
        public void SetRect(Vector2 position, Vector2 size)
        {
            transform.GetComponent<RectTransform>().anchoredPosition = position;
            _left.anchoredPosition = new Vector2(-size.x / 2, 0);
            _right.anchoredPosition = new Vector2(size.x / 2, 0);
            _top.anchoredPosition = new Vector2(0, size.y / 2);
            _bottom.anchoredPosition = new Vector2(0, -size.y / 2);
            _top.sizeDelta = new Vector2(size.x, _top.sizeDelta.y);
            _bottom.sizeDelta = new Vector2(size.x, _bottom.sizeDelta.y);
        }
        [Button]
        public void SetRect(RectTransform rectTransform)
        {
            SetRect(rectTransform.anchoredPosition, rectTransform.sizeDelta);
        }
    }
}
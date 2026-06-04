using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Cards
{
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutElement))]
    public sealed class ShadowedTextLayoutBinder : MonoBehaviour
    {
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text shadowText;
        [SerializeField] private Vector2 shadowOffset = new(0f, -4f);
        [SerializeField] private float extraWidthPadding = 0f;
        [SerializeField] private float extraHeightPadding = 0f;
        [SerializeField] private bool syncShadowText = true;

        private LayoutElement layoutElement;
        private string lastText;
        private float lastWidth;
        private float lastHeight;

        private void OnEnable()
        {
            Cache();
            Apply(true);
        }

        private void OnValidate()
        {
            Cache();
            Apply(true);
        }

        private void LateUpdate()
        {
            Apply(false);
        }

        public void SetText(string value)
        {
            if (mainText == null)
            {
                return;
            }

            mainText.text = value;
            Apply(true);
        }

        private void Cache()
        {
            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
            }
        }

        private void Apply(bool force)
        {
            if (mainText == null || shadowText == null)
            {
                return;
            }

            Cache();

            if (syncShadowText && shadowText.text != mainText.text)
            {
                shadowText.text = mainText.text;
            }

            // Using GetPreferredValues for measurements
            Vector2 preferredSize = mainText.GetPreferredValues(mainText.text);

            float width = Mathf.Ceil(preferredSize.x + extraWidthPadding);
            float height = Mathf.Ceil(
                Mathf.Max(preferredSize.y, shadowText.GetPreferredValues(shadowText.text).y)
                + Mathf.Abs(shadowOffset.y)
                + extraHeightPadding
            );

            if (!force &&
                mainText.text == lastText &&
                Mathf.Approximately(width, lastWidth) &&
                Mathf.Approximately(height, lastHeight))
            {
                return;
            }

            lastText = mainText.text;
            lastWidth = width;
            lastHeight = height;

            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            ConfigureTextRect(mainText.rectTransform, width, height, Vector2.zero);
            ConfigureTextRect(shadowText.rectTransform, width, height, shadowOffset);

            // Enforce explicit render order
            shadowText.transform.SetAsFirstSibling();
            mainText.transform.SetAsLastSibling();

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
            }
        }

        private static void ConfigureTextRect(RectTransform rect, float width, float height, Vector2 anchoredPosition)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one;
        }
    }
}

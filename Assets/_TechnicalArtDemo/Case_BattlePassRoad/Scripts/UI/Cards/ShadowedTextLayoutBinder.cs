using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TechnicalArtDemo.BattlePass.UI.Cards
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LayoutElement))]
    public sealed class ShadowedTextLayoutBinder : MonoBehaviour
    {
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text shadowText;
        [SerializeField] private Vector2 shadowOffset = new(0f, -4f);
        [SerializeField] private Vector2 padding;

        private LayoutElement layoutElement;

        private string lastText;
        private Vector2 lastSize;
        private Vector2 lastShadowOffset;
        private Vector2 lastPadding;

#if UNITY_EDITOR
        private bool refreshQueued;
        private bool forceQueuedRefresh;
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorRefresh(true);
                return;
            }
#endif

            Refresh(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            QueueEditorRefresh(true);
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Refresh(false);
        }

        private void QueueEditorRefresh(bool force)
        {
            forceQueuedRefresh |= force;

            if (refreshQueued)
            {
                return;
            }

            refreshQueued = true;

            EditorApplication.delayCall += () =>
            {
                if (this == null)
                {
                    return;
                }

                bool shouldForce = forceQueuedRefresh;
                refreshQueued = false;
                forceQueuedRefresh = false;

                Refresh(shouldForce);
            };
        }
#endif

        [ContextMenu("Refresh Layout")]
        public void RefreshLayout()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorRefresh(true);
                return;
            }
#endif

            Refresh(true);
        }

        public void SetText(string value)
        {
            if (mainText == null)
            {
                return;
            }

            mainText.text = value;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorRefresh(true);
                return;
            }
#endif

            Refresh(true);
        }

        private void Refresh(bool force)
        {
            if (mainText == null || shadowText == null)
            {
                return;
            }

            layoutElement ??= GetComponent<LayoutElement>();

            if (shadowText.text != mainText.text)
            {
                shadowText.text = mainText.text;
                force = true;
            }

            Vector2 preferred = mainText.GetPreferredValues(mainText.text);
            float width = Mathf.Ceil(preferred.x + padding.x);
            float height = Mathf.Ceil(preferred.y + Mathf.Abs(shadowOffset.y) + padding.y);
            Vector2 size = new(width, height);

            bool renderOrderChanged =
                shadowText.transform.GetSiblingIndex() > mainText.transform.GetSiblingIndex();

            if (!force &&
                mainText.text == lastText &&
                size == lastSize &&
                shadowOffset == lastShadowOffset &&
                padding == lastPadding &&
                !renderOrderChanged)
            {
                return;
            }

            lastText = mainText.text;
            lastSize = size;
            lastShadowOffset = shadowOffset;
            lastPadding = padding;

            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            ConfigureText(mainText.rectTransform, size, Vector2.zero);
            ConfigureText(shadowText.rectTransform, size, shadowOffset);

            shadowText.transform.SetAsFirstSibling();
            mainText.transform.SetAsLastSibling();

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);

            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
            }
        }

        private static void ConfigureText(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;
        }
    }
}
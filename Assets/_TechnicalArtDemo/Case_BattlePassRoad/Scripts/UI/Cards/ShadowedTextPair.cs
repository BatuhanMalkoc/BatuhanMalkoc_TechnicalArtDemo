using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TechnicalArtDemo.BattlePass.UI.Cards
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ShadowedTextPair : MonoBehaviour
    {
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text shadowText;
        [SerializeField] private bool syncShadowText = true;
        [SerializeField] private bool enforceRenderOrder = true;

        private string lastMainText;
        private bool lastSyncShadowText;
        private bool lastEnforceRenderOrder;

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

            Refresh(true, allowHierarchyChanges: true);
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

            Refresh(false, allowHierarchyChanges: true);
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

                Refresh(shouldForce, allowHierarchyChanges: true);
            };
        }
#endif

        [ContextMenu("Refresh")]
        public void RefreshNow()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorRefresh(true);
                return;
            }
#endif

            Refresh(true, allowHierarchyChanges: true);
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

            Refresh(true, allowHierarchyChanges: true);
        }

        private void Refresh(bool force, bool allowHierarchyChanges)
        {
            if (mainText == null || shadowText == null)
            {
                return;
            }

            string currentText = mainText.text;

            bool shadowTextOutOfSync =
                syncShadowText &&
                shadowText.text != currentText;

            bool renderOrderWrong =
                enforceRenderOrder &&
                shadowText.transform.GetSiblingIndex() > mainText.transform.GetSiblingIndex();

            bool changed =
                force ||
                currentText != lastMainText ||
                syncShadowText != lastSyncShadowText ||
                enforceRenderOrder != lastEnforceRenderOrder ||
                shadowTextOutOfSync ||
                renderOrderWrong;

            if (!changed)
            {
                return;
            }

            if (shadowTextOutOfSync)
            {
                shadowText.text = currentText;
            }

            if (enforceRenderOrder && allowHierarchyChanges)
            {
                shadowText.transform.SetAsFirstSibling();
                mainText.transform.SetAsLastSibling();
            }

            lastMainText = currentText;
            lastSyncShadowText = syncShadowText;
            lastEnforceRenderOrder = enforceRenderOrder;
        }
    }
}
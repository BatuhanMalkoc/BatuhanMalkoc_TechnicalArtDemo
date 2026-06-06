using TechnicalArtDemo.BattlePass.Runtime;
using TechnicalArtDemo.BattlePass.UI.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Road
{
    [DisallowMultipleComponent]
    public sealed class TierNodeView : MonoBehaviour
    {
        [Header("State Visuals")]
        [SerializeField] private GameObject notReachedVisual;
        [SerializeField] private GameObject reachedVisual;

        [Header("Content")]
        [SerializeField] private GameObject numberGroup;
        [SerializeField] private ShadowedTextPair numberText;
        [SerializeField] private GameObject specialIconRoot;
        [SerializeField] private Image specialIconImage;

        public void Bind(int tierNumber, BattlePassTierNodeState state)
        {
            Bind(tierNumber, state, useSpecialIcon: false, specialIcon: null);
        }

        public void Bind(
            int tierNumber,
            BattlePassTierNodeState state,
            bool useSpecialIcon,
            Sprite specialIcon)
        {
            BindState(state);
            BindContent(tierNumber, useSpecialIcon, specialIcon);
        }

        private void BindState(BattlePassTierNodeState state)
        {
            bool isReached = state == BattlePassTierNodeState.Reached;

            SetActive(notReachedVisual, !isReached);
            SetActive(reachedVisual, isReached);
        }

        private void BindContent(
            int tierNumber,
            bool useSpecialIcon,
            Sprite specialIcon)
        {
            bool showSpecialIcon = useSpecialIcon && specialIcon != null;

            SetActive(numberGroup, !showSpecialIcon);
            SetActive(specialIconRoot, showSpecialIcon);

            if (numberText != null)
            {
                numberText.SetText(tierNumber.ToString());
            }

            if (specialIconImage != null)
            {
                specialIconImage.sprite = showSpecialIcon ? specialIcon : null;
                specialIconImage.enabled = showSpecialIcon;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

#if UNITY_EDITOR
        [Header("Editor Preview")]
        [SerializeField] private int previewTierNumber = 1;
        [SerializeField] private BattlePassTierNodeState previewState;
        [SerializeField] private bool previewUseSpecialIcon;
        [SerializeField] private Sprite previewSpecialIcon;

        [ContextMenu("Preview Bind")]
        private void PreviewBind()
        {
            Bind(
                previewTierNumber,
                previewState,
                previewUseSpecialIcon,
                previewSpecialIcon);
        }
#endif
    }
}
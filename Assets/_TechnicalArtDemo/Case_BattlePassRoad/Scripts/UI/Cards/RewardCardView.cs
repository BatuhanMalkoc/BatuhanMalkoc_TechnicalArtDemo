using TechnicalArtDemo.BattlePass.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Cards
{
    [DisallowMultipleComponent]
    public sealed class RewardCardView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private ShadowedTextPair titleText;

        [Header("Footer - Action Label")]
        [SerializeField] private GameObject actionLabelGroup;
        [SerializeField] private ShadowedTextPair actionLabelText;

        [Header("Footer - Icon Value")]
        [SerializeField] private GameObject iconValueGroup;
        [SerializeField] private Image valueIconImage;
        [SerializeField] private ShadowedTextLayoutBinder valueTextBinder;

        [Header("Badges")]
        [SerializeField] private GameObject lockBadge;
        [SerializeField] private GameObject alertBadge;

        [Header("States")]
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject claimedOverlay;
        [SerializeField] private GameObject claimableGlow;
        [SerializeField] private GameObject selectedOutline;

        public void Bind(RewardCardData data, RewardCardState state, bool isSelected)
        {
            if (data == null)
            {
                return;
            }

            BindContent(data);
            BindFooter(data);
            BindState(state);
            BindSelection(isSelected);
        }

        private void BindContent(RewardCardData data)
        {
            if (data.Style != null && cardBackgroundImage != null)
            {
                cardBackgroundImage.sprite = data.Style.CardBackgroundSprite;
            }

            if (rewardIconImage != null)
            {
                rewardIconImage.sprite = data.RewardIcon;
            }

            if (titleText != null)
            {
                titleText.SetText(data.DisplayTitle);
            }
        }

        private void BindFooter(RewardCardData data)
        {
            switch (data.ResolvedFooterMode)
            {
                case RewardFooterMode.ActionLabel:
                    SetActive(actionLabelGroup, true);
                    SetActive(iconValueGroup, false);

                    if (actionLabelText != null)
                    {
                        actionLabelText.SetText(data.ActionLabelText);
                    }

                    break;

                case RewardFooterMode.IconValue:
                    SetActive(actionLabelGroup, false);
                    SetActive(iconValueGroup, true);

                    if (valueIconImage != null)
                    {
                        valueIconImage.sprite = data.ValueIcon;
                    }

                    if (valueTextBinder != null)
                    {
                        valueTextBinder.SetText(data.ValueText);
                    }

                    break;
            }
        }

        private void BindState(RewardCardState state)
        {
            SetActive(lockBadge, state == RewardCardState.Locked);
            SetActive(alertBadge, state == RewardCardState.Claimable);
            SetActive(lockedOverlay, state == RewardCardState.Locked);
            SetActive(claimedOverlay, state == RewardCardState.Claimed);
            SetActive(claimableGlow, state == RewardCardState.Claimable);
        }

        private void BindSelection(bool isSelected)
        {
            SetActive(selectedOutline, isSelected);
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
        [SerializeField] private RewardCardData previewData;
        [SerializeField] private RewardCardState previewState;
        [SerializeField] private bool previewSelected;

        [ContextMenu("Preview Bind")]
        private void PreviewBind()
        {
            Bind(previewData, previewState, previewSelected);
        }
#endif
    }
}

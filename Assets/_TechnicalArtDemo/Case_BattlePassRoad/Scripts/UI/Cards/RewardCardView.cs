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
        [SerializeField] private GameObject claimedCheckBadge;

        [Header("States")]
        [SerializeField] private GameObject claimedOverlay;
        [SerializeField] private GameObject claimableGlow;
        [SerializeField] private GameObject selectedOutline;

        [Header("State Styles")]
        [SerializeField] private RewardCardStyle claimableStyle;
        [SerializeField] private RewardCardStyle claimedStyle;

        public void Bind(RewardCardData data, RewardCardProgressState progressState, bool isSelected)
        {
            Bind(data, progressState, RewardCardAccessState.Available, isSelected);
        }

        public void Bind(
            RewardCardData data,
            RewardCardProgressState progressState,
            RewardCardAccessState accessState,
            bool isSelected)
        {
            if (data == null)
            {
                return;
            }

            BindContent(data, progressState);
            BindFooter(data);
            BindState(progressState, accessState);
            BindSelection(isSelected);
        }

        private void BindContent(RewardCardData data, RewardCardProgressState progressState)
        {
            if (cardBackgroundImage != null)
            {
                RewardCardStyle resolvedStyle = ResolveBackgroundStyle(data, progressState);
                if (resolvedStyle != null)
                {
                    cardBackgroundImage.sprite = resolvedStyle.CardBackgroundSprite;
                }
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

        private RewardCardStyle ResolveBackgroundStyle(RewardCardData data, RewardCardProgressState progressState)
        {
            switch (progressState)
            {
                case RewardCardProgressState.Reached:
                    return claimableStyle != null ? claimableStyle : data.Style;

                case RewardCardProgressState.Claimed:
                    return claimedStyle != null ? claimedStyle : data.Style;

                case RewardCardProgressState.NotReached:
                default:
                    return data.Style;
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

                case RewardFooterMode.None:
                default:
                    SetActive(actionLabelGroup, false);
                    SetActive(iconValueGroup, false);
                    break;
            }
        }

        private void BindState(RewardCardProgressState progressState, RewardCardAccessState accessState)
        {
            bool isClaimed = progressState == RewardCardProgressState.Claimed;
            bool isPremiumLocked = accessState == RewardCardAccessState.PremiumLocked;
            bool canClaimNow = progressState == RewardCardProgressState.Reached && !isPremiumLocked;

            SetActive(lockBadge, !isClaimed && isPremiumLocked);
            SetActive(alertBadge, canClaimNow);
            SetActive(claimedCheckBadge, isClaimed);

            SetActive(claimedOverlay, false);
            SetActive(claimableGlow, canClaimNow);
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
        [SerializeField] private RewardCardProgressState previewProgressState;
        [SerializeField] private RewardCardAccessState previewAccessState;
        [SerializeField] private bool previewSelected;

        [ContextMenu("Preview Bind")]
        private void PreviewBind()
        {
            Bind(previewData, previewProgressState, previewAccessState, previewSelected);
        }
#endif
    }
}
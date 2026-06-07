using System;
using TechnicalArtDemo.BattlePass.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Cards
{
    [DisallowMultipleComponent]
    public sealed class RewardCardView : MonoBehaviour
    {
        [Header("Battle Pass")]
        [SerializeField] private RewardCardData rewardData;
        [SerializeField, Min(0)] private int tierIndex;
        [SerializeField] private bool premiumReward;
        [SerializeField] private bool claimed;

        [Header("Interaction")]
        [SerializeField] private Button hitButton;

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

        public event Action<RewardCardView> Clicked;

        public int TierIndex => tierIndex;
        public bool IsPremiumReward => premiumReward;
        public bool IsClaimed => claimed;
        public RewardCardData RewardData => rewardData;

        private void Awake()
        {
            CacheInteraction();
        }

        private void OnEnable()
        {
            CacheInteraction();

            if (hitButton != null)
            {
                hitButton.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            if (hitButton != null)
            {
                hitButton.onClick.RemoveListener(HandleClick);
            }
        }

        public void BindForBattlePass(int currentLevel, bool premiumOwned)
        {
            if (rewardData == null)
            {
                RefreshInteractable(false);
                return;
            }

            RewardCardProgressState progressState = ResolveProgressState(currentLevel);
            RewardCardAccessState accessState = ResolveAccessState(premiumOwned);

            Bind(rewardData, progressState, accessState, isSelected: false);
            RefreshInteractable(CanClaim(currentLevel, premiumOwned));
        }

        public bool CanClaim(int currentLevel, bool premiumOwned)
        {
            return
                rewardData != null &&
                !claimed &&
                tierIndex <= currentLevel &&
                (!premiumReward || premiumOwned);
        }

        public bool TryClaim(int currentLevel, bool premiumOwned)
        {
            if (!CanClaim(currentLevel, premiumOwned))
            {
                return false;
            }

            claimed = true;
            BindForBattlePass(currentLevel, premiumOwned);
            return true;
        }

        public void SetClaimed(bool value, int currentLevel, bool premiumOwned)
        {
            if (claimed == value)
            {
                return;
            }

            claimed = value;
            BindForBattlePass(currentLevel, premiumOwned);
        }

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

        private RewardCardProgressState ResolveProgressState(int currentLevel)
        {
            if (claimed)
            {
                return RewardCardProgressState.Claimed;
            }

            return tierIndex <= currentLevel
                ? RewardCardProgressState.Reached
                : RewardCardProgressState.NotReached;
        }

        private RewardCardAccessState ResolveAccessState(bool premiumOwned)
        {
            return premiumReward && !premiumOwned
                ? RewardCardAccessState.PremiumLocked
                : RewardCardAccessState.Available;
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

        private RewardCardStyle ResolveBackgroundStyle(
            RewardCardData data,
            RewardCardProgressState progressState)
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

        private void BindState(
            RewardCardProgressState progressState,
            RewardCardAccessState accessState)
        {
            bool isReached = progressState == RewardCardProgressState.Reached;
            bool isClaimed = progressState == RewardCardProgressState.Claimed;
            bool isPremiumLocked = accessState == RewardCardAccessState.PremiumLocked;

            bool showPremiumLock = !isClaimed && isPremiumLocked;
            bool showReadyAlert = !isClaimed && isReached;
            bool showClaimableGlow = isReached && !isPremiumLocked;

            SetActive(lockBadge, showPremiumLock);
            SetActive(alertBadge, showReadyAlert);
            SetActive(claimedCheckBadge, isClaimed);

            SetActive(claimedOverlay, false);
            SetActive(claimableGlow, showClaimableGlow);
        }

        private void BindSelection(bool isSelected)
        {
            SetActive(selectedOutline, isSelected);
        }

        private void CacheInteraction()
        {
            if (hitButton == null)
            {
                hitButton = GetComponentInChildren<Button>(true);
            }
        }

        private void RefreshInteractable(bool canInteract)
        {
            if (hitButton != null)
            {
                hitButton.interactable = canInteract;
            }
        }

        private void HandleClick()
        {
            Clicked?.Invoke(this);
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
        [SerializeField] private RewardCardProgressState previewProgressState;
        [SerializeField] private RewardCardAccessState previewAccessState;
        [SerializeField] private bool previewSelected;

        private void Reset()
        {
            CacheInteraction();
        }

        private void OnValidate()
        {
            CacheInteraction();
        }

        [ContextMenu("Preview Bind")]
        private void PreviewBind()
        {
            Bind(rewardData, previewProgressState, previewAccessState, previewSelected);
        }

        [ContextMenu("Preview Battle Pass State")]
        private void PreviewBattlePassState()
        {
            BindForBattlePass(tierIndex, premiumOwned: !premiumReward);
        }
#endif
    }
}
using UnityEngine;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    [CreateAssetMenu(fileName = "Reward_", menuName = "Battle Pass/Reward Card Data")]
    public sealed class RewardCardData : ScriptableObject
    {
        [Header("Reward")]
        [SerializeField] private RewardItemData item;
        [SerializeField] private RewardCardStyle style;

        [Header("Overrides")]
        [SerializeField] private string titleOverride;
        [SerializeField] private Sprite rewardIconOverride;
        [SerializeField] private Sprite valueIconOverride;

        [Header("Footer")]
        [SerializeField] private bool useItemDefaultFooterMode = true;
        [SerializeField] private RewardFooterMode footerMode = RewardFooterMode.IconValue;
        [SerializeField] private string actionLabelText;
        [SerializeField] private string valueText;

        public RewardItemData Item => item;
        public RewardCardStyle Style => style;
        public string ActionLabelText => actionLabelText;
        public string ValueText => valueText;

        public RewardFooterMode ResolvedFooterMode =>
            useItemDefaultFooterMode && item != null
                ? item.DefaultFooterMode
                : footerMode;

        public string DisplayTitle =>
            !string.IsNullOrWhiteSpace(titleOverride)
                ? titleOverride
                : item != null
                    ? item.DisplayName
                    : string.Empty;

        public Sprite RewardIcon =>
            rewardIconOverride != null
                ? rewardIconOverride
                : item != null
                    ? item.RewardIcon
                    : null;

        public Sprite ValueIcon =>
            valueIconOverride != null
                ? valueIconOverride
                : item != null
                    ? item.ValueIcon
                    : null;
    }
}

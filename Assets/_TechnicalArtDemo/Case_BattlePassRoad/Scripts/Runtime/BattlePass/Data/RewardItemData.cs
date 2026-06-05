using UnityEngine;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    [CreateAssetMenu(fileName = "Item_", menuName = "Battle Pass/Reward Item")]
    public sealed class RewardItemData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Sprite rewardIcon;
        [SerializeField] private Sprite valueIcon;
        [SerializeField] private RewardFooterMode defaultFooterMode = RewardFooterMode.IconValue;

        public string DisplayName => displayName;
        public Sprite RewardIcon => rewardIcon;
        public Sprite ValueIcon => valueIcon;
        public RewardFooterMode DefaultFooterMode => defaultFooterMode;
    }
}

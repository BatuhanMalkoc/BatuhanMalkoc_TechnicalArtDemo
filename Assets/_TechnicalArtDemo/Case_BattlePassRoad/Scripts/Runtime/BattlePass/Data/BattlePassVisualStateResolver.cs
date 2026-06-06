namespace TechnicalArtDemo.BattlePass.Runtime
{
    public static class BattlePassVisualStateResolver
    {
        public static RewardCardProgressState ResolveRewardProgressState(
            int rewardTier,
            int currentTier,
            bool isClaimed)
        {
            if (isClaimed)
            {
                return RewardCardProgressState.Claimed;
            }

            return rewardTier <= currentTier
                ? RewardCardProgressState.Reached
                : RewardCardProgressState.NotReached;
        }

        public static RewardCardAccessState ResolveRewardAccessState(
            RewardPassLane passLane,
            bool ownsPremiumPass)
        {
            return passLane == RewardPassLane.Premium && !ownsPremiumPass
                ? RewardCardAccessState.PremiumLocked
                : RewardCardAccessState.Available;
        }

        public static BattlePassTierNodeState ResolveTierNodeState(
            int tier,
            int currentTier)
        {
            return tier <= currentTier
                ? BattlePassTierNodeState.Reached
                : BattlePassTierNodeState.NotReached;
        }
    }
}
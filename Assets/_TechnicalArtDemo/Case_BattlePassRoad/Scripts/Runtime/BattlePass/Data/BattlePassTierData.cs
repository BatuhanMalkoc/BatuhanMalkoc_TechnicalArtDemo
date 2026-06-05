using System;
using UnityEngine;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    [Serializable]
    public struct BattlePassTierData
    {
        [SerializeField] private int tier;
        [SerializeField] private RewardCardData freeReward;
        [SerializeField] private RewardCardData premiumReward;

        public int Tier => tier;
        public RewardCardData FreeReward => freeReward;
        public RewardCardData PremiumReward => premiumReward;
    }
}

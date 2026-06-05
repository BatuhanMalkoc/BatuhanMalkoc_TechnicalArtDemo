using System.Collections.Generic;
using UnityEngine;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    [CreateAssetMenu(fileName = "Road_", menuName = "Battle Pass/Road Data")]
    public sealed class BattlePassRoadData : ScriptableObject
    {
        [SerializeField] private string roadName;
        [SerializeField] private List<BattlePassTierData> tiers;

        public string RoadName => roadName;
        public IReadOnlyList<BattlePassTierData> Tiers => tiers;
    }
}

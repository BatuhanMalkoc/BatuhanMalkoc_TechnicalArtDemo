using UnityEngine;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    [CreateAssetMenu(fileName = "Style_", menuName = "Battle Pass/Reward Card Style")]
    public sealed class RewardCardStyle : ScriptableObject
    {
        [SerializeField] private string styleName;
        [SerializeField] private Sprite cardBackgroundSprite;

        public string StyleName => styleName;
        public Sprite CardBackgroundSprite => cardBackgroundSprite;
    }
}

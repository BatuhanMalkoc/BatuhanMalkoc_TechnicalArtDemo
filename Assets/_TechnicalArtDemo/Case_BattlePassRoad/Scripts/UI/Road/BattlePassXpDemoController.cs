using System.Collections;
using TechnicalArtDemo.BattlePass.Runtime;
using TechnicalArtDemo.BattlePass.UI.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Road
{
    [DisallowMultipleComponent]
    public sealed class BattlePassXpDemoController : MonoBehaviour
    {
        private const int InvalidCachedInt = -1;

        [Header("Demo Progress")]
        [SerializeField, Min(1)] private int maxLevel = 50;
        [SerializeField, Min(1)] private int requiredXpPerLevel = 400;
        [SerializeField, Min(0)] private int startingLevel;
        [SerializeField, Min(0)] private int startingXp = 120;
        [SerializeField, Min(1)] private int xpPerTap = 120;

        [Header("Actions")]
        [SerializeField] private Button addXpButton;
        [SerializeField] private Button unlockPremiumButton;

        [Header("HUD")]
        [SerializeField] private Image xpBarFill;
        [SerializeField] private ShadowedTextPair xpText;
        [SerializeField] private TierNodeView currentLevelBadge;

        [Header("Road Progress")]
        [SerializeField] private RectTransform reachedRailFill;
        [SerializeField] private RectTransform roadProgressEdgeRoot;
        [SerializeField] private RectTransform currentProgressLineRoot;

        [Header("Road Layout")]
        [SerializeField] private float progressStartX = 2200f;
        [SerializeField] private float tierSpacing = 400f;
        [SerializeField] private float fillOriginX;

        [Header("Road Motion")]
        [SerializeField, Min(0f)] private float roadMoveDuration = 0.35f;
        [SerializeField, Range(0f, 2f)] private float roadOvershoot = 0.75f;

        [Header("Tier Nodes")]
        [SerializeField] private Transform tierNodesRoot;
        [SerializeField] private bool includeInactiveTierNodes = true;
        [SerializeField] private int firstNodeTierIndex;
        [SerializeField] private int nodeDisplayNumberOffset;

        [Header("Reward Cards")]
        [SerializeField] private Transform premiumRewardCardsRoot;
        [SerializeField] private Transform freeRewardCardsRoot;
        [SerializeField] private bool includeInactiveRewardCards = true;
        [SerializeField] private bool premiumOwned;

        private BattlePassProgressModel progressModel;

        private TierNodeView[] tierNodes;
        private RewardCardView[] premiumRewardCards;
        private RewardCardView[] freeRewardCards;

        private Coroutine roadMoveRoutine;

        private int cachedLevel = InvalidCachedInt;
        private int cachedXp = InvalidCachedInt;
        private int cachedRequiredXp = InvalidCachedInt;

        private float cachedXpFill = -1f;
        private float cachedRoadX;
        private bool hasRoadPosition;
        private bool rewardCardsSubscribed;

        private void Awake()
        {
            CollectTierNodes();
            CollectRewardCards();

            progressModel = new BattlePassProgressModel(
                maxLevel,
                requiredXpPerLevel,
                startingLevel,
                startingXp);

            progressModel.Changed += HandleProgressChanged;
        }

        private void OnEnable()
        {
            if (addXpButton != null)
            {
                addXpButton.onClick.AddListener(AddDemoXp);
            }

            if (unlockPremiumButton != null)
            {
                unlockPremiumButton.onClick.AddListener(UnlockPremiumPass);
            }

            SubscribeRewardCards();
            RefreshXpUi();
            RefreshPremiumButton();
        }

        private void OnDisable()
        {
            if (addXpButton != null)
            {
                addXpButton.onClick.RemoveListener(AddDemoXp);
            }

            if (unlockPremiumButton != null)
            {
                unlockPremiumButton.onClick.RemoveListener(UnlockPremiumPass);
            }

            UnsubscribeRewardCards();
            StopRoadAnimation();
        }

        private void OnDestroy()
        {
            if (progressModel != null)
            {
                progressModel.Changed -= HandleProgressChanged;
            }
        }

        [ContextMenu("Add Demo XP")]
        public void AddDemoXp()
        {
            progressModel?.AddXp(xpPerTap);
        }

        [ContextMenu("Unlock Premium Pass")]
        public void UnlockPremiumPass()
        {
            SetPremiumOwned(true);
        }

        [ContextMenu("Lock Premium Pass")]
        public void LockPremiumPass()
        {
            SetPremiumOwned(false);
        }

        public void SetPremiumOwned(bool value)
        {
            if (premiumOwned == value)
            {
                RefreshPremiumButton();
                return;
            }

            premiumOwned = value;

            if (progressModel != null)
            {
                RefreshRewardCards(progressModel.Level);
            }

            RefreshPremiumButton();
        }

        [ContextMenu("Refresh XP UI")]
        public void RefreshXpUi()
        {
            if (progressModel == null)
            {
                return;
            }

            ApplySnapshot(progressModel.Snapshot, false);
        }

        [ContextMenu("Collect Tier Nodes")]
        private void CollectTierNodesContext()
        {
            CollectTierNodes();
            RefreshXpUi();
        }

        [ContextMenu("Collect Reward Cards")]
        private void CollectRewardCardsContext()
        {
            bool wasSubscribed = rewardCardsSubscribed;

            if (wasSubscribed)
            {
                UnsubscribeRewardCards();
            }

            CollectRewardCards();

            if (wasSubscribed)
            {
                SubscribeRewardCards();
            }

            RefreshXpUi();
        }

        private void HandleProgressChanged(BattlePassProgressSnapshot snapshot)
        {
            ApplySnapshot(snapshot, true);
        }

        private void ApplySnapshot(BattlePassProgressSnapshot snapshot, bool allowRoadAnimation)
        {
            bool levelChanged = cachedLevel != snapshot.Level;

            RefreshHud(snapshot, levelChanged);

            if (levelChanged)
            {
                RefreshTierNodes(snapshot.Level);
                RefreshRewardCards(snapshot.Level);
            }

            RefreshRoadProgress(snapshot.Level, allowRoadAnimation && levelChanged);

            cachedLevel = snapshot.Level;
            cachedXp = snapshot.Xp;
            cachedRequiredXp = snapshot.RequiredXp;
        }

        private void RefreshHud(BattlePassProgressSnapshot snapshot, bool levelChanged)
        {
            if (xpBarFill != null && !Approximately(cachedXpFill, snapshot.NormalizedXp))
            {
                cachedXpFill = snapshot.NormalizedXp;
                xpBarFill.fillAmount = snapshot.NormalizedXp;
            }

            if (xpText != null &&
                (cachedXp != snapshot.Xp || cachedRequiredXp != snapshot.RequiredXp))
            {
                xpText.SetText(snapshot.Xp + "/" + snapshot.RequiredXp);
            }

            if (currentLevelBadge != null && levelChanged)
            {
                currentLevelBadge.Bind(
                    snapshot.Level,
                    BattlePassTierNodeState.Reached);
            }
        }

        private void RefreshTierNodes(int currentLevel)
        {
            if (tierNodes == null)
            {
                return;
            }

            for (int i = 0; i < tierNodes.Length; i++)
            {
                TierNodeView node = tierNodes[i];

                if (node == null)
                {
                    continue;
                }

                int tierIndex = firstNodeTierIndex + i;
                int displayNumber = tierIndex + nodeDisplayNumberOffset;

                BattlePassTierNodeState state =
                    tierIndex <= currentLevel
                        ? BattlePassTierNodeState.Reached
                        : BattlePassTierNodeState.NotReached;

                node.Bind(displayNumber, state);
            }
        }

        private void RefreshRewardCards(int currentLevel)
        {
            RefreshRewardCardGroup(premiumRewardCards, currentLevel);
            RefreshRewardCardGroup(freeRewardCards, currentLevel);
        }

        private void RefreshRewardCardGroup(RewardCardView[] cards, int currentLevel)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                RewardCardView card = cards[i];

                if (card == null)
                {
                    continue;
                }

                card.BindForBattlePass(currentLevel, premiumOwned);
            }
        }

        private void HandleRewardCardClicked(RewardCardView card)
        {
            if (card == null || progressModel == null)
            {
                return;
            }

            if (card.TryClaim(progressModel.Level, premiumOwned))
            {
                RefreshRewardCards(progressModel.Level);
            }
        }

        private void SubscribeRewardCards()
        {
            if (rewardCardsSubscribed)
            {
                return;
            }

            SubscribeRewardCardGroup(premiumRewardCards);
            SubscribeRewardCardGroup(freeRewardCards);
            rewardCardsSubscribed = true;
        }

        private void UnsubscribeRewardCards()
        {
            if (!rewardCardsSubscribed)
            {
                return;
            }

            UnsubscribeRewardCardGroup(premiumRewardCards);
            UnsubscribeRewardCardGroup(freeRewardCards);
            rewardCardsSubscribed = false;
        }

        private void SubscribeRewardCardGroup(RewardCardView[] cards)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].Clicked += HandleRewardCardClicked;
                }
            }
        }

        private void UnsubscribeRewardCardGroup(RewardCardView[] cards)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].Clicked -= HandleRewardCardClicked;
                }
            }
        }

        private void RefreshRoadProgress(int level, bool animate)
        {
            float targetX = GetRoadX(level);

            if (!hasRoadPosition)
            {
                SetRoadPosition(targetX);
                hasRoadPosition = true;
                return;
            }

            if (!animate || roadMoveDuration <= 0f || !Application.isPlaying)
            {
                SetRoadPosition(targetX);
                return;
            }

            MoveRoadPosition(targetX);
        }

        private void CollectTierNodes()
        {
            if (tierNodesRoot == null)
            {
                tierNodes = new TierNodeView[0];
                return;
            }

            tierNodes = tierNodesRoot.GetComponentsInChildren<TierNodeView>(
                includeInactiveTierNodes);

            System.Array.Sort(tierNodes, CompareTierNodesByX);
        }

        private void CollectRewardCards()
        {
            premiumRewardCards = GetRewardCardsFromRoot(premiumRewardCardsRoot);
            freeRewardCards = GetRewardCardsFromRoot(freeRewardCardsRoot);
        }

        private RewardCardView[] GetRewardCardsFromRoot(Transform root)
        {
            if (root == null)
            {
                return new RewardCardView[0];
            }

            RewardCardView[] cards = root.GetComponentsInChildren<RewardCardView>(
                includeInactiveRewardCards);

            System.Array.Sort(cards, CompareRewardCardsByX);

            return cards;
        }

        private float GetRoadX(int level)
        {
            return progressStartX + level * tierSpacing;
        }

        private void MoveRoadPosition(float targetX)
        {
            float startX = GetCurrentRoadX();

            if (Approximately(startX, targetX))
            {
                SetRoadPosition(targetX);
                return;
            }

            StopRoadAnimation();
            roadMoveRoutine = StartCoroutine(AnimateRoadPosition(startX, targetX));
        }

        private IEnumerator AnimateRoadPosition(float fromX, float toX)
        {
            float elapsed = 0f;

            while (elapsed < roadMoveDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / roadMoveDuration);
                float easedT = EaseOutBack(t, roadOvershoot);
                float x = Mathf.LerpUnclamped(fromX, toX, easedT);

                SetRoadPosition(x);

                yield return null;
            }

            SetRoadPosition(toX);
            roadMoveRoutine = null;
        }

        private void SetRoadPosition(float x)
        {
            if (Approximately(cachedRoadX, x) && hasRoadPosition)
            {
                return;
            }

            cachedRoadX = x;
            hasRoadPosition = true;

            float reachedWidth = Mathf.Max(0f, x - fillOriginX);

            SetWidth(reachedRailFill, reachedWidth);
            SetAnchoredX(roadProgressEdgeRoot, x);
            SetAnchoredX(currentProgressLineRoot, x);
        }

        private float GetCurrentRoadX()
        {
            if (currentProgressLineRoot != null)
            {
                return currentProgressLineRoot.anchoredPosition.x;
            }

            if (roadProgressEdgeRoot != null)
            {
                return roadProgressEdgeRoot.anchoredPosition.x;
            }

            return cachedRoadX;
        }

        private void StopRoadAnimation()
        {
            if (roadMoveRoutine == null)
            {
                return;
            }

            StopCoroutine(roadMoveRoutine);
            roadMoveRoutine = null;
        }

        private void RefreshPremiumButton()
        {
            if (unlockPremiumButton != null)
            {
                unlockPremiumButton.interactable = !premiumOwned;
            }
        }

        private static int CompareTierNodesByX(TierNodeView a, TierNodeView b)
        {
            if (a == null && b == null)
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            return GetAnchoredX(a.transform).CompareTo(GetAnchoredX(b.transform));
        }

        private static int CompareRewardCardsByX(RewardCardView a, RewardCardView b)
        {
            if (a == null && b == null)
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            return GetAnchoredX(a.transform).CompareTo(GetAnchoredX(b.transform));
        }

        private static float GetAnchoredX(Transform target)
        {
            RectTransform rectTransform = target as RectTransform;

            if (rectTransform != null)
            {
                return rectTransform.anchoredPosition.x;
            }

            return target.GetSiblingIndex();
        }

        private static float EaseOutBack(float t, float overshoot)
        {
            t = Mathf.Clamp01(t) - 1f;

            float strength = 1.70158f + overshoot;
            float extra = strength + 1f;

            return 1f + extra * t * t * t + strength * t * t;
        }

        private static void SetWidth(RectTransform target, float width)
        {
            if (target == null)
            {
                return;
            }

            target.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width);
        }

        private static void SetAnchoredX(RectTransform target, float x)
        {
            if (target == null)
            {
                return;
            }

            Vector2 position = target.anchoredPosition;

            if (Approximately(position.x, x))
            {
                return;
            }

            position.x = x;
            target.anchoredPosition = position;
        }

        private static bool Approximately(float a, float b)
        {
            return Mathf.Abs(a - b) <= 0.001f;
        }
    }
}
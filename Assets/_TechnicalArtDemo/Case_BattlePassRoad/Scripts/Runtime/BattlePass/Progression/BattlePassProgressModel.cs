using System;

namespace TechnicalArtDemo.BattlePass.Runtime
{
    public readonly struct BattlePassProgressSnapshot
    {
        public BattlePassProgressSnapshot(
            int level,
            int xp,
            int requiredXp,
            int maxLevel)
        {
            Level = level;
            Xp = xp;
            RequiredXp = requiredXp;
            MaxLevel = maxLevel;
        }

        public int Level { get; }
        public int Xp { get; }
        public int RequiredXp { get; }
        public int MaxLevel { get; }

        public bool IsComplete => Level >= MaxLevel;

        public float NormalizedXp
        {
            get
            {
                if (RequiredXp <= 0)
                {
                    return 1f;
                }

                return Math.Min(1f, Math.Max(0f, Xp / (float)RequiredXp));
            }
        }
    }

    public sealed class BattlePassProgressModel
    {
        private readonly int maxLevel;
        private readonly int requiredXpPerLevel;

        private int level;
        private int xp;

        public BattlePassProgressModel(
            int maxLevel,
            int requiredXpPerLevel,
            int startingLevel = 0,
            int startingXp = 0)
        {
            this.maxLevel = Math.Max(0, maxLevel);
            this.requiredXpPerLevel = Math.Max(1, requiredXpPerLevel);

            level = Clamp(startingLevel, 0, this.maxLevel);
            xp = level >= this.maxLevel ? 0 : Math.Max(0, startingXp);

            NormalizeOverflow();
        }

        public event Action<BattlePassProgressSnapshot> Changed;
        public event Action<int> LevelReached;

        public int Level => level;
        public int Xp => xp;
        public int RequiredXp => level >= maxLevel ? 0 : requiredXpPerLevel;
        public int MaxLevel => maxLevel;
        public bool IsComplete => level >= maxLevel;

        public BattlePassProgressSnapshot Snapshot => CreateSnapshot();

        public bool IsTierReached(int tier)
        {
            return tier <= level;
        }

        public void AddXp(int amount)
        {
            if (amount <= 0 || IsComplete)
            {
                Publish();
                return;
            }

            int previousLevel = level;

            xp += amount;
            NormalizeOverflow();

            Publish();

            if (level != previousLevel)
            {
                LevelReached?.Invoke(level);
            }
        }

        public void AdvanceLevel()
        {
            if (IsComplete)
            {
                Publish();
                return;
            }

            level++;
            xp = 0;

            Publish();
            LevelReached?.Invoke(level);
        }

        public void SetProgress(int targetLevel, int targetXp)
        {
            int previousLevel = level;

            level = Clamp(targetLevel, 0, maxLevel);
            xp = IsComplete ? 0 : Math.Max(0, targetXp);

            NormalizeOverflow();

            Publish();

            if (level != previousLevel)
            {
                LevelReached?.Invoke(level);
            }
        }

        private void NormalizeOverflow()
        {
            while (!IsComplete && xp >= requiredXpPerLevel)
            {
                xp -= requiredXpPerLevel;
                level++;
            }

            if (IsComplete)
            {
                xp = 0;
            }
        }

        private BattlePassProgressSnapshot CreateSnapshot()
        {
            return new BattlePassProgressSnapshot(level, xp, RequiredXp, maxLevel);
        }

        private void Publish()
        {
            Changed?.Invoke(CreateSnapshot());
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
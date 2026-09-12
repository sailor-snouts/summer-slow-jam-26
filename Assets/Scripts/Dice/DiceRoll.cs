using System.Collections.Generic;

namespace Game
{
    public readonly struct DiceRoll
    {
        public int Count { get; }
        public int Sides { get; }
        public IReadOnlyList<int> Values { get; }
        public int Total { get; }

        public DiceRoll(int count, int sides, IReadOnlyList<int> values)
        {
            this.Count = count;
            this.Sides = sides;
            this.Values = values;

            int total = 0;
            foreach (int v in values) total += v;
            Total = total;
        }

        public override string ToString()
        {
            return $"{Count}d{Sides}: [{string.Join(", ", Values)}] = {Total}";
        }
    }
}

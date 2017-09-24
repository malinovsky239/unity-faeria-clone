using System;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public static class CardLibrary
    {
        public struct Card
        {
            public string CardName;
            public int Damage;
            public int Life;
            public int Energy;
            public int Index;

            public Card(string cardName, int damage, int life, int energy)
            {
                CardName = cardName;
                Damage = damage;
                Life = life;
                Energy = energy;
                Index = -1;
            }

            public int TurnsToDefeat(Card other)
            {
                if (Damage == 0)
                {
                    return Int32.MaxValue;
                }
                return (other.Life + Damage - 1) / Damage;
            }
        }

        private static readonly Card[] Cards =
        {
            new Card("Faun",     damage: 1, life: 1, energy: 1),
            new Card("Cerberus", damage: 4, life: 2, energy: 3),
            new Card("Centaur",  damage: 2, life: 4, energy: 3),
            new Card("Minotaur", damage: 4, life: 5, energy: 7),
            new Card("Harpy",    damage: 3, life: 1, energy: 2),
        };

        public static Card GetRandomCard()
        {
            var index = Random.Range(0, Cards.Length);
            Cards[index].Index = index;
            return Cards[index];
        }
    }
}

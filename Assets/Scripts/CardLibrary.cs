using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        private class CardHistory
        {
            private int _cnt;
            private int _sumValue;

            public void AddCard(Card card)
            {
                _cnt++;
                _sumValue += card.Energy;
            }

            public float GetAverage()
            {
                if (_cnt == 0)
                {
                    return 0;
                }
                return (float)_sumValue / _cnt;
            }
        }

        private static Dictionary<GameController.Player, CardHistory> _history = new Dictionary<GameController.Player, CardHistory>();
        private static float _averageCardValue;

        private static readonly Card[] Cards =
        {
            new Card("Faun",     damage: 1, life: 1, energy: 1),
            new Card("Harpy",    damage: 3, life: 1, energy: 3),
            new Card("Centaur",  damage: 2, life: 4, energy: 4),
            new Card("Cerberus", damage: 4, life: 3, energy: 5),
            new Card("Minotaur", damage: 4, life: 5, energy: 9),
        };

        public static Card GetRandomCard(GameController.Player player)
        {
            if (!_history.ContainsKey(player))
            {
                _history.Add(player, new CardHistory());
            }

            int index;
            if (Mathf.Approximately(_history[player].GetAverage(), _averageCardValue))
            {
                index = Random.Range(0, Cards.Length);
            }
            else
            {
                var probabilityUnit = 2.0 / (Cards.Length * (Cards.Length + 1));
                double probability = Random.value;
                if (_history[player].GetAverage() > _averageCardValue)
                {
                    probability = 1 - probability;
                }
                for (index = 0; index < Cards.Length; index++)
                {
                    probability -= (index + 1) * probabilityUnit;
                    if (probability < Mathf.Epsilon)
                    {
                        break;
                    }
                }
                if (_history[player].GetAverage() > _averageCardValue)
                {
                    index = Cards.Length - 1 - index;
                }
            }

            Cards[index].Index = index;
            _history[player].AddCard(Cards[index]);
            return Cards[index];
        }

        static CardLibrary()
        {
            var sum = Cards.Sum(card => card.Energy);
            _averageCardValue = (float)sum / Cards.Length;
        }
    }
}

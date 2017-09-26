using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class DeckController : MonoBehaviour
    {
        [SerializeField] private Vector3 _leftBound;
        [SerializeField] private Vector3 _rightBound;
        private List<CardController> _cardsInDeck;
        public GameObject ActiveCard { get; private set; }

        private void Awake()
        {
            _cardsInDeck = new List<CardController>();
        }

        public Vector3 NewCardPosition(CardController card)
        {
            _cardsInDeck.Add(card);
            if (_cardsInDeck.Count == 1)
            {
                return _leftBound;
            }
            StartCoroutine(AdjustCardPositionsWhenReady(card));
            return _rightBound;
        }

        public void RemoveCard(CardController card)
        {
            _cardsInDeck.Remove(card);
            AdjustCardPositions();
        }

        public void SelectCard(GameObject card)
        {
            ActiveCard = card;
        }

        private IEnumerator AdjustCardPositionsWhenReady(CardController newCard)
        {
            yield return new WaitUntil(() => newCard.CurrentState == CardController.State.IntroMovement);
            AdjustCardPositions();
        }

        private void AdjustCardPositions()
        {
            Vector3 step = (_rightBound - _leftBound) / Mathf.Max(_cardsInDeck.Count - 1, 1);
            for (int i = 0; i < _cardsInDeck.Count; i++)
            {
                _cardsInDeck[i].AdjustPosition(_leftBound + step * i);
            }
        }
    }
}

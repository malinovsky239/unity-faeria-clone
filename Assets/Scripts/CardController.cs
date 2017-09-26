using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class CardController : MonoBehaviour
    {
        public enum State
        {
            IntroScale,
            IntroReverseScale,
            IntroMovement,
            InDeck,
            MovingToField,
            AdjustingCardInTheCell,
            FittingTheCell,
            OnTheField,
            MovingOnTheField
        }

        private class CardScale
        {
            private static readonly Vector3 ScaleStep = Vector3.one;

            public static readonly float[] TargetScale =
            {
                Constants.Card.ScaleUpperBound, Constants.Card.InDeckScale, Constants.Card.OnTheFieldScale
            };

            public static readonly Vector3[] ScaleSpeed =
            {
                ScaleStep, -ScaleStep * Constants.Card.ReverseScaleSpeedCoeff, -ScaleStep
            };

            public int ScaleStage;
            public float LastStepScaleDifference = float.PositiveInfinity;
        }

        public State CurrentState { get; private set; }
        public CellController CellController { get; private set; }
        public bool CanMoveThisTurn { get; private set; }

        private Vector3 _movementSpeed;
        private Vector3 _lastStepDifference = Vector3.zero;
        private Vector3 _desiredPosition;

        private CardScale _cardScale;

        private bool _highlightOn;
        private bool _requestedToMoveUp;

        private static int _sortingOrderCounter; // universal counter making separate sorting order for each new card    
        private int _sortingOrder;
        private State _stateOfLastSortingOrderUpdate;

        public GameController.Player Owner { get; private set; }
        private CardLibrary.Card _info;
        private Sprite _creature;

        private GameController _gameController;
        private FieldController _fieldController;
        private CardMouseInteraction _cardMouseInteraction;
        private CardPropertiesHelper _cardPropertiesHelper;
        [SerializeField] private GameObject _attackIcon;

        private void Awake()
        {
            _cardMouseInteraction = GetComponent<CardMouseInteraction>();
            _cardPropertiesHelper = GetComponent<CardPropertiesHelper>();
            _gameController = GameObject.Find(Constants.StringLiterals.GameController).GetComponent<GameController>();
            _fieldController = _gameController.GetComponent<FieldController>();
        }

        private void Start()
        {
            _sortingOrder = _sortingOrderCounter++;
            Utils.SetSortingOrderRecursively(gameObject, _sortingOrder);

            _cardScale = new CardScale();
        }

        public void Initialize(Vector3 targetPosition, CardLibrary.Card info, Sprite creature, GameController.Player player)
        {
            Owner = player;
            if (Owner == GameController.Player.Player1)
            {
                _cardPropertiesHelper.SetupCard(info, creature);
            }
            else
            {
                _cardPropertiesHelper.SetupCard();
            }

            _info = info;
            _creature = creature;

            AnimateMovement(targetPosition, State.IntroScale);
        }

        public void Unfreeze()
        {
            CanMoveThisTurn = true;
        }

        public void AdjustPosition(Vector3 newPosition)
        {
            _desiredPosition = newPosition;
        }

        public void GoToField(CellController initialCell)
        {
            _gameController.GetDeckByOwner(Owner).RemoveCard(this);
            _gameController.ChangeEnergy(-GetEnergyPrice(), Owner);
            Reveal();
            CanMoveThisTurn = false;
            if (Owner == GameController.Player.Player1)
            {
                _cardMouseInteraction.UnlockForMouseInteractions();
            }
            CellController = initialCell;
            AnimateMovement(initialCell.transform.position, State.MovingToField);
        }

        private void Reveal()
        {
            if (Owner == GameController.Player.Player2)
            {
                _cardPropertiesHelper.SetupCard(_info, _creature);
            }
        }

        public IEnumerator MoveTo(CellController destinationCell)
        {
            CanMoveThisTurn = false;
            var sourceCell = CellController;
            _fieldController.MoveContent(sourceCell, destinationCell);
            AnimateMovement(destinationCell.transform.position, State.MovingOnTheField);
            CellController = destinationCell;
            yield return new WaitUntil(() => CurrentState == State.OnTheField);
            destinationCell.PostProcessCard(Owner);
        }

        public IEnumerator Attack(GameObject attackedEntity)
        {
            _gameController.OngoingAnimationsCount++;
            yield return new WaitUntil(() => CurrentState == State.OnTheField);
            var attackIcon = Instantiate(_attackIcon);
            attackIcon.transform.position = (transform.position + attackedEntity.transform.position) / 2;
            yield return new WaitForSeconds(Constants.Intervals.AttackIconRotation);

            var attackedCard = attackedEntity.GetComponent<CardController>();
            if (attackedCard)
            {
                attackedCard.BringDamage(GetDamage());
                BringDamage(attackedCard.GetDamage());
            }
            else
            {
                var attackedOrb = attackedEntity.GetComponent<OrbController>();
                attackedOrb.BringDamage(GetDamage());
            }
            Destroy(attackIcon.gameObject);
            _gameController.OngoingAnimationsCount--;
        }

        private void AnimateMovement(Vector3 fieldPosition, State newState)
        {
            _gameController.OngoingAnimationsCount++;
            CurrentState = newState;
            _desiredPosition = new Vector3(fieldPosition.x, fieldPosition.y);
            _movementSpeed = _desiredPosition - transform.position;
        }

        private void BringDamage(int value)
        {
            _info.Life -= value;
            _cardPropertiesHelper.SetHealth(_info.Life);
            if (_info.Life <= 0)
            {
                StartCoroutine(Die());
            }
        }

        private IEnumerator Die()
        {
            _fieldController.ClearCell(CellController);
            yield return new WaitForSeconds(Constants.Intervals.CardDeath);
            Destroy(gameObject);
        }

        public int GetEnergyPrice()
        {
            return _info.Energy;
        }

        public int GetDamage()
        {
            return _info.Damage;
        }

        public int GetHealth()
        {
            return _info.Life;
        }

        public bool IsSelectable()
        {
            return Owner == GameController.Player.Player1 &&
                   !_gameController.IsUIBlocked() &&
                   (CurrentState == State.InDeck && IsPlayable() ||
                    CurrentState == State.OnTheField && CanMoveThisTurn);
        }

        public bool IsPlayable()
        {
            return _info.Energy <= _gameController.GetPlayerEnergy(Owner);
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case State.IntroScale:
                case State.IntroReverseScale:
                case State.FittingTheCell:
                    var scale = transform.localScale;
                    if (Mathf.Abs(scale.x - CardScale.TargetScale[_cardScale.ScaleStage]) > _cardScale.LastStepScaleDifference)
                    {
                        _cardScale.LastStepScaleDifference = float.PositiveInfinity;
                        _cardScale.ScaleStage++;
                        CurrentState++;
                    }
                    else
                    {
                        _cardScale.LastStepScaleDifference = Mathf.Abs(scale.x - CardScale.TargetScale[_cardScale.ScaleStage]);
                        scale += CardScale.ScaleSpeed[_cardScale.ScaleStage] * Time.deltaTime;
                    }
                    transform.localScale = scale;
                    break;

                case State.IntroMovement:
                case State.MovingToField:
                case State.MovingOnTheField:
                    var diff = _desiredPosition - transform.position;
                    if (Vector3.Dot(diff, _lastStepDifference) < 0 ||
                        CurrentState == State.MovingToField && diff.magnitude < (_movementSpeed * Time.deltaTime).magnitude)
                    {
                        _lastStepDifference = Vector3.zero;
                        _gameController.OngoingAnimationsCount--;
                        if (CurrentState == State.MovingOnTheField)
                        {
                            CurrentState = State.OnTheField;
                            _cardMouseInteraction.UnlockForMouseInteractions();
                        }
                        else
                        {
                            CurrentState++;
                        }
                    }
                    else
                    {
                        _lastStepDifference = _desiredPosition - transform.position;
                        transform.position += _movementSpeed * Time.deltaTime;
                    }
                    break;

                case State.AdjustingCardInTheCell:
                    diff = _desiredPosition - transform.position;
                    if (diff.magnitude > Constants.SmallEps)
                    {
                        transform.position += diff * Constants.Card.OnTheFieldAdjustmentSpeed;
                    }
                    else
                    {
                        CurrentState++;
                    }
                    break;

                case State.InDeck:
                    diff = _desiredPosition - transform.position;
                    if (diff.magnitude > Constants.LargeEps)
                    {
                        var temp = diff * Time.deltaTime;
                        transform.position += new Vector3(temp.x, temp.y * Constants.Card.InDeckShiftOnSelectionSpeed, temp.z);
                    }
                    if (_cardMouseInteraction.State != CardMouseInteraction.MouseInteractionState.Basic)
                    {
                        if (!_requestedToMoveUp)
                        {
                            _requestedToMoveUp = true;
                            _desiredPosition += Vector3.up * Constants.Card.InDeckShiftOnSelection;
                        }
                    }
                    else
                    {
                        if (_requestedToMoveUp)
                        {
                            _requestedToMoveUp = false;
                            _desiredPosition -= Vector3.up * Constants.Card.InDeckShiftOnSelection;
                        }
                    }
                    break;

                case State.OnTheField:
                    if (_cardMouseInteraction.State != CardMouseInteraction.MouseInteractionState.Basic)
                    {
                        if (!_highlightOn)
                        {
                            _highlightOn = true;
                            _cardPropertiesHelper.Highlight(true);
                        }
                    }
                    else
                    {
                        if (_highlightOn)
                        {
                            _highlightOn = false;
                            _cardPropertiesHelper.Highlight(false);
                        }
                    }
                    break;

                default:
                    throw new Exception("Unknown Card State!");
            }

            if (_stateOfLastSortingOrderUpdate != CurrentState)
            {
                SetSortingOrder();
            }
        }

        private void SetSortingOrder()
        {
            _stateOfLastSortingOrderUpdate = CurrentState;
            switch (CurrentState)
            {
                case State.OnTheField:
                case State.InDeck:
                    Utils.SetSortingOrderRecursively(gameObject, _sortingOrder);
                    break;

                case State.AdjustingCardInTheCell:
                case State.FittingTheCell:
                case State.IntroMovement:
                case State.IntroReverseScale:
                case State.IntroScale:
                case State.MovingOnTheField:
                case State.MovingToField:
                    Utils.SetSortingOrderRecursively(gameObject, Constants.Card.SortingOrderShiftForTopmost + _sortingOrder);
                    break;

                default:
                    throw new Exception("Unknown Card State!");
            }
        }
    }
}

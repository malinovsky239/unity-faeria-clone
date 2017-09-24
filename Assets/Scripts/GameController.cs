using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        public enum Mode
        {
            WaitingForAction,
            EnhancingTerrain1,
            PlacingCard,
            MovingCard
        }

        public enum Player
        {
            Player1,
            Player2,
            None,
            Both
        }

        public Mode CurrentMode { get; set; }
        public int OngoingAnimationsCount { get; set; }

        private FieldController _fieldController;
        private FieldInitializer _fieldInitializer;

        [SerializeField] private SpriteCollection _spriteCollection;
        [SerializeField] private Sprite[] _cardCreatures;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _playerEnergyIndicator;
        [SerializeField] private GameObject _npcEnergyIndicator;
        [SerializeField] private DeckController _playerDeck;
        [SerializeField] private DeckController _npcDeck;
        [SerializeField] private EndTurn _endTurn;
        [SerializeField] private MessageController _messageController;

        private int _currentMoveTerrainCellsCount;
        private int _playerEnergy;
        public bool IsPlayerMove { get; private set; }

        private NpcBehaviour _npc;

        private void Awake()
        {
            _fieldController = GetComponent<FieldController>();
            _fieldInitializer = GetComponent<FieldInitializer>();
            _spriteCollection = GetComponent<SpriteCollection>();
        }

        private void Start()
        {
            _fieldInitializer.Initialize();
            _npc = GetComponent<NpcBehaviour>();

            _playerEnergy = Constants.StartMoveEnergyBonus;
            _currentMoveTerrainCellsCount = Constants.StartMoveTerrainCellsCount;

            StartCoroutine(GameInitialization());
        }

        private IEnumerator GameInitialization()
        {
            for (var playerNumber = 0; playerNumber < 2; playerNumber++)
            {
                for (var i = 0; i < Constants.CardsPerSideAtStart; i++)
                {
                    var player = (Player)playerNumber;
                    var card = GenerateRandomCard(GetDeckByOwner(player), player);
                    if (player == Player.Player2)
                    {
                        _npc.PutCardIntoDeck(card);
                    }
                    yield return new WaitUntil(() => card.CurrentState == CardController.State.IntroMovement);
                }
            }
            yield return new WaitUntil(() => OngoingAnimationsCount == 0);
            _messageController.Show(_spriteCollection.NewTurnMessage);
            IsPlayerMove = true;
            CurrentMode = Mode.WaitingForAction;
        }

        public IEnumerator NextTurn()
        {
            IsPlayerMove = false;
            NewTurnPreparationsStage1(Player.Player2);
            yield return new WaitForSeconds(Constants.Intervals.BeforeNPCTurn);
            NewTurnPreparationsStage2(Player.Player2);
            StartCoroutine(_npc.MakeTurn());
            yield return new WaitUntil(() => _npc.TurnStage == NpcBehaviour.NpcTurnStage.EndTurn && OngoingAnimationsCount == 0);

            _currentMoveTerrainCellsCount = Constants.StartMoveTerrainCellsCount;
            if (_fieldController.GetPotentialTerrainExpansion(FieldController.CellOwner.Player1).Count == 0)
            {
                _currentMoveTerrainCellsCount = 0;
                _endTurn.On();
            }

            NewTurnPreparationsStage1(Player.Player1);
            yield return new WaitUntil(() => OngoingAnimationsCount == 0);
            NewTurnPreparationsStage2(Player.Player1);
            IsPlayerMove = true;
            _messageController.Show(_spriteCollection.NewTurnMessage);
        }

        private void NewTurnPreparationsStage1(Player player)
        {
            _fieldController.RefillMagicSources();
            var card = GenerateRandomCard(GetDeckByOwner(player), player);
            if (player == Player.Player2)
            {
                _npc.PutCardIntoDeck(card);
            }
        }

        private void NewTurnPreparationsStage2(Player player)
        {
            ChangeEnergy(
                Constants.StartMoveEnergyBonus +
                Constants.ControlledMagicSourceEnergyBonus *
                _fieldController.CountMagicSourcesUnderControl(player), player);
            _fieldController.AllowCardsToMove(player);
        }

        public void ChangeEnergy(int changeValue, Player player)
        {
            switch (player)
            {
                case Player.Player1:
                    _playerEnergy += changeValue;
                    _playerEnergyIndicator.GetComponent<TextMesh>().text = _playerEnergy.ToString();
                    break;

                case Player.Player2:
                    _npc.CurrentEnergy += changeValue;
                    _npcEnergyIndicator.GetComponent<TextMesh>().text = _npc.CurrentEnergy.ToString();
                    break;

                default:
                    throw new Exception("Wrong player");
            }
        }

        public void DecrementTerrainCellsCount()
        {
            _currentMoveTerrainCellsCount--;
            if (_fieldController.GetPotentialTerrainExpansion(FieldController.CellOwner.Player1).Count == 0)
            {
                _currentMoveTerrainCellsCount = 0;
            }
            if (_currentMoveTerrainCellsCount == 0)
            {
                _endTurn.On();
            }
        }

        public CardController GenerateRandomCard(DeckController deck, Player player)
        {
            GameObject card = Instantiate(_cardPrefab);
            CardLibrary.Card cardProperties = CardLibrary.GetRandomCard();
            CardController controller = card.GetComponent<CardController>();
            controller.Initialize(deck.NewCardPosition(controller),
                cardProperties, _cardCreatures[cardProperties.Index], player);
            return controller;
        }

        public int GetPlayerEnergy(Player player)
        {
            switch (player)
            {
                case Player.Player1:
                    return _playerEnergy;
                case Player.Player2:
                    return _npc.CurrentEnergy;
                default:
                    throw new Exception("Wrong player");
            }
        }

        public Vector3 GetOrbPositionByOwner(Player player)
        {
            int xShift;
            switch (player)
            {
                case Player.Player1:
                    xShift = _fieldInitializer.Radius;
                    break;

                case Player.Player2:
                    xShift = -_fieldInitializer.Radius;
                    break;

                default:
                    throw new Exception("Wrong player");
            }
            var orb = _fieldController.FieldContent[_fieldInitializer.CenterX + xShift][_fieldInitializer.CenterY];
            return orb.transform.position;
        }

        public bool IsUIBlocked()
        {
            return OngoingAnimationsCount > 0 || !IsPlayerMove;
        }

        public void SwitchToWaitingMode()
        {
            _fieldController.RemoveCellHighlighting();
            CurrentMode = Mode.WaitingForAction;
        }

        public DeckController GetDeckByOwner(Player player)
        {
            switch (player)
            {
                case Player.Player1:
                    return _playerDeck;
                case Player.Player2:
                    return _npcDeck;
                default:
                    throw new Exception("Wrong player");
            }
        }
    }
}

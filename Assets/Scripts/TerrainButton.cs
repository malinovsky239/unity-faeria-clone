using UnityEngine;

namespace Assets.Scripts
{
    public class TerrainButton : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private PointerController _pointerController;
        private FieldController _fieldController;

        private void Awake()
        {
            _fieldController = _gameController.GetComponent<FieldController>();
        }

        private void OnMouseDown()
        {
            if (_gameController.CurrentMode == GameController.Mode.WaitingForAction && !_gameController.IsUIBlocked())
            {
                _gameController.CurrentMode = GameController.Mode.EnhancingTerrain1;
                _fieldController.ShowPotentialTerrainExpansion(FieldController.CellOwner.Player1);
                _pointerController.transform.position = transform.position;
            }
        }
    }
}

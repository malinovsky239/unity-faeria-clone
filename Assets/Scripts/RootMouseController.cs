using UnityEngine;

namespace Assets.Scripts
{
    public class RootMouseController : MonoBehaviour
    {
        private GameController _gameController;

        private void Start()
        {
            _gameController = GetComponent<GameController>();
        }

        private void Update()
        {
            if (_gameController.CurrentMode != GameController.Mode.GameOver)
            {
                if (Input.GetMouseButtonDown(Constants.RightMouseButton))
                {
                    _gameController.SwitchToWaitingMode();
                }
            }
        }
    }
}

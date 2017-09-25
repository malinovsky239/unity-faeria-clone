using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class PointerController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Material _pointMaterial;
        [SerializeField] private GameController _gameController;
        private FieldController _fieldController;
        private readonly List<GameObject> _points = new List<GameObject>();
        private GameObject _graphics;

        private void Awake()
        {
            _graphics = GameObject.Find(Constants.StringLiterals.Graphics);
            _fieldController = _gameController.GetComponent<FieldController>();
            for (var i = 0; i < Constants.PointerDotsCount; i++)
            {
                var point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.GetComponent<Renderer>().material = _pointMaterial;
                point.transform.localScale = Vector3.one * Constants.PointerSingleDotScale;
                _points.Add(point);
                point.transform.parent = _graphics.transform;
            }
        }

        private void DrawLine(Vector3 source, Vector3 target, int l, int r)
        {
            var step = (target - source) / (r - l);
            for (var i = l; i < r; i++)
            {
                _points[i].transform.position = source + step * (i - l);
            }
        }

        private void Update()
        {
            if (_gameController.CurrentMode != GameController.Mode.WaitingForAction && _gameController.CurrentMode != GameController.Mode.GameOver)
            {
                var source = transform.position;
                var target = _camera.ScreenToWorldPoint(Input.mousePosition);
                if (_gameController.CurrentMode == GameController.Mode.MovingCard && _fieldController.CellToAttackFrom)
                {
                    var intermediatePoint = _fieldController.CellToAttackFrom.transform.position;
                    DrawLine(source, intermediatePoint, 0, Constants.PointerDotsCount / 2);
                    DrawLine(intermediatePoint, target, Constants.PointerDotsCount / 2, Constants.PointerDotsCount);
                }
                else
                {
                    DrawLine(source, target, 0, Constants.PointerDotsCount);
                }
                _graphics.SetActive(true);
            }
            else
            {
                _graphics.SetActive(false);
            }
        }
    }
}

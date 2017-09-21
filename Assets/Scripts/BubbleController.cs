using UnityEngine;

namespace Assets.Scripts
{
    public class BubbleController : MonoBehaviour
    {
        private int _step;
        private float _speed;
        private int _lifeDuration;
        private bool _isInGoToOrbMode;
        private Vector3 _target;

        public void GoToOrb(Vector3 orbPosition)
        {
            _isInGoToOrbMode = true;
            _target = orbPosition;
            GetComponent<SpriteRenderer>().sortingLayerName = Constants.StringLiterals.FlyingBubbleSortingLayer;
        }

        private void Awake()
        {
            _lifeDuration = Random.Range(Constants.Bubble.ShortestLifeDuration, Constants.Bubble.LongestLifeDuration);
            _speed = Random.Range(Constants.Bubble.MinSpeed, Constants.Bubble.MaxSpeed);
            transform.localScale *= Random.Range(Constants.Bubble.MinScale, Constants.Bubble.MaxScale);
            transform.position += new Vector3(
                Random.Range(Constants.Bubble.InitMinShiftX, Constants.Bubble.InitMaxShiftX),
                Random.Range(Constants.Bubble.InitMinShiftY, Constants.Bubble.InitMaxShiftY));
        }

        private void Update()
        {
            if (_isInGoToOrbMode)
            {
                var difference = _target - transform.position;
                if (difference.magnitude < Constants.Bubble.DestructionRange)
                {
                    Destroy(gameObject);
                }
                transform.position += difference * Constants.Bubble.SpeedTowardsOrb;
            }
            else
            {
                _step++;
                transform.position += Vector3.up * _speed * Time.deltaTime;
                if (_step == _lifeDuration)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

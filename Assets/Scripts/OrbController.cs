using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class OrbController : MonoBehaviour
    {
        private GameObject _hero;
        private TextMesh _life;

        private GameController.Player _owner = GameController.Player.Player1;
        private MessageController _messageController;
        private SpriteCollection _spriteCollection;

        private void Awake()
        {
            _hero = GameObject.Find("Hero");
            _life = GameObject.Find("Heart/Life").GetComponent<TextMesh>();
            _messageController = GameObject.Find(Constants.StringLiterals.MessageController).GetComponent<MessageController>();
            _spriteCollection = GameObject.Find(Constants.StringLiterals.GameController).GetComponent<SpriteCollection>();
        }

        public void SetOwner(GameController.Player player)
        {
            _owner = player;
        }

        public void SetHero(Sprite heroSprite, float heroScale = 1)
        {
            _hero.GetComponent<SpriteRenderer>().sprite = heroSprite;
            var localScale = _hero.transform.localScale;
            localScale *= heroScale;
            _hero.transform.localScale = localScale;
        }

        public void BringDamage(int value)
        {
            var newHealth = GetHealth() - value;
            SetHealth(newHealth);
            if (newHealth < 0)
            {
                StartCoroutine(Die());
            }
        }

        private IEnumerator Die()
        {
            GetComponent<SpriteRenderer>().sprite = _spriteCollection.BrokenOrb;
            yield return new WaitForSeconds(Constants.Intervals.BeforeOrbDeath);
            _messageController.Show(_owner == GameController.Player.Player1
                ? _spriteCollection.LossMessage
                : _spriteCollection.VictoryMessage);
        }

        private int GetHealth()
        {
            return Convert.ToInt32(_life.text);
        }

        private void SetHealth(int value)
        {
            _life.text = value.ToString();
        }
    }
}

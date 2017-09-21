using UnityEngine;

namespace Assets.Scripts
{
    public class CardPropertiesHelper : MonoBehaviour
    {
        private SpriteCollection _spriteCollection;
        private SpriteRenderer _shieldSpriteRenderer;

        private void Awake()
        {
            _spriteCollection = GameObject.Find(Constants.StringLiterals.GameController).GetComponent<SpriteCollection>();
            _shieldSpriteRenderer = transform.Find("Shield").GetComponent<SpriteRenderer>();
        }

        public void Highlight(bool on)
        {
            _shieldSpriteRenderer.sprite = on ? _spriteCollection.HighlightedShield : _spriteCollection.BasicShield;
        }

        public void SetupCard(CardLibrary.Card info = new CardLibrary.Card(), Sprite creature = null)
        {
            if (!creature)
            {
                SetName();
                SetHealth();
                SetDamage();
                SetEnergy();
                SetCreature();
                SetOpponentShield();
            }
            else
            {
                SetName(info.CardName);
                SetHealth(info.Life);
                SetDamage(info.Damage);
                SetEnergy(info.Energy);
                SetCreature(creature);
            }
        }

        public void SetHealth(int health = -1)
        {
            transform.Find("Heart/Life").GetComponent<TextMesh>().text = health == -1 ? "?" : health.ToString();
        }

        private void SetCreature(Sprite creature = null)
        {
            transform.Find("Creature").GetComponent<SpriteRenderer>().sprite = creature ?? _spriteCollection.Unknown;
        }

        private void SetName(string cardName = "?")
        {
            transform.Find("Shield/CardName").GetComponent<TextMesh>().text = cardName;
        }

        private void SetDamage(int damage = -1)
        {
            transform.Find("Fire/Damage").GetComponent<TextMesh>().text = damage == -1 ? "?" : damage.ToString();
        }

        private void SetEnergy(int energy = -1)
        {
            transform.Find("BlueSphere/Energy").GetComponent<TextMesh>().text = energy == -1 ? "?" : energy.ToString();
        }

        private void SetOpponentShield()
        {
            _shieldSpriteRenderer.sprite = _spriteCollection.OpponentCardShield;
        }
    }
}

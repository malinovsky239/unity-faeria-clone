using UnityEngine;

namespace Assets.Scripts
{
    public class SpriteCollection : MonoBehaviour
    {
        [SerializeField] public Sprite BasicShield;
        [SerializeField] public Sprite HighlightedShield;
        [SerializeField] public Sprite OpponentCardShield;
        [SerializeField] public Sprite FilledMagicSource;
        [SerializeField] public Sprite EmptyMagicSource;
        [SerializeField] public Sprite Unknown;
        [SerializeField] public Sprite NewTurnMessage;
        [SerializeField] public Sprite VictoryMessage;
        [SerializeField] public Sprite LossMessage;
        [SerializeField] public Sprite BrokenOrb;
    }
}
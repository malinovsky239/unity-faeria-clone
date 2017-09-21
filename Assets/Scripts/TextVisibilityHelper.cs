using UnityEngine;

namespace Assets.Scripts
{
    public class TextVisibilityHelper : MonoBehaviour
    {
        // workaround for putting text mesh on top of parent sprite from non-default layer
        private void Awake()
        {
            var textRenderer = GetComponent<Renderer>();
            var parentRenderer = GetComponentInParent<SpriteRenderer>();
            textRenderer.sortingLayerID = parentRenderer.sortingLayerID;
            textRenderer.sortingOrder = parentRenderer.sortingOrder + 1;
        }
    }
}

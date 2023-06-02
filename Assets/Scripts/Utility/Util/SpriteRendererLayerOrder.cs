using UnityEngine;

namespace Utility.Util
{
    public class SpriteRendererLayerOrder : MonoBehaviour
    {
        [SerializeField] private string defaultLayerName;
        [SerializeField] private string targetLayerName;

        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _renderer.sortingLayerName = targetLayerName;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _renderer.sortingLayerName = defaultLayerName;
        }
    }
}

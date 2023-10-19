using System;
using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Squirrel
{
    public class DragItem : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D target;
        
        [SerializeField] private AudioClip takeAudioClip;
        [SerializeField] private AudioClip dropAudioClip;

        [NonSerialized] public Collider2D Collider2D;
        
        public Action onFire;
        
        private Vector3 _originPos;
        private bool _isInit;

        private void Awake()
        {
            if (!_isInit)
            {
                Init();
            }
        }

        private void Init()
        {
            _originPos = transform.position;
            _isInit = true;
            Collider2D = GetComponent<Collider2D>();
        }
        // onDragStart

        private void OnMouseDrag()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = new Vector3(pos.x, pos.y, 0);

            transform.position = pos;
        }

        private void OnMouseUp()
        {
            AudioManager.Instance.PlaySfx(dropAudioClip);
            if (target.enabled && Vector2.Distance(target.transform.position, transform.position) < target.radius * 1.2f)
            {
                Debug.Log("Drag Fire!");
                onFire?.Invoke();
            }
        }

        public void Reset()
        {
            if (!_isInit)
            {
                Init();
            }
            else
            {
                transform.position = _originPos;
            }

            Collider2D.enabled = true;
            gameObject.SetActive(true);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Squirrel
{
    public class DragItem : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerDownHandler
    {
        [SerializeField] private CircleCollider2D target;
        
        [SerializeField] private AudioData takeAudioData;
        [SerializeField] private AudioData dropAudioData;
        
        [NonSerialized] public Collider2D Collider2D;
        
        public Action onTake;
        public Action onFire;
        
        private Vector3 _originPos;
        private bool _isInit;
        private bool _isDrag;

        public void Awake()
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
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDrag)
            {
                takeAudioData.Play();
                onTake?.Invoke();
            }

            _isDrag = true;
            var pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            pos = new Vector3(pos.x, pos.y, 0);

            transform.position = pos;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDrag = false;
            dropAudioData.Play();
            if (target.enabled && Vector2.Distance(target.transform.position, transform.position) < target.radius * 1.2f)
            {
                Debug.Log("Drag Fire!");
                onFire?.Invoke();
            }
        }

        public void ResetDragItem()
        {
            if (!_isInit)
            {
                Init();
            }
            else
            {
                transform.position = _originPos;
            }

            _isDrag = false;
            Collider2D.enabled = true;
            gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }
    }
}
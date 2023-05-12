using System;
using UnityEngine;

namespace Game.Camping
{
    public class Diary : MonoBehaviour
    {
        [Range(0, .1f)] [SerializeField] private float disValue;

        [SerializeField] private CircleCollider2D fire;
        
        [SerializeField] private Transform outPos;
        
        public Action onOpen;
        public Action onFire;
        public Action onPickUp;

        private Vector3 _originPos;
        
        private Vector3 _clickedPos;
        
        private bool _isDrag;

        private bool _isInit;

        private bool _isOut;

        private void Awake()
        {
            if (!_isInit)
            {
                _originPos = transform.position;
                _isInit = true;
            }
        }


        private void OnMouseDown()
        {
            if (_isOut)
            {
                _clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _clickedPos = new Vector3(_clickedPos.x, _clickedPos.y, 0);
            }
        }

        private void OnMouseDrag()
        {
            if (_isOut)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos = new Vector3(pos.x, pos.y, 0);

                if (_isDrag)
                {
                    transform.position = pos;
                }
                else if (Vector3.Distance(_clickedPos, pos) > disValue)
                {
                    transform.position = pos;
                    _isDrag = true;
                }
            }
        }

        private void OnMouseUp()
        {
            if (_isOut)
            {
                if (_isDrag)
                {
                    _isDrag = false;
                    if (Vector3.Distance(fire.transform.position, transform.position) < fire.radius)
                    {
                        onFire?.Invoke();
                    }
                }
                else
                {
                    onOpen?.Invoke();
                }
            }
            else
            {
                transform.position = outPos.position;
                _isOut = true;
                onPickUp?.Invoke();
            }
        }

        public void Reset()
        {
            if (!_isInit)
            {
                _originPos = transform.position;
                _isInit = true;
            }
            else
            {
                transform.position = _originPos;
            }

            _isOut = false;

            gameObject.SetActive(false);
        }
    }
}

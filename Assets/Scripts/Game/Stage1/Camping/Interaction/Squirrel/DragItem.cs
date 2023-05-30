using System;
using UnityEngine;

namespace Game.Stage1.Camping.Interaction.Squirrel
{
    public class DragItem : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D target;

        public Action onFire;

        private Vector3 _originPos;
        private bool _isInit;

        private void Awake()
        {
            if (!_isInit)
            {
                _originPos = transform.position;
                _isInit = true;
            }
        }

        private void OnMouseDrag()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = new Vector3(pos.x, pos.y, 0);

            transform.position = pos;
        }

        private void OnMouseUp()
        {
            if (target.enabled &&
                Vector2.Distance(target.transform.position, transform.position) < target.radius * 1.2f)
            {
                Debug.Log("ㅎㅇ");
                onFire?.Invoke();
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

            GetComponent<Collider2D>().enabled = true;
            gameObject.SetActive(true);
        }
    }
}
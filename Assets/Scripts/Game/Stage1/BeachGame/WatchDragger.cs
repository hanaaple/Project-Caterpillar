using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BeachGame
{
    public class WatchDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Range(0, 9)] [SerializeField] private int defaultIndex;
        [Space(5)]
        
        [Range(1, 10)] [SerializeField] private int divCount;
        [Range(0, 360)] [SerializeField] private int startRot;

        [Range(1, 5)] [SerializeField] private float rotSpeed;
        
        [Range(1, 5)] [SerializeField] private float angleWeight;

        private float[] _angles;
        private float _size;

        private Vector3 _beforeVec;

        public Action[] actions;

        [NonSerialized] public int index;
        [NonSerialized] public Stack<int> pastIdxs;
        
        [NonSerialized] public bool interactable;

        private void Start()
        {
            pastIdxs = new Stack<int>();
        }

        private void OnEnable()
        {
            UpdateData();
        }

        private void OnValidate()
        {
            UpdateData();
        }

        private void UpdateData()
        {
            _angles = new float[divCount];
            actions = new Action[divCount];

            _size = 360f / divCount;
            var angle = startRot - _size / 2;
            for (int i = 0; i < divCount; i++)
            {
                angle %= 360;
                _angles[i] = angle;
                angle += _size;
            }

            if (defaultIndex >= divCount)
            {
                defaultIndex = divCount - 1;
            }

            index = defaultIndex;
            SetRotation(_angles[defaultIndex]);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            _beforeVec = eventData.position - (Vector2) transform.position;
            StopAllCoroutines();
            pastIdxs.Clear();
            pastIdxs.Push(index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }
            var beforeAngle = transform.eulerAngles.z;

            Vector2 point = eventData.position - (Vector2) transform.position;

            var signedAngle = Vector2.SignedAngle(_beforeVec, point);
            var afterAngle = beforeAngle + signedAngle / angleWeight;
            afterAngle = afterAngle % 360;
            transform.eulerAngles = new Vector3(0, 0, afterAngle);

            _beforeVec = point;

            var close = _angles.OrderBy(angle =>
                    Mathf.Abs(Vector2.SignedAngle(
                        new Vector2(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad)),
                        transform.up)))
                .Last();
            var closeIdx = Array.FindIndex(_angles, angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(close));

            if (pastIdxs.Count == divCount)
            {
                pastIdxs.Clear();
                pastIdxs.Push(index);
                Debug.Log("현재 Stack : " +
                          String.Join("", new List<int>(pastIdxs).ConvertAll(i => i.ToString()).ToArray()) +
                          ", 추가 : " + closeIdx);
            }else if (pastIdxs.Count == 0)
            {
                pastIdxs.Push(index);
            }
            

            if (pastIdxs.Peek() != closeIdx)
            {
                if (pastIdxs.Contains(closeIdx))
                {
                    while (pastIdxs.Peek() != closeIdx)
                    {
                        pastIdxs.Pop();
                    }
                }
                else if (signedAngle < 0 && pastIdxs.Contains((pastIdxs.Peek() - 1 + divCount) % divCount))
                {
                    Debug.Log("11");
                    pastIdxs.Clear();
                    pastIdxs.Push(index);
                    while (pastIdxs.Peek() != closeIdx)
                    {
                        pastIdxs.Push((pastIdxs.Peek() - 1 + divCount) % divCount);
                    }
                }
                else if (signedAngle > 0 && pastIdxs.Contains((pastIdxs.Peek() + 1 + divCount) % divCount))
                {
                    Debug.Log("22");
                    pastIdxs.Clear();
                    pastIdxs.Push(index);
                    while (pastIdxs.Peek() != closeIdx)
                    {
                        pastIdxs.Push((pastIdxs.Peek() + 1) % divCount);
                    }
                }
                // 801 쌓고 바로 반대편으로 돌려서 3으로 가면 포함하지 않아서 예외사항 있음
                else
                {
                    if (signedAngle < 0)
                    {
                        // 오른쪽
                        // 6 -> 4 값이 작아짐   76 -> 5
                        while (pastIdxs.Peek() != closeIdx)
                        {
                            pastIdxs.Push((pastIdxs.Peek() - 1 + divCount) % divCount);
                        }
                    }
                    else if (signedAngle > 0)
                    {
                        //왼쪽
                        // 4 -> 6 값이 커짐     67 -> 8
                        while (pastIdxs.Peek() != closeIdx)
                        {
                            pastIdxs.Push((pastIdxs.Peek() + 1) % divCount);
                        }
                    }
                }
                // Debug.Log("현재 Stack : " +
                //           String.Join("", new List<int>(pastIdxs).ConvertAll(i => i.ToString()).ToArray()) +
                //           ", 추가 : " + closeIdx);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }
            var beforeAngle = transform.eulerAngles.z;

            Vector2 point = eventData.position - (Vector2) transform.position;

            var afterAngle = beforeAngle + Vector2.SignedAngle(_beforeVec, point) / angleWeight;
            afterAngle = afterAngle % 360;
            transform.eulerAngles = new Vector3(0, 0, afterAngle);

            var close = _angles.OrderBy(angle =>
                    Mathf.Abs(Vector2.SignedAngle(
                        new Vector2(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad)),
                        transform.up)))
                .Last();
            
            float weight = Mathf.Lerp(.2f, .8f, Mathf.Abs(_size - Mathf.Abs(close - afterAngle)) / _size * 2);

            StartCoroutine(SetRotation(close, weight));
        }

        private IEnumerator SetRotation(float targetAngle, float weight)
        {
            interactable = false;
            var targetVector = new Vector2(Mathf.Cos((targetAngle + 90) * Mathf.Deg2Rad), Mathf.Sin((targetAngle + 90) * Mathf.Deg2Rad));
            
            var waitForFixedUpdate = new WaitForFixedUpdate();
            float t = 0f;
            Vector2 startVector = transform.up;
            while (t <= 1f)
            {
                var afterVector = Vector2.Lerp(startVector, targetVector, t);
                transform.up = afterVector;
                t += Time.fixedDeltaTime * weight * rotSpeed;
                yield return waitForFixedUpdate;
            }
            
            var idx = Array.FindIndex(_angles, angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(targetAngle));
            
            interactable = true;
            actions[idx]?.Invoke();
        }

        private void SetRotation(float targetAngle)
        {
            var targetVector = new Vector2(Mathf.Cos((targetAngle + 90) * Mathf.Deg2Rad),
                Mathf.Sin((targetAngle + 90) * Mathf.Deg2Rad));
            transform.up = targetVector;
        }

        public void Init()
        {
            interactable = true;
            SetRotation(_angles[defaultIndex]);
            index = Array.FindIndex(_angles, angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(_angles[defaultIndex]));
        }
    }
}
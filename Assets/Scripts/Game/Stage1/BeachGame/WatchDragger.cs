using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Stage1.BeachGame
{
    public class WatchDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Range(0, 9)] [SerializeField] private int defaultIndex;

        [Space(5)] [Range(1, 10)] [SerializeField]
        private int divCount;

        [Range(0, 360)] [SerializeField] private int startRot;

        [Range(1, 5)] [SerializeField] private float rotSpeed;

        [Range(1, 5)] [SerializeField] private float angleWeight;

        private float[] _angles;
        private float _size;

        private Vector3 _beforeVec;

        public Action[] actions;

        [NonSerialized] public int Index;
        [NonSerialized] public Stack<int> PastIndex;
        [NonSerialized] public bool Interactable;

        private void Start()
        {
            PastIndex = new Stack<int>();
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

            Index = defaultIndex;
            SetRotation(_angles[defaultIndex]);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Interactable)
            {
                return;
            }

            _beforeVec = eventData.position - (Vector2) transform.position;
            StopAllCoroutines();
            PastIndex.Clear();
            PastIndex.Push(Index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Interactable)
            {
                return;
            }

            var beforeAngle = transform.eulerAngles.z;

            var point = eventData.position - (Vector2) transform.position;

            var signedAngle = Vector2.SignedAngle(_beforeVec, point);
            var afterAngle = beforeAngle + signedAngle / angleWeight;
            afterAngle %= 360;
            transform.eulerAngles = new Vector3(0, 0, afterAngle);

            _beforeVec = point;

            var close = _angles.OrderBy(angle =>
                    Mathf.Abs(Vector2.SignedAngle(
                        new Vector2(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad)),
                        transform.up)))
                .Last();
            var closeIdx = Array.FindIndex(_angles, angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(close));

            if (PastIndex.Count == divCount)
            {
                PastIndex.Clear();
                PastIndex.Push(Index);
                Debug.Log("현재 Stack : " +
                          String.Join("", new List<int>(PastIndex).ConvertAll(i => i.ToString()).ToArray()) +
                          ", 추가 : " + closeIdx);
            }
            else if (PastIndex.Count == 0)
            {
                PastIndex.Push(Index);
            }


            if (PastIndex.Peek() != closeIdx)
            {
                if (PastIndex.Contains(closeIdx))
                {
                    while (PastIndex.Peek() != closeIdx)
                    {
                        PastIndex.Pop();
                    }
                }
                else
                    switch (signedAngle)
                    {
                        case < 0 when PastIndex.Contains((PastIndex.Peek() - 1 + divCount) % divCount):
                        {
                            Debug.Log("11");
                            PastIndex.Clear();
                            PastIndex.Push(Index);
                            while (PastIndex.Peek() != closeIdx)
                            {
                                PastIndex.Push((PastIndex.Peek() - 1 + divCount) % divCount);
                            }

                            break;
                        }
                        case > 0 when PastIndex.Contains((PastIndex.Peek() + 1 + divCount) % divCount):
                        {
                            Debug.Log("22");
                            PastIndex.Clear();
                            PastIndex.Push(Index);
                            while (PastIndex.Peek() != closeIdx)
                            {
                                PastIndex.Push((PastIndex.Peek() + 1) % divCount);
                            }

                            break;
                        }
                        case < 0:
                        {
                            // 오른쪽
                            // 6 -> 4 값이 작아짐   76 -> 5
                            while (PastIndex.Peek() != closeIdx)
                            {
                                PastIndex.Push((PastIndex.Peek() - 1 + divCount) % divCount);
                            }

                            break;
                        }
                        case > 0:
                        {
                            //왼쪽
                            // 4 -> 6 값이 커짐     67 -> 8
                            while (PastIndex.Peek() != closeIdx)
                            {
                                PastIndex.Push((PastIndex.Peek() + 1) % divCount);
                            }

                            break;
                        }
                    }
                // Debug.Log("현재 Stack : " +
                //           String.Join("", new List<int>(pastIdxs).ConvertAll(i => i.ToString()).ToArray()) +
                //           ", 추가 : " + closeIdx);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Interactable)
            {
                return;
            }

            var beforeAngle = transform.eulerAngles.z;

            var point = eventData.position - (Vector2) transform.position;

            var afterAngle = beforeAngle + Vector2.SignedAngle(_beforeVec, point) / angleWeight;
            afterAngle %= 360;
            transform.eulerAngles = new Vector3(0, 0, afterAngle);

            var close = _angles.OrderBy(angle =>
                    Mathf.Abs(Vector2.SignedAngle(
                        new Vector2(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad)),
                        transform.up)))
                .Last();

            var weight = Mathf.Lerp(.2f, .8f, Mathf.Abs(_size - Mathf.Abs(close - afterAngle)) / _size * 2);

            StartCoroutine(SetRotation(close, weight));
        }

        private IEnumerator SetRotation(float targetAngle, float weight)
        {
            Interactable = false;
            var targetVector = new Vector2(Mathf.Cos((targetAngle + 90) * Mathf.Deg2Rad),
                Mathf.Sin((targetAngle + 90) * Mathf.Deg2Rad));

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            Vector2 startVector = transform.up;
            while (t <= 1f)
            {
                var afterVector = Vector2.Lerp(startVector, targetVector, t);
                transform.up = afterVector;
                t += Time.fixedDeltaTime * weight * rotSpeed;
                yield return waitForFixedUpdate;
            }

            var idx = Array.FindIndex(_angles, angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(targetAngle));

            Interactable = true;
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
            Interactable = true;
            SetRotation(_angles[defaultIndex]);
            Index = Array.FindIndex(_angles,
                angle => Mathf.RoundToInt(angle) == Mathf.RoundToInt(_angles[defaultIndex]));
        }
    }
}
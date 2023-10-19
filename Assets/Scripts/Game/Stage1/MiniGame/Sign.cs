using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using Utility.Property;

namespace Game.Stage1.MiniGame
{
    public class Sign : Default.MiniGame
    {
        private enum Direction
        {
            None = 0,
            Up = 1,
            Left = 2,
            Down = 3,
            Right = 4
        }

        [Serializable]
        private class Edge
        {
            public Vector2 v1;
            public Vector2 v2;
        }

        private class Point
        {
            public Animator Animator;
            public Vector2 Pos;
        }

        [SerializeField] private GameObject prefab;
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private RectTransform form;
        [SerializeField] private Transform root;
        [SerializeField] private Image start;
        [SerializeField] private Vector2Int formSize;

        [SerializeField] private AudioClip arriveAudioClip;
        [SerializeField] private AudioClip moveOneAudioClip;
        
        [SerializeField] private bool isEnd;
        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Image end;
        
        [Header("Start Position")]
        [SerializeField] private Vector2Int startPos;
        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Vector2Int endPos;

        [Header("Setting Point")] [SerializeField] private Vector2Int startPoint;
        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Vector2Int endPoint;
        
        [ConditionalHideInInspector("isEnd")] [SerializeField]
        private float endTime;
        
        [SerializeField] private Edge[] edges;

        private Stack<Point> _path;
        private ObjectPool<Animator> _pointObjectPool;

        private static readonly int Before = Animator.StringToHash("Before");
        private static readonly int Next = Animator.StringToHash("Next");

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if (start)
            {
                start.transform.SetParent(root);
                start.transform.position = GetPointPosition(startPoint, start.GetComponent<RectTransform>().rect.size);
            }

            if (end)
            {
                end.transform.SetParent(root);
                end.transform.position = GetPointPosition(endPoint, end.GetComponent<RectTransform>().rect.size);
            }
        }

        protected override void End(bool isClear = true)
        {
            eventTrigger.triggers.Clear();
            base.End(isClear);
        }

        protected override void Init()
        {
            base.Init();
            _pointObjectPool = new ObjectPool<Animator>(() => Instantiate(prefab).GetComponent<Animator>(),
                animator => { animator.gameObject.SetActive(true); },
                animator => { animator.gameObject.SetActive(false); }, animator => { Destroy(animator.gameObject); });
            _path = new Stack<Point>();

            _path.Push(new Point {Pos = startPoint});
            PushPoint(startPos);

            if (isEnd)
            {
                StartCoroutine(EndTimer());
            }

            var pointerEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };

            pointerEvent.callback.AddListener(_ =>
            {
                var pointerEventData = _ as PointerEventData;
                var pos = (Vector2) form.position - form.sizeDelta / 2;

                var unitSize = form.sizeDelta / formSize;

                var xy = (pointerEventData.position - pos) / unitSize;
                xy.x = Mathf.FloorToInt(xy.x);
                xy.y = Mathf.FloorToInt(xy.y);

                if (_path.Any(item => Mathf.Approximately(Vector2.Distance(item.Pos, xy), 0)))
                {
                    while (_path.Peek().Pos != xy && _path.Count > 1)
                    {
                        var popItem = _path.Pop();
                        Debug.Log($"Pop - {popItem.Pos}. {_path.Count}");
                        _pointObjectPool.Release(popItem.Animator);
                        if (_path.Peek().Animator)
                        {
                            _path.Peek().Animator.SetInteger(Next, (int) Direction.None);
                        }
                    }
                }

                // Debug.Log($"{xy}, {IsEnable(xy)}");

                if (!IsEnable(xy))
                {
                    return;
                }

                // 이미 뚫은 Path인 경우 뒤로 돌리기

                PushPoint(xy);
            });

            eventTrigger.triggers.Add(pointerEvent);
        }

        private Vector2 GetPointPosition(Vector2 pos, Vector2 size)
        {
            var pivotPos = (Vector2) form.position - form.sizeDelta / 2;
            var unitSize = form.sizeDelta / formSize;
            var targetPos = pivotPos + new Vector2(unitSize.x * pos.x, unitSize.y * pos.y) + size / 2;
            return targetPos;
        }

        private bool IsEnable(Vector2 targetPos)
        {
            if (_path.Count == 1)
            {
                // Debug.Log($"IsEnable? {targetPos}, {startPos}");

                if (Mathf.Approximately(Vector2.Distance(targetPos, startPos), 0))
                {
                    return true;
                }

                return false;
            }

            var lastPoint = _path.Peek();

            if (lastPoint.Pos.x < 0 || lastPoint.Pos.y < 0 || lastPoint.Pos.x >= formSize.x ||
                lastPoint.Pos.y >= formSize.y)
            {
                return false;
            }

            var l1Distance =
                (int) (Mathf.Abs(lastPoint.Pos.x - targetPos.x) + Mathf.Abs(lastPoint.Pos.y - targetPos.y));
            if (l1Distance != 1)
            {
                return false;
            }

            Vector2 v1, v2;

            if (Mathf.Approximately(targetPos.x, lastPoint.Pos.x))
            {
                // 위아래로 가는 경우 y값 큰 값 기준으로 (x, y) (x + 1, y)
                var maxY = Mathf.Max(targetPos.y, lastPoint.Pos.y);
                v1.x = targetPos.x;
                v1.y = maxY;

                v2.x = targetPos.x + 1;
                v2.y = maxY;
            }
            else
            {
                // 좌우 이동인 경우 x값 큰 값 기준 (x, y), (x, y + 1)
                var maxX = Mathf.Max(targetPos.x, lastPoint.Pos.x);
                v1.x = maxX;
                v1.y = targetPos.y;

                v2.x = maxX;
                v2.y = targetPos.y + 1;
            }

            Debug.Log($"wall {v1} -> {v2}");

            return !edges.Any(item =>
            {
                if (Mathf.Approximately(targetPos.x, lastPoint.Pos.x))
                {
                    // 상하 이동인 경우 && 같은 y값만 검색 경우
                    if (!Mathf.Approximately(item.v1.y, item.v2.y) || !Mathf.Approximately(v1.y, item.v1.y))
                    {
                        return false;
                    }

                    Vector2 minV, maxV;
                    if (item.v1.x > item.v2.x)
                    {
                        minV = item.v2;
                        maxV = item.v1;
                    }
                    else
                    {
                        minV = item.v1;
                        maxV = item.v2;
                    }

                    Debug.Log($"edge {minV} -> {maxV}");

                    return (int) minV.x <= (int) v1.x && (int) maxV.x >= (int) v2.x;
                }
                else
                {
                    // Debug.Log($"좌우 - src: ({v1} - {v2}) - ({item.v1} - {item.v2})");
                    // 좌우 이동인 경우 && 같은 x값만 검색 경우
                    if (!Mathf.Approximately(item.v1.x, item.v2.x) || !Mathf.Approximately(v1.x, item.v1.x))
                    {
                        return false;
                    }

                    Vector2 minV, maxV;
                    if (item.v1.y > item.v2.y)
                    {
                        minV = item.v2;
                        maxV = item.v1;
                    }
                    else
                    {
                        minV = item.v1;
                        maxV = item.v2;
                    }

                    // Debug.Log($"edge {minV} -> {maxV}");

                    return (int) minV.y <= (int) v1.y && (int) maxV.y >= (int) v2.y;
                }
            });
        }

        private void PushPoint(Vector2 targetPos)
        {
            var beforePoint = _path.Peek();

            var dir = targetPos - beforePoint.Pos;

            Direction next = default, before = default;
            if (Mathf.Approximately(Vector2.Distance(dir, Vector2.up), 0))
            {
                next = Direction.Up;
                before = Direction.Down;
            }
            else if (Mathf.Approximately(Vector2.Distance(dir, Vector2.left), 0))
            {
                next = Direction.Left;
                before = Direction.Right;
            }
            else if (Mathf.Approximately(Vector2.Distance(dir, Vector2.down), 0))
            {
                next = Direction.Down;
                before = Direction.Up;
            }
            else if (Mathf.Approximately(Vector2.Distance(dir, Vector2.right), 0))
            {
                next = Direction.Right;
                before = Direction.Left;
            }

            if (beforePoint.Animator)
            {
                beforePoint.Animator.SetInteger(Next, (int) next);
            }

            if (!isEnd && Mathf.Approximately(Vector2.Distance(targetPos, endPoint), 0))
            {
                // EndPoint는 이미 있으므로 생성 X
                End();
            }
            else
            {
                var point = new Point {Animator = _pointObjectPool.Get(), Pos = targetPos};
                point.Animator.transform.SetParent(root);
                point.Animator.transform.position =
                    GetPointPosition(targetPos, point.Animator.GetComponent<RectTransform>().rect.size);
                point.Animator.SetInteger(Before, (int) before);

                _path.Push(point);
                Debug.Log($"Push {targetPos}, {_path.Count}");
                
                // endPos -> endPoint로 연결 후 자동 종료
                if (!isEnd && Mathf.Approximately(Vector2.Distance(targetPos, endPos), 0))
                {
                    PushPoint(endPoint);
                }
            }
        }

        private IEnumerator EndTimer()
        {
            var t = 0f;
            while (t <= endTime)
            {
                t += Time.deltaTime;
                yield return null;
            }

            End();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Property;
using Utility.Util;

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
            public Vector2Int Pos;
        }

        [SerializeField] private GameObject prefab;
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private RectTransform form;
        [SerializeField] private Image start;

        [SerializeField] private Vector2Int formSize;

        // [SerializeField] private int pointSize;
        [SerializeField] private float lineOffset;

        [SerializeField] private AudioData arriveAudioData;
        [SerializeField] private AudioData moveOneAudioData;

        [SerializeField] private bool isEnd;

        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Image end;

        [Header("Start Position")] [SerializeField]
        private Vector2Int startPos;

        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Vector2Int endPos;

        [Header("Setting Point")] [SerializeField]
        private Vector2Int startPoint;

        [ConditionalHideInInspector("isEnd", true)] [SerializeField]
        private Vector2Int endPoint;

        [ConditionalHideInInspector("isEnd")] [SerializeField]
        private float endTime;

        [SerializeField] private Edge[] edges;

        private Stack<Point> _path;
        private ObjectPool<Animator> _pointObjectPool;
        private Image _formImage;

        private Vector2 PointSize => (form.sizeDelta - LineOffset) / formSize;

        // private Vector2 UnitSize => PointSize - LineOffset;
        
        private Vector2 PivotPos => form.position * Operators.WindowToCanvasVector2 - form.sizeDelta / 2 + LineOffset / 2;

        private Vector2 LineOffset
        {
            get
            {
                if (_formImage == null)
                {
                    _formImage = form.GetComponent<Image>();
                }

                var ratio = form.sizeDelta / _formImage.sprite.rect.size;

                return lineOffset * ratio;
            }
        }

        private static readonly int Before = Animator.StringToHash("Before");
        private static readonly int Next = Animator.StringToHash("Next");

        private void OnValidate()
        {
            if (_path != null)
            {
                foreach (var point in _path)
                {
                    var rectTransform = point.Animator.transform as RectTransform;
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = PointSize;
                        rectTransform.SetParent(form);
                        rectTransform.anchoredPosition = GetPointAnchoredPosition(point.Pos);
                    }
                }
            }

            if (start)
            {
                var rectTransform = start.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = PointSize;
                    rectTransform.SetParent(form);
                    rectTransform.anchoredPosition = GetPointAnchoredPosition(startPoint);
                }
            }

            if (end)
            {
                var rectTransform = end.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = PointSize;
                    rectTransform.SetParent(form);
                    rectTransform.anchoredPosition = GetPointAnchoredPosition(endPoint);
                }
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

            EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.Drag, _ =>
            {
                var pointerEventData = _ as PointerEventData;
                
                var xy = (pointerEventData.position * Operators.WindowToCanvasVector2 - PivotPos) / PointSize;
                var point = new Vector2Int(Mathf.FloorToInt(xy.x), Mathf.FloorToInt(xy.y));

                if (_path.Any(item => item.Pos == point))
                {
                    while (_path.Peek().Pos != point && _path.Count > 1)
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

                if (!IsEnable(point))
                {
                    return;
                }

                // 이미 뚫은 Path인 경우 뒤로 돌리기

                PushPoint(point);
            });
        }

        private Vector2 GetPointAnchoredPosition(Vector2Int pointPos)
        {
            Debug.Log($"pos - {pointPos}, PointSize - {PointSize}");
            var targetPos = new Vector2(PointSize.x * pointPos.x, PointSize.y * pointPos.y) + PointSize / 2 + LineOffset / 2;

            return targetPos;
        }

        private bool IsEnable(Vector2Int targetPos)
        {
            if (_path.Count == 1)
            {
                // Debug.Log($"IsEnable? {targetPos}, {startPos}");
                if (targetPos == startPos)
                {
                    return true;
                }

                return false;
            }

            if (targetPos.y < 0 || targetPos.y > formSize.y || targetPos.x < 0 || targetPos.x > formSize.x)
            {
                return false;
            }

            var lastPoint = _path.Peek();

            if (lastPoint.Pos.x < 0 || lastPoint.Pos.y < 0 || lastPoint.Pos.x >= formSize.x ||
                lastPoint.Pos.y >= formSize.y)
            {
                return false;
            }

            var l1Distance = Mathf.Abs(lastPoint.Pos.x - targetPos.x) + Mathf.Abs(lastPoint.Pos.y - targetPos.y);
            if (l1Distance != 1)
            {
                return false;
            }

            Vector2 v1, v2;

            if (targetPos.x == lastPoint.Pos.x)
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

        private void PushPoint(Vector2Int targetPos)
        {
            var beforePoint = _path.Peek();

            var dir = targetPos - beforePoint.Pos;

            Direction next = default, before = default;
            if (dir == Vector2.up)
            {
                next = Direction.Up;
                before = Direction.Down;
            }
            else if (dir == Vector2.left)
            {
                next = Direction.Left;
                before = Direction.Right;
            }
            else if (dir == Vector2.down)
            {
                next = Direction.Down;
                before = Direction.Up;
            }
            else if (dir == Vector2Int.right)
            {
                next = Direction.Right;
                before = Direction.Left;
            }

            if (beforePoint.Animator)
            {
                beforePoint.Animator.SetInteger(Next, (int) next);
            }

            if (!isEnd && targetPos == endPoint)
            {
                // EndPoint는 이미 있으므로 생성 X
                End();
                arriveAudioData.Play();
            }
            else
            {
                var point = new Point {Animator = _pointObjectPool.Get(), Pos = targetPos};
                var rectTransform = point.Animator.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = PointSize;
                    rectTransform.SetParent(form);
                    rectTransform.localScale = Vector3.one;
                    rectTransform.anchoredPosition = GetPointAnchoredPosition(point.Pos);
                }

                point.Animator.SetInteger(Before, (int) before);

                _path.Push(point);
                Debug.Log($"Push {point.Pos}, {_path.Count}");
                moveOneAudioData.Play();

                // endPos -> endPoint로 연결 후 자동 종료
                if (!isEnd && point.Pos == endPos)
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
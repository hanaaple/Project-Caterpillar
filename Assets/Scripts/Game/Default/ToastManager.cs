using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Default
{
    public class ToastManager : MonoBehaviour
    {
        [SerializeField] private Transform toastMessageParent;
        [SerializeField] private float textSec;
        
        private Queue<string> _toastQueue;
        private Animator _toastMessageParentAnimator;
        
        private Coroutine _toastCoroutine;
        private Coroutine _toastDisappearCoroutine;

        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int IndexHash = Animator.StringToHash("Index");
        private static readonly int DisAppearHash = Animator.StringToHash("DisAppear");

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _toastQueue = new Queue<string>();
            _toastMessageParentAnimator = toastMessageParent.GetComponent<Animator>();
        }
        
        public void EnQueue(string toastContent)
        {
            _toastQueue.Enqueue(toastContent);
            
            
            _toastCoroutine ??= StartCoroutine(StartToast());
        }
        
        private GameObject GetToastMessage()
        {
            for (var i = 0; i < toastMessageParent.childCount; i++)
            {
                if (!toastMessageParent.GetChild(i).gameObject.activeSelf)
                {
                    return toastMessageParent.GetChild(i).gameObject;
                }
            }

            var toastMessage = Instantiate(toastMessageParent.GetChild(0).gameObject, toastMessageParent);

            return toastMessage;
        }

        private IEnumerable<Animator> GetActiveToastMessages()
        {
            var toastMessages = new List<Animator>();
            for (var i = 0; i < toastMessageParent.childCount; i++)
            {
                if (toastMessageParent.GetChild(i).gameObject.activeSelf)
                {
                    toastMessages.Add(toastMessageParent.GetChild(i).GetComponent<Animator>());
                }
            }

            return toastMessages.ToArray();
        }

        private IEnumerator StartToast()
        {
            Debug.Log("StartToast");
            if (!toastMessageParent.gameObject.activeSelf)
            {
                toastMessageParent.gameObject.SetActive(true);
            }

            while (_toastQueue.Count > 0)
            {
                var toasts = GetActiveToastMessages();
                foreach (var t in toasts)
                {
                    var animator = t.GetComponent<Animator>();
                    var index = animator.GetInteger(IndexHash);
                    animator.SetInteger(IndexHash, index + 1);

                    if (index > 2)
                    {
                        t.gameObject.SetActive(false);
                    }
                }

                var toastMessage = GetToastMessage();
                var toastAnimator = toastMessage.GetComponent<Animator>();
                var toastText = toastMessage.GetComponentInChildren<TMP_Text>(true);
                toastText.text = "";

                toastMessage.transform.SetAsLastSibling();
                toastMessage.gameObject.SetActive(true);
                toastAnimator.SetInteger(IndexHash, 0);

                _toastMessageParentAnimator.SetTrigger(ResetHash);

                yield return new WaitUntil(() => toastAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));


                // 전부 늘어날때까지 대기

                // 전부 보이면 Text Print
                var toastContent = _toastQueue.Dequeue();
                var waitTextSec = new WaitForSeconds(textSec);
                foreach (var t in toastContent)
                {
                    toastText.text += t;
                    yield return waitTextSec;
                }

                // 타이핑 완료 후??
                // Reset and Start Disappear

                _toastMessageParentAnimator.SetTrigger(DisAppearHash);

                _toastDisappearCoroutine ??= StartCoroutine(ToastDisappear());
            }

            _toastCoroutine = null;
        }

        private IEnumerator ToastDisappear()
        {
            yield return new WaitUntil(() => _toastMessageParentAnimator.GetCurrentAnimatorStateInfo(0).IsName("End"));
            // 전부 사라지게 만들기.
            _toastDisappearCoroutine = null;
            toastMessageParent.gameObject.SetActive(false);

            Debug.Log("Toast 종료");

            for (var idx = 0; idx < toastMessageParent.childCount; idx++)
            {
                toastMessageParent.GetChild(idx).gameObject.SetActive(false);
            }
        }
    }
}
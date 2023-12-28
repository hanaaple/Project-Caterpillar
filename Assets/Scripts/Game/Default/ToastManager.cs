using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility.Audio;
using Utility.Util;

namespace Game.Default
{
    public class ToastManager : MonoBehaviour
    {
        // private enum ToastType
        // {
        //     Dark = 0,
        //     Light = 1
        // }
        
        // [SerializeField] private ToastType toastType;
        [SerializeField] private GameObject toastMessagePrefab;
        [SerializeField] private Transform toastMessageParent;
        [SerializeField] private float textSec;

        [SerializeField] private AudioData typingAudioData;
        
        public Action onToastEnd;

        private Queue<string> _toastQueue;
        private Animator _toastMessageParentAnimator;

        private Coroutine _toastCoroutine;
        private Coroutine _toastDisappearCoroutine;

        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int IndexHash = Animator.StringToHash("Index");
        private static readonly int DisAppearHash = Animator.StringToHash("DisAppear");
        // private static readonly int TypeHash = Animator.StringToHash("Type");
        private static readonly int PlayHash = Animator.StringToHash("Play");

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _toastQueue = new Queue<string>();
            _toastMessageParentAnimator = toastMessageParent.GetComponent<Animator>();

            toastMessageParent.GetComponent<RectTransform>().sizeDelta = new Vector2(580,
                10 + toastMessagePrefab.GetComponent<RectTransform>().sizeDelta.y * 3);
        }

        public void Enqueue(string toastContent)
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

            // ObjectPoolHelper.Instance.Getgff<>()
            var toastMessage = Instantiate(toastMessagePrefab, toastMessageParent);

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

            typingAudioData.Play();
            
            while (_toastQueue.Count > 0)
            {
                var toasts = GetActiveToastMessages();
                foreach (var toast in toasts)
                {
                    var index = toast.GetInteger(IndexHash);
                    toast.SetInteger(IndexHash, index + 1);

                    if (index > 2)
                    {
                        toast.gameObject.SetActive(false);
                    }
                }

                var toastMessage = GetToastMessage();
                var toastAnimator = toastMessage.GetComponent<Animator>();
                var toastText = toastMessage.GetComponentInChildren<TMP_Text>(true);
                toastText.text = "";

                toastMessage.transform.SetAsLastSibling();
                toastMessage.gameObject.SetActive(true);
                toastAnimator.SetInteger(IndexHash, 0);
                // toastAnimator.SetInteger(TypeHash, (int) toastType);
                toastAnimator.SetTrigger(PlayHash);

                _toastMessageParentAnimator.SetTrigger(ResetHash);

                yield return new WaitUntil(() => toastAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));


                // 전부 늘어날때까지 대기

                // 전부 보이면 Text Print
                var toastContent = _toastQueue.Dequeue();
                
                for (var index = 0; index < toastContent.Length; index++)
                {
                    var t = toastContent[index];
                    if (t.Equals('<'))
                    {
                        while (!t.Equals('>'))
                        {
                            toastText.text += t;

                            index++;
                            t = toastContent[index];
                        }

                        toastText.text += t;

                        index++;
                    }

                    toastText.text += toastContent[index];

                    if (!t.Equals(' '))
                    {
                        yield return YieldInstructionProvider.WaitForSeconds(textSec);
                    }
                }

                _toastMessageParentAnimator.SetTrigger(DisAppearHash);

                _toastDisappearCoroutine ??= StartCoroutine(ToastDisappear());
            }

            _toastCoroutine = null;
            typingAudioData.Stop();
            onToastEnd?.Invoke();
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
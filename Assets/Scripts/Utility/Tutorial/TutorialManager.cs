using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.Scene;
using Utility.Util;

namespace Utility.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Image tutorialImage;
        [SerializeField] private Animator pressKeyAnimator;
        [SerializeField] private AudioData activeAudioData;
        [SerializeField] private AudioData turnPageAudioData;

        private InputActions _inputActions;
        private TutorialHelper _tutorialHelper;
        private Action _onEndAction;

        private static readonly int IsPressHash = Animator.StringToHash("IsPress");

        private void Awake()
        {
            _inputActions = new InputActions(nameof(TutorialManager))
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause?.Invoke(); },
                OnInteractPerformed = () =>
                {
                    pressKeyAnimator.SetBool(IsPressHash, true);
                    if (_tutorialHelper.GetIsEnd())
                    {
                        SceneLoader.Instance.onLoadScene -= EndTutorial;
                        TimeScaleHelper.Pop();
                        InputManager.PopInputAction(_inputActions);
                        tutorialPanel.SetActive(false);
                        _onEndAction?.Invoke();
                        _onEndAction = null;
                    }
                    else
                    {
                        turnPageAudioData.Play();
                        _tutorialHelper.StartNext();
                    }
                },
                OnInteractCanceled = () => { pressKeyAnimator.SetBool(IsPressHash, false); }
            };
        }

        public void StartTutorial(TutorialHelper tutorialHelper, Action onEndAction)
        {
            _onEndAction = onEndAction;
            _tutorialHelper = tutorialHelper;

            _tutorialHelper.Init(tutorialImage);
            tutorialPanel.SetActive(true);
            pressKeyAnimator.SetBool(IsPressHash, false);
            InputManager.PushInputAction(_inputActions);
            TimeScaleHelper.Push(0f);
            activeAudioData.Play();

            SceneLoader.Instance.onLoadScene += EndTutorial;
        }

        private void EndTutorial()
        {
            if (_onEndAction != null)
            {
                TimeScaleHelper.Pop();
                InputManager.PopInputAction(_inputActions);
                tutorialPanel.SetActive(false);
                _onEndAction = null;
            }
        }
    }
}
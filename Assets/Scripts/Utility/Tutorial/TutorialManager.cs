using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.Util;

namespace Utility.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private GameObject tutorialPanel;
    
        [SerializeField] private Image tutorialImage;
    
        private InputActions _inputActions;

        private TutorialHelper _tutorialHelper;

        private Action _onEndAction;

        private void Awake()
        {
            _inputActions = new InputActions(nameof(TutorialManager))
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause?.Invoke(); },
                OnInteract = () =>
                {
                    if (_tutorialHelper.GetIsEnd())
                    {
                        TimeScaleHelper.Pop();
                        InputManager.PopInputAction(_inputActions);
                        tutorialPanel.SetActive(false);
                        _onEndAction?.Invoke();
                    }
                    else
                    {
                        _tutorialHelper.StartNext();
                    }
                }
            };
        }

        public void StartTutorial(TutorialHelper tutorialHelper, Action onEndAction)
        {
            _onEndAction = onEndAction;
            _tutorialHelper = tutorialHelper;
        
            _tutorialHelper.Init(tutorialImage);
            tutorialPanel.SetActive(true);
            InputManager.PushInputAction(_inputActions);
            TimeScaleHelper.Push(0f);
        }
    }
}

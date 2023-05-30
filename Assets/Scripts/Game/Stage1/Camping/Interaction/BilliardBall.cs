using System;
using Game.Default;
using UnityEngine;
using UnityEngine.UI;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction
{
    [Serializable]
    public class BilliardBallToastData : ToastData
    {
        public enum ToastType
        {
            Appear,
            Icon,
            Reset
        }

        public ToastType toastType;
    }
    
    [Serializable]
    public class BilliardBallIcon
    {
        /// <summary>
        /// Use for Reset check or Set Animator Trigger
        /// </summary>
        public enum BilliardBallType
        {
            Camping,
            Tree,
            Rock,
            Reset
        }

        public BilliardBallType billiardBallType;
        [Header("Up, Down, Left, Right")]
        public Vector4 clearVector;
        
        public bool Check(Vector4 input)
        {
            if (billiardBallType == BilliardBallType.Reset)
            {
                if (input == Vector4.zero || (input.x >= clearVector.x && input.y >= clearVector.y && input.z >= clearVector.z && input.w >= clearVector.w))
                {
                    return true;
                }
            }
            else
            {
                if (input == clearVector)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class BilliardBall : CampingInteraction
    {
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private Button exitButton;
        [SerializeField] private Animator billiardBallAnimator;

        [Header("당구공")] [SerializeField] private Button up;
        [SerializeField] private Button down;
        [SerializeField] private Button right;
        [SerializeField] private Button left;
        [SerializeField] private Button center;

        [SerializeField] private BilliardBallIcon[] billiardBallIcons;
        
        [SerializeField] private BilliardBallToastData[] toastData;
        
        private Vector4 _inputVector;

        private void OnMouseUp()
        {
            var appearToastData =
                Array.Find(toastData, item => item.toastType == BilliardBallToastData.ToastType.Appear);
            if (!appearToastData.isToasted)
            {
                appearToastData.isToasted = true;

                foreach (var toastContent in appearToastData.toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(toastContent);
                }
            }
            setInteractable(false);
            uiPanel.SetActive(true);
            ResetInteraction();
            Appear();
        }

        private void Start()
        {
            up.onClick.AddListener(() => { PushInput(new Vector4(1, 0, 0, 0)); });
            down.onClick.AddListener(() => { PushInput(new Vector4(0, 1, 0, 0)); });
            left.onClick.AddListener(() => { PushInput(new Vector4(0, 0, 1, 0)); });
            right.onClick.AddListener(() => { PushInput(new Vector4(0, 0, 0, 1)); });
            center.onClick.AddListener(() =>
            {
                var resetToastData =
                    Array.Find(toastData, item => item.toastType == BilliardBallToastData.ToastType.Reset);
                if (!resetToastData.isToasted)
                {
                    resetToastData.isToasted = true;

                    foreach (var toastContent in resetToastData.toastContents)
                    {
                        SceneHelper.Instance.toastManager.Enqueue(toastContent);
                    }
                }

                _inputVector = Vector4.zero;
                UpdateUI();
            });
            
            exitButton.onClick.AddListener(() =>
            {
                uiPanel.SetActive(false);
                setInteractable(true);
            });

            ResetInteraction();
        }

        private void PushInput(Vector4 input)
        {
            _inputVector += input;
            UpdateUI();
        }

        private void UpdateUI()
        {
            var billiardBallIcon = Array.Find(billiardBallIcons, item => item.Check(_inputVector));
            if (billiardBallIcon == null)
            {
                return;
            }

            if (billiardBallIcon.billiardBallType == BilliardBallIcon.BilliardBallType.Reset)
            {
                billiardBallAnimator.SetTrigger("Reset");
                _inputVector = Vector4.zero;
            }
            else
            {
                billiardBallAnimator.SetTrigger(billiardBallIcon.billiardBallType.ToString());

                var iconToastData = Array.Find(toastData,
                    item => item.toastType == BilliardBallToastData.ToastType.Icon);
                if (iconToastData.isToasted)
                {
                    return;
                }
                iconToastData.isToasted = true;

                foreach (var toastContent in iconToastData.toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(toastContent);
                }
            }
        }

        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void ResetInteraction()
        {
            
        }
    }
}
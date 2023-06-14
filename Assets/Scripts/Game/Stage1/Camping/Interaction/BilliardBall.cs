using System;
using Game.Default;
using Game.Stage1.Camping.Interaction.Show;
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
        public enum IconType
        {
            Mountain,
            Tree,
            Rock,
            Lake,
            MosquitoRepellent
        }

        public IconType iconType;
        public Vector2 iconPos;
    }

    public class BilliardBall : CampingInteraction
    {
        [SerializeField] private ShowPanel showPanel;
        [SerializeField] private Animator billiardBallAnimator;

        [Header("당구공")] [SerializeField] private Button up;
        [SerializeField] private Button down;
        [SerializeField] private Button right;
        [SerializeField] private Button left;
        [SerializeField] private Button center;

        [SerializeField] private BilliardBallIcon[] billiardBallIcons;

        [SerializeField] private BilliardBallToastData[] toastData;

        private Vector2 _pos;
        
        private static readonly int DownHash = Animator.StringToHash("Down");
        private static readonly int UpHash = Animator.StringToHash("Up");
        private static readonly int LeftHash = Animator.StringToHash("Left");
        private static readonly int RightHash = Animator.StringToHash("Right");
        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int DefaultHash = Animator.StringToHash("Default");

        private void OnMouseUp()
        {
            var appearToastData =
                Array.Find(toastData, item => item.toastType == BilliardBallToastData.ToastType.Appear);
            if (!appearToastData.IsToasted)
            {
                appearToastData.IsToasted = true;

                foreach (var toastContent in appearToastData.toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(toastContent);
                }
            }

            setInteractable(false);
            
            _pos = Vector2.one;
            UpdateUI();
            showPanel.Show();
            Appear();
        }

        private void Start()
        {
            up.onClick.AddListener(() =>
            {
                billiardBallAnimator.SetTrigger(UpHash);
                PushInput(Vector2.down);
            });
            down.onClick.AddListener(() =>
            {
                billiardBallAnimator.SetTrigger(DownHash);
                PushInput(Vector2.up);
            });
            left.onClick.AddListener(() =>
            {
                billiardBallAnimator.SetTrigger(LeftHash);
                PushInput(Vector2.left);
            });
            right.onClick.AddListener(() =>
            {
                billiardBallAnimator.SetTrigger(RightHash);
                PushInput(Vector2.right);
            });
            center.onClick.AddListener(() =>
            {
                billiardBallAnimator.SetTrigger(ResetHash);
                var resetToastData =
                    Array.Find(toastData, item => item.toastType == BilliardBallToastData.ToastType.Reset);
                if (!resetToastData.IsToasted)
                {
                    resetToastData.IsToasted = true;

                    foreach (var toastContent in resetToastData.toastContents)
                    {
                        SceneHelper.Instance.toastManager.Enqueue(toastContent);
                    }
                }

                _pos = Vector2.one;
                UpdateUI();
            });

            showPanel.exitButton.onClick.AddListener(() =>
            {
                showPanel.Hide();
                setInteractable(true);
            });
        }

        private void PushInput(Vector2 input)
        {
            _pos += input;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (IsReset())
            {
                billiardBallAnimator.SetTrigger(ResetHash);
                _pos = Vector2.one;
            }
            Debug.Log($"위치: {_pos}");

            var billiardBallIcon = Array.Find(billiardBallIcons,
                item => Mathf.Approximately(Vector2.Distance(item.iconPos, _pos), 0f));
            if (billiardBallIcon == null)
            {
                billiardBallAnimator.SetTrigger(DefaultHash);
                return;
            }

            billiardBallAnimator.SetTrigger(billiardBallIcon.iconType.ToString());

            var iconToastData = Array.Find(toastData, item => item.toastType == BilliardBallToastData.ToastType.Icon);
            if (iconToastData.IsToasted)
            {
                return;
            }

            iconToastData.IsToasted = true;

            foreach (var toastContent in iconToastData.toastContents)
            {
                SceneHelper.Instance.toastManager.Enqueue(toastContent);
            }
        }

        private bool IsReset()
        {
            return (int)_pos.x < 0 || (int)_pos.x > 4 || (int)_pos.y < 0 || (int)_pos.y > 4;
        }
    }
}
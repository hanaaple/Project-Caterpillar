using UnityEngine;
using UnityEngine.UI;

namespace Game.Camping
{
    public class BilliardBall : CampingInteraction
    {
        [SerializeField]
        private GameObject gamePanel;
        
        [SerializeField]
        private Button up;
        [SerializeField]
        private Button down;
        [SerializeField]
        private Button right;
        [SerializeField]
        private Button left;
        
        [SerializeField]
        private GameObject billiardBallCheck;

        private Vector2 _previousInput;

        [SerializeField]
        private Button exitButton;

        private void OnMouseDown()
        {
            setEnable(false);
            gamePanel.SetActive(true);
            UpdateUI(Vector2.zero);
            Appear();
        }

        private void Start()
        {
            up.onClick.AddListener(() =>
            {
                UpdateUI(Vector2.up);
            });
            down.onClick.AddListener(() =>
            {
                UpdateUI(Vector2.down);
            });
            left.onClick.AddListener(() =>
            {
                UpdateUI(Vector2.left);
            });
            right.onClick.AddListener(() =>
            {
                UpdateUI(Vector2.right);
            });
            
            exitButton.onClick.AddListener(() =>
            {
                gamePanel.SetActive(false);
                setEnable(true);
            });
            
            Reset();
        }

        private void UpdateUI(Vector2 input)
        {
            if (_previousInput == input || input == Vector2.up || input == Vector2.zero)
            {
                billiardBallCheck.SetActive(false);
            }
            else
            {
                billiardBallCheck.SetActive(true);
            }

            _previousInput = input;
        }
        
        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void Reset()
        {
            UpdateUI(Vector2.zero);
        }
    }
}

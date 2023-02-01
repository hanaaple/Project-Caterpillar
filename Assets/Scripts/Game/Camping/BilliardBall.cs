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
        
        [Space(10)]
        [SerializeField]
        private Image billiardBall;
        [SerializeField]
        private Sprite defaultSprite;
        [SerializeField]
        private Sprite upSprite;
        [SerializeField]
        private Sprite downSprite;
        [SerializeField]
        private Sprite rightSprite;
        [SerializeField]
        private Sprite leftSprite;
        
        [Space(5)]
        
        [SerializeField]
        private GameObject billiardBallCheck;

        private Vector2 _previousInput;

        [SerializeField]
        private Button exitButton;

        private void OnMouseUp()
        {
            setInteractable(false);
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
                setInteractable(true);
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

            if (input == Vector2.up)
            {
                billiardBall.sprite = upSprite;
            }else if (input == Vector2.down)
            {
                billiardBall.sprite = downSprite;
            }else if (input == Vector2.right)
            {
                billiardBall.sprite = rightSprite;
            }else if (input == Vector2.left)
            {
                billiardBall.sprite = leftSprite;
            }else if (input == Vector2.zero)
            {
                billiardBall.sprite = defaultSprite;
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

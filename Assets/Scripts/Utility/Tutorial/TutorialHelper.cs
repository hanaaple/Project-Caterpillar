using UnityEngine;
using UnityEngine.UI;

namespace Utility.Tutorial
{
    public class TutorialHelper : MonoBehaviour
    {
        [SerializeField] private Sprite[] tutorialSprites;

        private int _index;
        private Image _tutorialImage;

        public void Init(Image image)
        {
            _index = 0;
            _tutorialImage = image;
            _tutorialImage.sprite = tutorialSprites[_index];
        }

        public void StartNext()
        {
            _index++;
            _tutorialImage.sprite = tutorialSprites[_index];
        }

        public bool GetIsExecuted()
        {
            // 흠
            return true;
        }
        
        public bool GetIsEnd()
        {
            return _index + 1 >= tutorialSprites.Length;
        }
    }
}
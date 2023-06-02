using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.BeachGame
{
    public class FragmentAlbumPicture : AlbumPicture
    {
        [SerializeField] private Image defaultImage;
        [SerializeField] private Image activeImage;
        [SerializeField] private Sprite[] clearSprites;

        public override void SetPanel(int idx)
        {
            if (clearSprites.Contains(PanelButton.image.sprite))
            {
                SetPanel(PictureState.Clear);
            }
            else
            {
                Debug.Log("끝");
                PanelButton.image.sprite = clearSprites[idx];
                defaultImage.sprite = clearSprites[idx];
                activeImage.sprite = clearSprites[idx];
            }
        }
    }
}

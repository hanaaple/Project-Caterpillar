using System.Linq;
using UnityEngine;

namespace Game.Stage1.BeachGame
{
    public class FragmentAlbumPicture : AlbumPicture
    {
        [SerializeField] private Sprite[] clearSprites;

        public override void SetPanel(int idx)
        {
            if (clearSprites.Contains(PanelButton.image.sprite))
            {
                Debug.Log("아직 1개 남음");
                SetPanel(PictureState.Clear);
            }
            else
            {
                Debug.Log("끝");
                PanelButton.image.sprite = clearSprites[idx];
            }
        }
    }
}

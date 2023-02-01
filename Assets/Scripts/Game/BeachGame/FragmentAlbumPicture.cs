using System.Linq;
using UnityEngine;

public class FragmentAlbumPicture : AlbumPicture
{
    [SerializeField] private Sprite[] clearSprites;

    public override void SetPanel(int idx)
    {
        if (clearSprites.Contains(panelButton.image.sprite))
        {
            Debug.Log("아직 1개 남음");
            SetPanel(PictureState.Clear);
        }
        else
        {
            Debug.Log("끝");
            panelButton.image.sprite = clearSprites[idx];
        }
    }
}

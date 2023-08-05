using UnityEngine;
using UnityEngine.UI;

public class MusicBtn : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;
    private int volume;

    void Awake()
    {
        GetVolume();
        SetButtonImage();
        SetSound();
    }

    void GetVolume()
    {
        volume = PlayerPrefs.GetInt("Music");
    }

    void SetVolume()
    {
        PlayerPrefs.SetInt("Music", volume);
    }

    void SetButtonImage()
    {
        image.sprite = sprites[volume];
    }

    void SetSound()
    {
        FindObjectOfType<MakeNoise>().MusicOnOff(volume == 1);
    }

    public void ChangeSound()
    {
        if (volume == 0)
            volume = 1;
        else
            volume = 0;

        SetVolume();
        SetButtonImage();
        SetSound();
    }
}

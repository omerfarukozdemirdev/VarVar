using UnityEngine;
using UnityEngine.UI;

public class SoundBtn : MonoBehaviour
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
        volume = PlayerPrefs.GetInt("Sound");
    }

    void SetVolume()
    {
        PlayerPrefs.SetInt("Sound", volume);
    }

    void SetButtonImage()
    {
        image.sprite = sprites[volume];
    }

    void SetSound()
    {
        FindObjectOfType<MakeNoise>().SetSoundVolume(volume);
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

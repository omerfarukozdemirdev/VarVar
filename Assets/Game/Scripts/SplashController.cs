using UnityEngine;

public class SplashController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    public void GuestPlay()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(9,0);

        if(!PlayerPrefs.HasKey("MoneyCount"))
            PlayerPrefs.SetInt("MoneyCount", 10000);

        if(!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);

        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}

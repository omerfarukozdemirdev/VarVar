using UnityEngine;

public class LoginController : MonoBehaviour
{
    private void Awake()
    {
        //if (!PlayerPrefs.HasKey("MoneyCount"))
        //    PlayerPrefs.SetInt("MoneyCount", 10000);

        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);

        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);

       UnityEngine.SceneManagement.SceneManager.LoadScene(2);       
    }

    public void GuestPlay()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(26, 0);

        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
}

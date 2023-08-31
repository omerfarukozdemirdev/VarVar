using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
    [SerializeField] Image loadingBar;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (!PlayerPrefs.HasKey("MoneyCount"))
            PlayerPrefs.SetInt("MoneyCount", 10000);

        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);

        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);

        StartCoroutine(LoadingBar());

    }

    IEnumerator LoadingBar()
    {
        for(int i = 0; i < 10; i++)
        {
            loadingBar.fillAmount = (float)(i * .1f);
            yield return new WaitForSecondsRealtime(.1f);
        }

        LoginScene();
    }

    void LoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}

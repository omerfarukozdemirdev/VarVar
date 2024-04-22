using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    private AsyncOperation asyncLoad;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (!PlayerPrefs.HasKey("MoneyCount"))
            PlayerPrefs.SetInt("MoneyCount", 5000);

        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);

        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);

        //StartCoroutine(LoadingBar());
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Start loading the scene asynchronously
        asyncLoad = SceneManager.LoadSceneAsync("Menu");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous operation is complete
        while (asyncLoad.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Normalize progress to a range of 0 to 1
            loadingBar.fillAmount = progress;

            yield return null; // Wait for the next frame
        }

        loadingBar.fillAmount = 1;
        // The scene is now loaded and you can perform any additional setup or logic
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true; // Activate the loaded scene
        }
    }
}

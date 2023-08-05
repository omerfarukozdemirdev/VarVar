using System.Collections;
using UnityEngine;

public class SettingsBtn : MonoBehaviour
{
    [SerializeField] private GameObject[] buttons;
    private bool buttonsIn;

    private void Start()
    {
        DeactivateButtons();
    }

    void DeactivateButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].SetActive(false);
    }

    public void SettingsBtnClicked()
    {
        buttonsIn = !buttonsIn;

        if (buttonsIn)
        {
            StopAllCoroutines();
            StartCoroutine(ButtonsIn());
            FindObjectOfType<MakeNoise>().PlaySFX(23, 0);
            return;
        }

        FindObjectOfType<MakeNoise>().PlaySFX(24, 0);
        DeactivateButtons();
    }

    IEnumerator ButtonsIn()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(true);
            yield return new WaitForSecondsRealtime(.1f);
        }
    }
}

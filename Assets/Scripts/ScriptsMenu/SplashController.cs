using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashController : MonoBehaviour
{
    public CanvasGroup[] screens;   // cada pantalla (agrega CanvasGroup a cada Image)
    public float fadeTime = 1f;
    public float displayTime = 2f;
    public GameObject mainMenu;

    void Start()
    {
        StartCoroutine(PlayScreens());
    }

    IEnumerator PlayScreens()
    {
        foreach (CanvasGroup cg in screens)
        {
            cg.alpha = 0;
            cg.gameObject.SetActive(true);

            // Fade In
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                cg.alpha = t / fadeTime;
                yield return null;
            }
            cg.alpha = 1;

            yield return new WaitForSeconds(displayTime);

            // Fade Out
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                cg.alpha = 1 - (t / fadeTime);
                yield return null;
            }
            cg.alpha = 0;

            cg.gameObject.SetActive(false);
        }

        mainMenu.SetActive(true);
    }
}

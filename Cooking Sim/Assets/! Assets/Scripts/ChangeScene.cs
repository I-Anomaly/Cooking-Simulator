using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{

    private void Start() => Fader.FadeIn();

    public void ChangeToSpecificScene(string sceneName)
    {
        StartCoroutine(FadeSceneChange(sceneName));
    }

    public IEnumerator FadeSceneChange(string sceneName)
    {
        Fader.FadeOut();
        while (Fader.isFading) yield return null; // wait until fade is complete
        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator FadeSceneChangeWithTimer(string sceneName, float time)
    {
        Fader.FadeOut(time);
        while (Fader.isFading) yield return null; // wait until fade is complete
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeToSpecificSceneNonFade(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGameNow()
    {
        StartCoroutine(QuitGame());
    }

    public IEnumerator QuitGame()
    {
        Fader.FadeOut();
        while (Fader.isFading) yield return null; // wait until fade is complete
        Application.Quit();
    }

    private void OnGUI()
    {
        //if (GUI.Button(new Rect(10, 10, 100, 20), "Jollof Scene"))
        //{
        //    ChangeToSpecificScene("MechanicsJollofFinal");
        //}

        //if (GUI.Button(new Rect(10, 30, 100, 20), "Fufu Scene"))
        //{
        //    ChangeToSpecificScene("MechanicsFufuFinal");
        //}

        //if (GUI.Button(new Rect(10, 50, 100, 20), "Quit"))
        //{
        //    QuitGame();
        //}
    }
}

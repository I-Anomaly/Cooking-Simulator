using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScript : MonoBehaviour
{
    [Header("UI Reference")]
    public Image loadingBar; // Reference to the green filled image

    [Header("Loading Settings")]
    public float minLoadingTime = 2f; // Minimum time the bar should be visible

    private string sceneToLoad;

    void Start()
    {
        // Get target scene name from PlayerPrefs (set by MenuManager)
        sceneToLoad = PlayerPrefs.GetString("SceneToLoad", "MechanicsJollofFinal");
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            timer += Time.deltaTime;
            float fill = Mathf.Min(timer / minLoadingTime, targetProgress);
            loadingBar.fillAmount = fill;

            if (timer >= minLoadingTime && targetProgress >= 1f)
            {
                yield return new WaitForSeconds(0.2f); // brief pause
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
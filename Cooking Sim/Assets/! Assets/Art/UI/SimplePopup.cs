using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePopup : MonoBehaviour
{
    [Header("Popup panel to toggle")]
    public GameObject panel;

    [Header("Optional: pause gameplay while popup is open")]
    public bool pauseWhileOpen = false;

    float _prevTimeScale = 1f;

    public void Show()
    {
        if (pauseWhileOpen)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        if (panel) panel.SetActive(true);
    }

    public void Hide()
    {
        if (pauseWhileOpen) Time.timeScale = _prevTimeScale;
        if (panel) panel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (pauseWhileOpen) Time.timeScale = _prevTimeScale;
        SceneManager.LoadScene(sceneName);
    }
}

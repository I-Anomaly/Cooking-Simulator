// Screen fader for VR

// Uses static methods, Fader.FadeIn() and Fader.FaderOut()
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader singleton = null;

    public static bool isFading = false;

    private Image fadePanel;

    private void Awake()
    {
        if (singleton)
        {
            Debug.LogWarning("Already have a Fader singleton");
            Destroy(this);  // already have one
            return;
        }
        // we're the first (and only!)
        singleton = this;
        fadePanel = GetComponentInChildren<Image>();
    }

    public IEnumerator Fade(Color fromColor, Color toColor, float duration)
    {
        if (isFading) yield break;  // only allow one fade in progress at a time
        isFading = true;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            fadePanel.color = Color.Lerp(fromColor, toColor, Mathf.Clamp01(t / duration));
            yield return null;
        }
        fadePanel.color = toColor;  // make sure we reach 100%
        isFading = false;
    }

    public static void FadeIn(float fadeTime=3) => singleton?.StartCoroutine(singleton.Fade(Color.black, Color.clear, fadeTime));

    public static void FadeOut(float fadeTime=3) => singleton?.StartCoroutine(singleton.Fade(Color.clear, Color.black, fadeTime));
}

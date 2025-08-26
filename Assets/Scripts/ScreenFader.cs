using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    CanvasGroup cg;
    UnityEngine.UI.Graphic graphic; // Image or other Graphic (optional)

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        graphic = GetComponent<UnityEngine.UI.Graphic>(); // if present
        cg.alpha = 0f;
        cg.blocksRaycasts = false;   // ✅ allow UI interaction by default
        cg.interactable = false;
        if (graphic) graphic.raycastTarget = false; // optional: be extra safe
    }

    public IEnumerator FadeOut(float duration)
    {
        // start blocking during fade
        cg.blocksRaycasts = true;
        if (graphic) graphic.raycastTarget = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }
        cg.alpha = 1f;
        // keep blocking while fully black
        cg.blocksRaycasts = true;
        if (graphic) graphic.raycastTarget = true;
    }

    public IEnumerator FadeIn(float duration)
    {
        // still block while coming down from black
        cg.blocksRaycasts = true;
        if (graphic) graphic.raycastTarget = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / duration);
            yield return null;
        }
        cg.alpha = 0f;

        // ✅ release UI once fully transparent
        cg.blocksRaycasts = false;
        if (graphic) graphic.raycastTarget = false;
    }
}

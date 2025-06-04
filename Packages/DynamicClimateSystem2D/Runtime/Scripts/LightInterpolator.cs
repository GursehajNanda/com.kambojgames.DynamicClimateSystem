using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class LightInterpolator : MonoBehaviour
{
    [Serializable]
    public class LightFrame
    {
        public Light2D ReferenceLight;
        public float NormalizedTime;
    }

    [Tooltip("The light of which the shape will be changed according to the defined frames")]
    public Light2D TargetLight;
    public LightFrame[] LightFrames;


    private void OnEnable()
    {
        ClimateData.Instance.RegisterLightBlender(this);
        SetRatio(0);
        DisableAllLights();
    }

    private void OnDisable()
    {
        ClimateData.Instance.UnregisterLightBlender(this);
    }

    public void SetRatio(float t)
    {
        if (LightFrames.Length == 0)
            return;

        int startFrame = 0;

        while (startFrame < LightFrames.Length - 1 && LightFrames[startFrame + 1].NormalizedTime < t)
        {
            startFrame += 1;
        }

        if (startFrame == LightFrames.Length - 1)
        {
            //the last frame is the "start frame" so there is no frame to interpolate TO, so we just use the last settings
            Interpolate(LightFrames[startFrame].ReferenceLight, LightFrames[startFrame].ReferenceLight, 0.0f);
        }
        else
        {
            float frameLength = LightFrames[startFrame + 1].NormalizedTime - LightFrames[startFrame].NormalizedTime;
            float frameValue = t - LightFrames[startFrame].NormalizedTime;
            float normalizedFrame = frameValue / frameLength;

            Interpolate(LightFrames[startFrame].ReferenceLight, LightFrames[startFrame + 1].ReferenceLight, normalizedFrame);
        }
    }

    void Interpolate(Light2D start, Light2D end, float t)
    {
       float cloudStrength = ClimateData.Instance.CloudsStrength;

        TargetLight.color = Color.Lerp(start.color, end.color, t);
        TargetLight.intensity = Mathf.Lerp(start.intensity, end.intensity, t);

        TargetLight.intensity += cloudStrength * 0.1f;


        var startPath = start.shapePath;
        var endPath = end.shapePath;

        // Safety check
        if (startPath.Length != endPath.Length)
        {
            Debug.LogError($"Shape paths don't match! startPath.Length = {startPath.Length}, endPath.Length = {endPath.Length}");
            return;
        }


        var newPath = new Vector3[startPath.Length];

        for (int i = 0; i < startPath.Length; ++i)
        {
            newPath[i] = Vector3.Lerp(startPath[i], endPath[i], t);
        }

        TargetLight.SetShapePath(newPath);
    }

    public void DisableAllLights()
    {
        foreach (var frame in LightFrames)
        {
            frame.ReferenceLight?.gameObject.SetActive(false);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public float timer, refresh, avgFramerate;

    int fps;

    private void Update()
    {
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;

        if (timer <= 0)
            avgFramerate = (int)(1f / timelapse);
    }

    private void OnGUI()
    {
        GUILayout.Label($"{avgFramerate}");
    }
}

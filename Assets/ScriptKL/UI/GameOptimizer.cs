using System.Collections;
using System.Threading;
using UnityEngine;

public class GameOptimizer : MonoBehaviour
{
    [Header("Frame Setting")]
    int maxRate = 9999;
    [SerializeField]
    private float tagetFrameRate = 60.0f;
    private float currentFrameTime;
    void Awake()
    {
        Application.targetFrameRate = maxRate;

        QualitySettings.vSyncCount = 0;

        currentFrameTime= Time.realtimeSinceStartup;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        QualitySettings.shadows = ShadowQuality.Disable;

        StartCoroutine(WaitForNextFrame());

    }

    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / tagetFrameRate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.01f;

            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 1000));

            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }

    
}
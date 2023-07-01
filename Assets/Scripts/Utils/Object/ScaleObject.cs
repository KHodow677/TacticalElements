using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class ScaleObject : MonoBehaviour {
    [SerializeField] Vector3 normalScale;
    [SerializeField] Vector3 largerScale;

    /// <summary>
    /// Scales the object to the larger scale over time seconds
    /// </summary>
    /// <param name="time">Time in seconds</param>
    public void ScaleUp (float time) {
        ScaleTransform(largerScale, time);
    }

    /// <summary>
    /// Scales the object to the normal scale over time seconds
    /// </summary>
    /// <param name="time">Time in seconds</param>
    public void ScaleDown (float time) {
        ScaleTransform(normalScale, time);
    }

    /// <summary>
    /// Scales the object to the target scale over time seconds
    /// </summary>
    /// <param name="targetScale">Scale to lerp to</param>
    /// <param name="time">Time in seconds</param>
    /// <returns></returns>
    /// <summary>
    /// Scales the object to the target scale over time seconds
    /// </summary>
    /// <param name="targetScale">Scale to lerp to</param>
    /// <param name="time">Time in seconds</param>
    private async void ScaleTransform(Vector3 targetScale, float time)
    {
        // Set up scaling
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        // Lerp localScale to targetScale over time seconds
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        // Ensure final scale is set accurately
        transform.localScale = targetScale;
    }

}

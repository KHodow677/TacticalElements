using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;

public class ScaleObject : MonoBehaviour {
    [SerializeField] Vector3 normalScale;
    [SerializeField] Vector3 largerScale;

    /// <summary>
    /// Scales the object to the larger scale over time seconds
    /// </summary>
    /// <param name="time">Time in seconds</param>
    public void ScaleUp (float time) {
        if (transform.localScale == largerScale) { return; }
        ScaleTransform(largerScale, time);
    }

    /// <summary>
    /// Scales the object to the normal scale over time seconds
    /// </summary>
    /// <param name="time">Time in seconds</param>
    public void ScaleDown (float time) {
        if (transform.localScale == normalScale) { return; }
        ScaleTransform(normalScale, time);
    }

    /// <summary>
    /// Scales the object to the target scale over time seconds
    /// </summary>
    /// <param name="targetScale">Scale to lerp to</param>
    /// <param name="time">Time in seconds</param>
    private void ScaleTransform(Vector3 targetScale, float time)
    {
        // Use DoTween's scale animation to animate the scaling
        transform.DOScale(targetScale, time)
            .SetEase(Ease.InOutSine);
    }

}

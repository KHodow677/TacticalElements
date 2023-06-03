using UnityEngine;

public class ToggleIndicators : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject highlight;
    /// <summary>
    /// Toggles the active state of the target GameObject
    /// </summary>
    /// <param name="active">New state of child1</param>
    public void ToggleTarget(bool active)
    {
        target.SetActive(active);
    }

    /// <summary>
    /// Toggles the active state of the highlight GameObject
    /// </summary>
    /// <param name="active">New state of highlight</param>
    public void ToggleHighlight(bool active)
    {
        highlight.SetActive(active);
    }
}
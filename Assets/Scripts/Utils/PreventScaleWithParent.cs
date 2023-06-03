using UnityEngine;

public class PreventScaleWithParent : MonoBehaviour
{
    [SerializeField] private Transform childTransform;
    private Vector3 initialScale;
    private Vector3 initialParentScale;
    private bool shouldSyncScale;

    private void Start()
    {
        // Store the initial scale of the child and its parent
        initialScale = childTransform.localScale;
        initialParentScale = childTransform.parent.localScale;
        
        // Subscribe to events when the child becomes active or inactive
        childTransform.gameObject.SetActive(false);
        childTransform.gameObject.SetActive(true);
        childTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        shouldSyncScale = true;
        SyncChildScale();
    }

    private void OnDisable()
    {
        shouldSyncScale = false;
    }

    private void Update()
    {
        if (shouldSyncScale)
            SyncChildScale();
    }

    private void SyncChildScale()
    {
        // Calculate the difference in scale between the child and its parent
        Vector3 parentScaleDifference = Vector3.Scale(initialParentScale, childTransform.localScale) - initialScale;

        // Adjust the child's scale to counteract the parent's scale changes
        childTransform.localScale = initialScale + parentScaleDifference;
    }
}

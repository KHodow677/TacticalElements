using UnityEngine;
using System.Collections;

public class TokenMoveController : MonoBehaviour
{
    [SerializeField] public float moveDuration = 0.5f;
    private ScaleObject scaleObject;
    private SpriteRenderer spriteRenderer;
    private bool isMoving;
    private void Start()
    {
        scaleObject = GetComponent<ScaleObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Moves the token to the target position over moveDuration seconds
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="moveDuration"></param>
    /// <returns></returns>
    private IEnumerator MoveToPosition(Vector3 targetPosition, float moveDuration)
    {
        // Skip if already moving
        if (isMoving) { yield break; }

        // Set up movement
        isMoving = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        scaleObject.ScaleUp(0.1f);
        spriteRenderer.sortingOrder = 3;

        // Lerp to position over moveDuration seconds
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Tear down movement
        transform.position = targetPosition;
        isMoving = false;
        scaleObject.ScaleDown(0.1f);
        spriteRenderer.sortingOrder = 2;
    }

    /// <summary>
    /// Moves the token to the target position over moveDuration seconds
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="moveDuration"></param>
    public void StartMoveToPosition(Vector3 targetPosition)
    {
        StartCoroutine(MoveToPosition(targetPosition, moveDuration));
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Rendering.Universal;

public class TokenMoveController : MonoBehaviour
{
    [SerializeField] public float moveDuration = 0.5f;
    [SerializeField] public GameObject captureParticlePrefab;
    [SerializeField] public Color particleColor;
    private ScaleObject scaleObject;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isMoving;
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
    private IEnumerator MoveToPosition(Vector3 targetPosition, float moveDuration, GameObject tokenAtPosition) {
        // Skip if already moving
        if (isMoving) { yield break; }

        // Set up movement
        isMoving = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        scaleObject.ScaleUp(0.1f);
        spriteRenderer.sortingOrder = 3;

        // Lerp to position over moveDuration seconds
        while (elapsedTime < moveDuration) {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        // Detroy tile at position if it exists
        if (tokenAtPosition != null) {
            GameObject particleInstance = Instantiate(captureParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            
            // Set the particle color
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = particleColor;
            
            // Play the particle effect
            particleSystem.Play();
            tokenAtPosition.SetActive(false);
        }

        // Tear down movement
        
        isMoving = false;
        scaleObject.ScaleDown(0.1f);
        spriteRenderer.sortingOrder = 2;
    }

    /// <summary>
    /// Moves the token to the target position over moveDuration seconds
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="moveDuration"></param>
    public void StartMoveToPosition(Vector3 targetPosition, GameObject tokenAtPosition = null)
    {
        StartCoroutine(MoveToPosition(targetPosition, moveDuration, tokenAtPosition));
    }
}

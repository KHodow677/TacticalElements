using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Rendering.Universal;
using DG.Tweening;
using System.Threading.Tasks;

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

    private async void MoveToPosition(Vector3 targetPosition, float moveDuration, GameObject tokenAtPosition)
    {
        // Set up movement
        isMoving = true;
        Vector3 startPosition = transform.position;
        scaleObject.ScaleUp(0.1f);
        await Task.Delay((int) (0.1f * 1000));
        spriteRenderer.sortingOrder = 3;

        // Use DoMove to animate the movement
        transform.DOMove(targetPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() =>
            {
                // Update any necessary visuals during the movement
                // (if needed)
            })
            .OnComplete(() =>
            {
                // Detroy tile at position if it exists
                if (tokenAtPosition != null)
                {
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
            });
    }

    public void StartMoveToPosition(Vector3 targetPosition, GameObject tokenAtPosition = null)
    {
        MoveToPosition(targetPosition, moveDuration, tokenAtPosition);
    }
}

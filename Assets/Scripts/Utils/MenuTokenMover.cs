using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTokenMover : MonoBehaviour
{
    [SerializeField] private float waitingPeriod;
    [SerializeField] private List<TokenMoveController> allTokenControllers;
    private TokenMoveController selectedTokenController;
    private GameObject selectedTile;

    private float elapsedTime = 0f;
    private bool isWaiting = false;

    private void Update()
    {
        if (!isWaiting) {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitingPeriod) {
                elapsedTime = 0f;
                StartCoroutine(MoveTokensWithDelay(waitingPeriod));
                isWaiting = true;
            }
        }
    }

    private IEnumerator MoveTokensWithDelay(float delay) {
        bool noTokensMoving = true;
        foreach (TokenMoveController tokenController in allTokenControllers) {
            if (tokenController.isMoving) {
                noTokensMoving = false;
                break;
            }
        }

        if (noTokensMoving) {
            selectedTokenController = allTokenControllers[Random.Range(0, allTokenControllers.Count)];

            List<GameObject> tiles = GetAvailableTiles(selectedTokenController.gameObject, "Player");
            selectedTile = tiles[Random.Range(0, tiles.Count)];

            selectedTokenController.StartMoveToPosition(selectedTile.transform.position);
        }

        yield return new WaitForSeconds(delay);
        isWaiting = false;
    }


    public GameObject GetTileAtPosition(Vector3 position) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f, LayerMask.GetMask("Tile"));

        if (colliders != null && colliders.Length > 0) {
            return colliders[0].gameObject;
        }
        return null;
    }

    public GameObject GetTokenAtPosition(Vector3 position, string side) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f, LayerMask.GetMask("Token"));

        if (colliders != null && colliders.Length > 0) {
            if (colliders[0].gameObject.tag == side) { return colliders[0].gameObject; }
        }
        return null;
    }

    public List<GameObject> GetAvailableTiles( GameObject token, string side) {
        List<GameObject> availableTiles = new List<GameObject>();
        TokenMoveOptions moveOptions = token.GetComponent<TokenMoveOptions>();
        foreach (Vector3 moveOffset in moveOptions.moveOffsetOptions) {
            Vector3 tilePosition = token.transform.position + moveOffset;
            GameObject tokenObject = GetTokenAtPosition(tilePosition, side);
            if (tokenObject != null) { continue; }
            GameObject tileObject = GetTileAtPosition(tilePosition);
            if (tileObject == null) { continue; }
            availableTiles.Add(tileObject);
        }
        return availableTiles;
    }
}

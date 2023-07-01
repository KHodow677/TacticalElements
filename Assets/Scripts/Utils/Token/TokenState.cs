using UnityEngine;
using System.Collections;


public class TokenState : MonoBehaviour {
    public bool isPlayerOwned = false;
    [SerializeField] private GameObject playerOwnedIndicator;

    /// <summary>
    /// Sets the token to be player owned
    /// </summary>
    public void SetPlayerOwned() {
        isPlayerOwned = true;
        playerOwnedIndicator.SetActive(true);
    }
    /// <summary>
    /// Sets the token to be not player owned
    /// </summary>
    public void UnsetPlayerOwned() {
        isPlayerOwned = false;
        playerOwnedIndicator.SetActive(false);
    }
}
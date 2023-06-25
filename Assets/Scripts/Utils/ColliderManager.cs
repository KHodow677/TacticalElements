using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : MonoBehaviour
{
    private static ColliderManager _instance;
    public static ColliderManager instance { get { return _instance; } }

    [SerializeField] private List<Collider2D> tokenColliders;
    [SerializeField] private List <Collider2D> tileColliders;

    private void Awake()
    {
        // Ensure only one instance of the class exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else { _instance = this; }
    }

    public void SwitchToTilesDeactivated()
    {
        foreach(Collider2D tileCollider in tileColliders)
        {
            tileCollider.enabled = false;
        }
    }

    public void SwitchToTilesActivated()
    {
        foreach (Collider2D tileCollider in tileColliders)
        {
            tileCollider.enabled = true;
        }
    }

    public void SwitchToTokensDeactivated()
    {
        foreach (Collider2D tileCollider in tokenColliders)
        {
            tileCollider.enabled = false;
        }
    }

    public void SwitchToTokensActivated()
    {
        foreach (Collider2D tileCollider in tokenColliders)
        {
            tileCollider.enabled = true;
        }
    }

}

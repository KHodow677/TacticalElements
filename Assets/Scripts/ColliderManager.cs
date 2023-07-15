using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : MonoBehaviour
{
    private static ColliderManager _instance;
    public static ColliderManager instance { get { return _instance; } }

    [SerializeField] private List<Collider2D> tokenColliders;
    [SerializeField] private List<Collider2D> tileColliders;
    [SerializeField] private float scaleTime;

    private List<ScaleObject> tokenScalers;
    private List<ScaleObject> tileScalers;

    [SerializeField] DraftTokenSelection draftTokenSelection;
    [SerializeField] GameTokenSelection gameTokenSelection;

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

    private void Start()
    {
        tokenScalers = new List<ScaleObject>();
        tileScalers = new List<ScaleObject>();

        foreach (Collider2D tokenCollider in tokenColliders)
        {
            tokenScalers.Add(tokenCollider.GetComponent<ScaleObject>());
        }

        foreach (Collider2D tileCollider in tileColliders)
        {
            tileScalers.Add(tileCollider.GetComponent<ScaleObject>());
        }
    }

    public void SwitchToTilesDeactivated()
    {
        for (int i = 0; i < tileColliders.Count; i++)
        {
/*            tileScalers[i].ScaleDown(scaleTime);*/
            tileColliders[i].enabled = false;
        }
    }

    public void SwitchToTilesActivated()
    {
        foreach (Collider2D tileCollider in tileColliders)
        {
            tileCollider.enabled = true;
        }
    }

    public void SwitchToTokensDeactivated(GameObject token = null)
    {
        for (int i = 0; i < tokenColliders.Count; i++)
        {
/*            if (token == null || token != tokenColliders[i].gameObject)
            {
                tokenScalers[i].ScaleDown(scaleTime);
            }*/
            tokenColliders[i].enabled = false;
        }
    }

    public void SwitchToTokensActivated()
    {
        foreach (Collider2D tokenCollider in tokenColliders)
        {
            tokenCollider.enabled = true;
        }
    }
}

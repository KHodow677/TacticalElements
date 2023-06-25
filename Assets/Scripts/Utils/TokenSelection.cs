using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenSelection : MonoBehaviour
{
    private void OnMouseEnter()
    {
        SelectionManager.instance.OnTokenSelect(gameObject);
    }
    private void OnMouseExit() 
    {
        SelectionManager.instance.OnTokenDeselect(gameObject);
    }
    private void OnMouseDown()
    {
        SelectionManager.instance.OnTokenClicked(gameObject);
    }
}

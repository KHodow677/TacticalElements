using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelection : MonoBehaviour
{
    private ToggleIndicators tileIndicator;
    private void Start()
    {
        tileIndicator = GetComponent<ToggleIndicators>();
    }
    private void OnMouseEnter()
    {
        if (!tileIndicator.IsHighlighted()) { return; }
        SelectionManager.instance.OnTileSelect(gameObject);
    }
    private void OnMouseExit()
    {
        if (!tileIndicator.IsHighlighted()) { return; }
        SelectionManager.instance.OnTileDeselect(gameObject);
    }
    private void OnMouseDown()
    {
        if (!tileIndicator.IsHighlighted()) { return; }
        SelectionManager.instance.OnTileClicked(gameObject, true);
    }
}

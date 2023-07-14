using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenMoveOptions : MonoBehaviour
{
    [SerializeField] public List<Vector3> moveOffsetOptions;
    [HideInInspector] public GameObject currentTile;
}

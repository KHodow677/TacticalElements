using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] SceneFader sceneFader;
    void Update()
    {
        // Check if the space key is pressed
        if (Input.GetMouseButtonDown(0)) {
            StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Menu Scene"));

        }
    }
}

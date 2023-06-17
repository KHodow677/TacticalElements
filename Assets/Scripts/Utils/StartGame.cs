using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] SceneFader sceneFader;
    void Update()
    {
        // Check if the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, "Main Game Scene"));
        }
    }
}

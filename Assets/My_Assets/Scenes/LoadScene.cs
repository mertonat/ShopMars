using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName="Base"; // Name of the scene to load
    [SerializeField] private Image progressBar; // Image with fill to act as progress bar


    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

        // Ensure the scene doesn't activate until loading is complete
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Calculate progress percentage
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Update image fill with progress
            if (progressBar != null)
                progressBar.fillAmount = progress;

            // Check if the loading is complete
            if (operation.progress >= 0.9f)
            {
                // Allow the scene to activate once fully loaded
                operation.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }
    }
}

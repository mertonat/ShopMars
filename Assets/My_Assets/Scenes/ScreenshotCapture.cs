using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    [SerializeField] private KeyCode screenshotKey = KeyCode.S; // Choose a key to take the screenshot
    [SerializeField] private int superSize = 2; // Set to 2 or higher for high-resolution images

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Screenshot_{timestamp}.png";
            ScreenCapture.CaptureScreenshot(fileName, superSize);
            Debug.Log("Screenshot taken: " + fileName);
        }
    }
}

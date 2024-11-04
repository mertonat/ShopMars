using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DissolveObject : MonoBehaviour
{  
  [SerializeField] private float noiseStrength = 0.25f;
    [SerializeField] private float objectHeight = 1.0f;
    [SerializeField] private float adjustmentTime = 60.0f; // Set the desired adjustment time in seconds
    [SerializeField] private float minNoiseScale = 48.0f;
    [SerializeField] private float maxNoiseScale = 56.0f;

    private Material material;
    private float height;

    public System.Action OnDissolveComplete;
    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        StartCoroutine(AdjustHeightAndNoiseScaleOverTime());
    }

    private IEnumerator AdjustHeightAndNoiseScaleOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < adjustmentTime)
        {
            var time = (Mathf.PI * 0.5f * (elapsedTime / adjustmentTime)/2);
            height = 1.0f + Mathf.Sin(time) * objectHeight;
            float noiseScale = Mathf.Lerp(48.0f, 56.0f, Mathf.PingPong(elapsedTime, 1.0f));

            SetHeightAndNoiseScale(height, noiseScale);


            elapsedTime += Time.deltaTime;
            //print(elapsedTime);
            yield return null;
        }

        // Ensure that the final height and noise scale are set after the specified adjustment time
        SetHeightAndNoiseScale(1.0f + objectHeight, 56.0f);
        OnDissolveComplete?.Invoke();
    }

    private void SetHeightAndNoiseScale(float height, float noiseScale)
    {
        material.SetFloat("_CutoffHeight", height);
        material.SetFloat("_NoiseStrength", noiseStrength);
        material.SetFloat("_NoiseScale", noiseScale);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseCollider : MonoBehaviour
{
    public CaseManager _CaseManager;
    public RectTransform scaleObject;
    private Vector3 initialSize;
    private Vector3 targetScale;
    private bool enableScale;
    private bool scalingUp;

    [SerializeField]
    private float scalePercentage = 10f;

    [SerializeField]
    private float lerpSpeed = 100.0f; // Adjusted for smoother scaling
    // Start is called before the first frame update
    void Start()
    {
        initialSize = transform.localScale;
        targetScale = initialSize * (1 + scalePercentage / 100f);
        //_CaseManager = transform.parent.parent.GetComponent<CaseManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableScale)
        {
            if (scalingUp)
            {
                ScaleObject(targetScale);
            }
            else
            {
                ScaleObject(initialSize);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            enableScale = true;
            scalingUp = true;
            _CaseManager.isPayment = true;
            isTutorial = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            scalingUp = false;
            _CaseManager.isPayment = false;

        }
    }
    private void ScaleObject(Vector3 target)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, target, lerpSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.localScale, target) < 0.01f)
        {
            transform.localScale = target;
        }
    }
    bool isTutorial;
    public bool IsUnlocked()
    {
        return isTutorial;
    }
}

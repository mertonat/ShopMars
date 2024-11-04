using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseMoneyActor : MonoBehaviour
{

    [SerializeField] private CaseManager _CaseManager;
    [SerializeField] private UIManager _uiManager;
    public List<MoneyActor> moneyActors = new List<MoneyActor>();

    private bool isTrigger = false;
    private float timer;
    float countVal;
    private int profitPerActor;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 moneyRotate;

    [SerializeField] protected float multplierX;
    [SerializeField] protected float multplierY;
    [SerializeField] protected float multplierZ;

    [SerializeField] private int xCount;
    [SerializeField] private int zCount;

    public Vector3 GetPosition()
    {
        float x = 0;
        float y = 0;
        float z = 0;

        Vector3 pos = Vector3.zero;
        for (int i = 0; i < moneyActors.Count; i++)
        {
            z = (moneyActors.Count - 1) % zCount;
            var valueX = (moneyActors.Count - 1) / zCount;
            valueX = Mathf.FloorToInt(valueX);
            x = valueX % xCount;
            var valueY = (moneyActors.Count - 1) / (zCount * xCount);
            y = Mathf.FloorToInt(valueY);


        }

        x *= multplierX;
        y *= multplierY;
        z *= multplierZ;

        pos = new Vector3(x, y, z);

        return pos;
    }

    public void CreateMoney(Vector3 pos)
    {
        var prefabMny = Resources.Load<GameObject>("MoneyActor");
        var mnyScene = Instantiate(prefabMny, pos, Quaternion.identity);
        var mny = mnyScene.GetComponent<MoneyActor>();
        moneyActors.Add(mny);
        var lastPos = GetPosition();
        mny.transform.parent = this.transform;
        mny.Move(lastPos, moneyRotate);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTrigger)
        {

            player = other.transform;
            isTrigger = true;
            int totalProfit = _CaseManager.GetTotalProfit();
            // Ensure moneyActors.Count > 0 to avoid divide by zero
            if (moneyActors.Count > 0)
            {
                profitPerActor = totalProfit / moneyActors.Count;
            }
            else
            {
                Debug.LogWarning("No money actors available for distribution.");
                profitPerActor = 0; // or some default value
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = false;
            countVal = 1;
        }
    }
    void Start()
    {
        // Loop through each child of the current GameObject (this.transform)
        foreach (Transform child in transform)
        {
            // Check if the child has a MoneyActor component
            MoneyActor moneyActor = child.GetComponent<MoneyActor>();

            // If the component exists, add it to the moneyActors list
            if (moneyActor != null)
            {
                moneyActors.Add(moneyActor);
            }
        }

        // Debug log to confirm the number of money actors added
        Debug.Log($"{moneyActors.Count} MoneyActors added to the list.");
    }

    // Update is called once per frame
    void Update()
    {
        if (isTrigger)
        {
            if (moneyActors.Count <= 0)
                return;
            timer -= Time.deltaTime;

            if (timer <= 0 && moneyActors.Count > 0) // Only proceed if there are money actors left
            {
                countVal++;
                var mny = moneyActors[moneyActors.Count - 1];
                moneyActors.Remove(mny);
                mny.MoveAdd(player.transform.position + Vector3.up * 2);
                int profitPerActor = _CaseManager.GetTotalProfit() / (moneyActors.Count + 1); // Adjust count to avoid division by zero
                _CaseManager.ReduceTotalProfit(profitPerActor);

                _uiManager.AddProfit(profitPerActor);

                timer = .3f / countVal;

                if (moneyActors.Count <= 0)
                {
                    isTrigger = false;
                }
            }
        }
    }

}


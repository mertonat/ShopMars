using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseWorkerPayCollider : MonoBehaviour
{
    UIManager _UIManager;
    BaseCharControl _Player;

    CaseManager _CaseManager;
    [SerializeField] private Image fillImage;

    private float initialPrice;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private float startTimer = .15f;
    [SerializeField] private float mnyTimer = .25f;
    [SerializeField] private float price;
    private float timer;
    private float timeMny;


    [SerializeField] private int amountPaid;
    [SerializeField] private int decreaseAmount = 1;

    public GameObject moneyActor;
    public int workerId;

    // Start is called before the first frame update
    void Start()
    {
        timer = startTimer;
        initialPrice = price;
        Debug.Log("Player money: " + GetPlayerTotalMoney());

        GameObject player = GameObject.FindWithTag("UIRoot");
        _CaseManager = transform.parent.GetComponent<CaseManager>();

        _Player = GameObject.FindWithTag("Player").GetComponent<BaseCharControl>();
        if (player != null)
        {
            _UIManager = player.GetComponent<UIManager>();

            if (_UIManager == null)
            {
                Debug.LogError("UIManager component not found on GameObject with tag 'UIroot'.");
            }
        }
        else
        {
            Debug.LogError("GameObject with tag 'UIroot' not found.");
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool isInsideZone = false;
    private Coroutine moneyDeductionCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInsideZone = true;
            if (moneyDeductionCoroutine == null)
            {
                moneyDeductionCoroutine = StartCoroutine(StartMoneyDeduction());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInsideZone = false;
            if (moneyDeductionCoroutine != null)
            {
                StopCoroutine(moneyDeductionCoroutine);
                moneyDeductionCoroutine = null;
            }
        }
    }

    private IEnumerator StartMoneyDeduction()
    {
        while (isInsideZone)
        {
            DecreaseMoney();
            yield return null;
        }
        moneyDeductionCoroutine = null;
    }

    void DecreaseMoney()
    {
        if (price <= 0 || GetPlayerTotalMoney() <= 0)
        {
            return;
        }

        timer -= Time.deltaTime;
        timeMny -= Time.deltaTime;

        if (timer <= 0 && price > 0)
        {
            int playerMoney = GetPlayerTotalMoney();
            float amountToDeduct = Mathf.Min(decreaseAmount, price);
            amountToDeduct = Mathf.Min(playerMoney, amountToDeduct);

            timer = startTimer / decreaseAmount;

            amountPaid += (int)amountToDeduct;
            price -= amountToDeduct;
            playerMoney -= (int)amountToDeduct;

            amountText.text = price.ToString();
            _UIManager.MoneySpendUpdate((int)amountToDeduct);
            fillImage.fillAmount = (float)amountPaid / initialPrice;
            PlayerPrefs.SetInt("money", playerMoney);
            decreaseAmount++;

            if (timeMny <= 0)
            {
                CreateMoney();
                timeMny = mnyTimer;
            }

            if (price <= 0)
            {
                PayOut();
                return;
            }
        }
    }

    private void CreateMoney()
    {
        var prefabMny = moneyActor;
        var mnyScene = Instantiate(prefabMny, _Player.transform.position + Vector3.up * 2, Quaternion.identity);
        var mny = mnyScene.GetComponent<MoneyActor>();

        mny.Move(transform.position);
    }

    private void PayOut()
    {
        Debug.Log("Player paid: " + amountPaid);
        PlayerPrefs.SetInt("caseWorkerPaid" + workerId, 1);  // or increase a count, e.g., PlayerPrefs.GetInt("workerPaid") + 1
        PlayerPrefs.Save(); // Ensure changes are saved
        _CaseManager.ActivateWorker();
        gameObject.SetActive(false);
    }
    private int GetPlayerTotalMoney()
    {
        return PlayerPrefs.GetInt("money");
    }

}

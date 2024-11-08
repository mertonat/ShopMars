using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField]
    private int money;

    [SerializeField]
    private TextMeshProUGUI moneyText;
    [SerializeField] bool a;
    void Awake()
    {
        //PlayerPrefs.DeleteAll();

        a = PlayerPrefs.GetInt("HasRunBefore", 0) == 1;
        if (PlayerPrefs.GetInt("HasRunBefore", 0) == 0) // Check if this is the first run
        {
            SaveMoney(80); // Save money only once
            PlayerPrefs.SetInt("HasRunBefore", 1); // Set the flag to indicate this has run
            PlayerPrefs.Save(); // Save changes to PlayerPrefs
        }
        //SaveMoney(800);
    }
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Player Money: " + LoadMoney());
        moneyText.text = LoadMoney().ToString();
        //MoneySpendUpdate(LoadMoney());
    }

    // Update is called once per frame
    void Update()
    {
        print(PlayerPrefs.GetInt("money") + "Load Money " + LoadMoney());
    }

    private int LoadMoney()
    {
        if (PlayerPrefs.HasKey("money"))
        {
            money = PlayerPrefs.GetInt("money");
        }
        return money;
    }
    public void SaveMoney(int amount)
    {
        PlayerPrefs.SetInt("money", amount);
        PlayerPrefs.Save();
    }

    private string ScoreShow(double Score)
    {
        string result;
        string[] ScoreNames = new string[] { "", "k", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", };
        int i;

        for (i = 0; i < ScoreNames.Length; i++)
            if (Score < 900)
                break;
            else Score = System.Math.Floor(Score / 100f) / 10f;

        if (Score == System.Math.Floor(Score))
            result = Score.ToString() + ScoreNames[i];
        else result = Score.ToString("F1") + ScoreNames[i];
        return result;
    }

    public void MoneySpendUpdate(int spendmoney)
    {
        int currentMoney = LoadMoney();

        currentMoney = currentMoney - spendmoney;
        moneyText.text = ScoreShow(currentMoney).ToString();

        // Optionally, save the updated money back to PlayerPrefs if necessary
        //SaveMoney(currentMoney);
    }
    public void AddProfit(int profit)
    {
        money += profit;
        moneyText.text = ScoreShow(money);
        SaveMoney(money); // Optional: save updated money back to PlayerPrefs
    }
}

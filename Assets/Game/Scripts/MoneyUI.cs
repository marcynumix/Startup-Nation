using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI moneyText;
    private int currentMoneyValue;

    // Start is called before the first frame update
    void Start()
    {
        Player.instance.OnMoneyChanged += OnMoneyChanged;
        UpdateMoney();
    }

    private void OnMoneyChanged()
    {
        UpdateMoney(1);
    }

    private void UpdateMoney(float animDuration = 0)
    {
        StartCoroutine(AnimateMoney(Player.instance.Money, animDuration));
    }

    IEnumerator AnimateMoney(int targetMoney, float duration)
    {
        int startMoney = currentMoneyValue;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            currentMoneyValue = Mathf.RoundToInt(Mathf.Lerp(startMoney, targetMoney, t));

            SetMoneyText(currentMoneyValue);

             
            yield return null;
        }
        currentMoneyValue = targetMoney;
        SetMoneyText(currentMoneyValue);
    }

    private void SetMoneyText(int money)
    {
        // show moneyvalue as USA currency without decimal
        moneyText.text = money.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    }

}

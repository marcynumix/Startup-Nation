using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static Player instance;
    public bool isGameOver = false;
    public int startMoney = 1000000;   

    private int money = 1000000;

    private int moneyBid = 1000;

    public delegate void MoneyChanged();
    public event MoneyChanged OnMoneyChanged;

    public delegate void MoneyBidChanged();
    public event MoneyBidChanged OnMoneyBidChanged;
    

    public int Money
    {
        get
        {
            return money;
        }
        set
        {
            money = value;
            // fire event
            if (OnMoneyChanged != null)
            {
                OnMoneyChanged();
            }
        }
    }

    public int MoneyBid
    {
        get
        {
            return moneyBid;
        }
        set
        {
            moneyBid = value;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Money = startMoney;
        MoneyBid = 0;
    }


}

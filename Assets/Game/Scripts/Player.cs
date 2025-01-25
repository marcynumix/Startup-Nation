using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public int startMoney = 1000000;   

    private int money = 1000000;

    public delegate void MoneyChanged();
    public event MoneyChanged OnMoneyChanged;
    

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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Money -= 100000;
        }
    }
}

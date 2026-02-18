using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class Bank : MonoBehaviour
{
    public static Bank Instance;
    Player player;
    [SerializeField] Wallet wallet;

    private int nextPayment = 100;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public int GetNextPayment()
    {
        return nextPayment;
    }

    public void CheckPayment(int weeks)
    {
        float growthRate = 1.5f;
        nextPayment = (int)(100 * Math.Pow(growthRate, weeks));

        if (wallet.GetAmount() < nextPayment)
        {
            GameManager.gm.GameLose((int)wallet.GetAmount());
        }
        else
        {
            wallet.Subtract(nextPayment);
        }
    }
}
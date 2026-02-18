using TMPro;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] TMP_Text creditsText;

    [SerializeField] int startingAmount;
    int amount = 0;

    void Update()
    {
        creditsText.text = $"Credits: {amount}; Next Payment: {Bank.Instance.GetNextPayment()}";
    }

    public void Add(int amount)
    {
        this.amount += amount;
    }

    public void Subtract(int amount)
    {
        this.amount -= amount;
    }

    public int GetAmount()
    {
        return amount;
    }
}
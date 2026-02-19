using TMPro;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] TMP_Text creditsText;

    [SerializeField] string startingAmount = "0";
    decimal amount = 0m;

    void Awake()
    {
        decimal.TryParse(startingAmount, out amount);
    }

    void Update()
    {
        if (creditsText != null)
        {
            int nextPayment = 0;
            if (Bank.Instance != null)
            {
                try
                {
                    nextPayment = Bank.Instance.GetNextPayment();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Wallet] Failed to get next payment from Bank: {ex.Message}");
                }
            }
            creditsText.text = $"{amount}";
        }
    }

    // Allow other scripts to assign which TMP_Text this Wallet should update.
    public void SetTextTarget(TMP_Text textTarget)
    {
        creditsText = textTarget;
    }

    public void Add(int amount)
    {
        Add((decimal)amount);
    }

    public void Add(decimal amount)
    {
        this.amount += amount;
    }

    public void Subtract(int amount)
    {
        Subtract((decimal)amount);
    }

    public void Subtract(decimal amount)
    {
        this.amount -= amount;
    }

    public decimal GetAmount()
    {
        return amount;
    }
}
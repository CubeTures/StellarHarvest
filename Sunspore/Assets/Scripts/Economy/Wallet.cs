using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance
    {
        get; private set;
    }

    [SerializeField] TMP_Text creditsText;

    [SerializeField] int startingAmount = 0;
    int amount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        amount = startingAmount;
    }

    void Update()
    {
        if (creditsText != null)
        {
            string txt = amount.ToString("N0");
            bool rebuild = txt != creditsText.text;
            creditsText.text = txt;

            if (rebuild)
            {
                Rebuild();
            }
        }
    }

    // Allow other scripts to assign which TMP_Text this Wallet should update.
    public void SetTextTarget(TMP_Text textTarget)
    {
        creditsText = textTarget;
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

    void Rebuild()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsText.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsText.transform.parent.GetComponent<RectTransform>());
    }
}
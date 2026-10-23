using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CafeCounter : UdonSharpBehaviour
{
    public CoinWallet wallet;
    public string[] itemNames = new string[]
    {
        "Coffee", "Tea", "Milk Tea with Pearls", "Milk Tea, No Boba", "Iced Tea",
        "Mango Smoothie", "Strawberry Smoothie", "Banana Smoothie", "Iced Matcha", "Iced Strawberry Matcha",
        "Energy Drink", "Muffin", "Water", "Cookies", "Croissant"
    };
    public int[] prices = new int[] { 5, 5, 10, 8, 8, 8, 8, 8, 8, 10, 4, 3, 1, 5, 5 };
    public VRCObjectPool[] pools;
    public Transform spawnPoint;
    public TextMeshProUGUI[] menuTexts;
    public TextMeshProUGUI statusText;
    public AudioSource buySound;

    void Start()
    {
        for (int i = 0; i < menuTexts.Length; i++)
        {
            if (i < itemNames.Length) menuTexts[i].text = itemNames[i] + "\n" + prices[i] + " coins";
        }
        statusText.text = "What would you like?";
    }

    public void Buy0() { Buy(0); }
    public void Buy1() { Buy(1); }
    public void Buy2() { Buy(2); }
    public void Buy3() { Buy(3); }
    public void Buy4() { Buy(4); }
    public void Buy5() { Buy(5); }
    public void Buy6() { Buy(6); }
    public void Buy7() { Buy(7); }
    public void Buy8() { Buy(8); }
    public void Buy9() { Buy(9); }
    public void Buy10() { Buy(10); }
    public void Buy11() { Buy(11); }
    public void Buy12() { Buy(12); }
    public void Buy13() { Buy(13); }
    public void Buy14() { Buy(14); }

    void Buy(int i)
    {
        if (!wallet.IsReady())
        {
            statusText.text = "Your coins are still loading, try again in a moment.";
            return;
        }

        if (wallet.GetCoins() < prices[i])
        {
            statusText.text = "You need " + prices[i] + " coins for that. Try the quiz booths!";
            return;
        }

        VRCPlayerApi me = Networking.LocalPlayer;
        Networking.SetOwner(me, pools[i].gameObject);
        GameObject cup = pools[i].TryToSpawn();

        if (cup == null)
        {
            statusText.text = "Sorry, that one has sold out for now. Please wait a minute.";
            return;
        }

        wallet.SpendCoins(prices[i]);
        Networking.SetOwner(me, cup);
        cup.transform.position = spawnPoint.position;
        cup.transform.rotation = spawnPoint.rotation;

        statusText.text = "Here's your " + itemNames[i] + ". Enjoy!";
        if (buySound != null) buySound.Play();
    }
}

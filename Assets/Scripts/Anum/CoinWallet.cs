using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Persistence;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CoinWallet : UdonSharpBehaviour
{
    public TextMeshProUGUI[] coinTexts;
    public int startCoins = 0;

    int coins;
    bool ready;

    void Start()
    {
        UpdateTexts();
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        int saved;
        if (PlayerData.TryGetInt(player, "coins", out saved))
        {
            coins = saved;
        }
        else
        {
            coins = startCoins;
            PlayerData.SetInt("coins", coins);
        }

        ready = true;
        UpdateTexts();
    }

    public bool IsReady()
    {
        return ready;
    }

    public int GetCoins()
    {
        return coins;
    }

    public void AddCoins(int amount)
    {
        if (!ready) return;
        coins += amount;
        PlayerData.SetInt("coins", coins);
        UpdateTexts();
    }

    public bool SpendCoins(int amount)
    {
        if (!ready) return false;
        if (coins < amount) return false;
        coins -= amount;
        PlayerData.SetInt("coins", coins);
        UpdateTexts();
        return true;
    }

    public int GetItem(int item)
    {
        int value;
        if (PlayerData.TryGetInt(Networking.LocalPlayer, "item" + item, out value))
        {
            return value;
        }
        return 0;
    }

    public bool HasItem(int item)
    {
        return GetItem(item) > 0;
    }

    public void GiveItem(int item, int place)
    {
        PlayerData.SetInt("item" + item, 10 + place);
    }

    public void GiveWearable(int item)
    {
        PlayerData.SetInt("item" + item, 1);
    }

    void UpdateTexts()
    {
        string text = "Coins: " + coins;
        if (!ready) text = "Coins: loading...";

        for (int i = 0; i < coinTexts.Length; i++)
        {
            if (coinTexts[i] != null) coinTexts[i].text = text;
        }
    }
}

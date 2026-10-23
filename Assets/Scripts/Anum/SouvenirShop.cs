using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SouvenirShop : UdonSharpBehaviour
{
    public CoinWallet wallet;
    public ShopCatalog catalog;
    public int hereId = 0;
    public string shopName = "Earth Souvenirs";
    public int firstId = 0;
    public int lastId = 34;

    public VRCObjectPool[] pools;
    public Transform spawnPoint;
    public ShopSlot[] slots;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI statusText;
    public AudioSource buySound;

    int selected;
    ShopSlot openSlot;      // the slot whose info card is open
    Transform spawnOverride; // where a bought or tried-on item appears instead of the till (in front of its shelf)

    void Start()
    {
        if (titleText != null) titleText.text = shopName;
        selected = firstId;
        ShowSelected();
        statusText.text = "Touch anything on the shelves to see it here.";
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) slots[i].UpdateLook();
        }
        SendCustomEventDelayedSeconds("Refresh", 3f);
    }

    // A display item was pressed: open its info card right there (and show it on the till too)
    public void ShowCard(ShopSlot slot)
    {
        if (openSlot != null && openSlot != slot) openSlot.HideCard();
        openSlot = slot;
        Select(slot.itemId);
        bool canTry = catalog.kinds[selected] == 2 && !wallet.HasItem(selected) && catalog.IsOnSale(selected, wallet);
        slot.ShowCard(catalog.names[selected], catalog.info[selected], priceText.text, catalog.kinds[selected], canTry);
    }

    // Buy from an item's card: what you buy appears in front of that item
    public void BuyFrom(ShopSlot slot)
    {
        Select(slot.itemId);
        spawnOverride = slot.spawnHere;
        Buy();
        spawnOverride = null;
        slot.SetStatus(statusText.text, priceText.text);
    }

    // Try a wearable on for a minute before buying: a copy appears in front of the item and goes back to
    // the shelf by itself after a minute
    public void TryOnFrom(ShopSlot slot)
    {
        int id = slot.itemId;
        Select(id);
        if (catalog.kinds[id] != 2) return;
        spawnOverride = slot.spawnHere;
        GameObject item = SpawnObject(id);
        spawnOverride = null;
        if (item == null) { slot.SetStatus(statusText.text, priceText.text); return; }
        ShopWearable w = item.GetComponent<ShopWearable>();
        if (w != null) w.StartTrial();
        slot.SetStatus("Here's one to try on for a minute: hold it, press use to wear it, and check the mirror.", priceText.text);
    }

    public void Select(int id)
    {
        selected = id;
        ShowSelected();
        statusText.text = "";
    }

    public void NextItem()
    {
        selected++;
        if (selected > lastId) selected = firstId;
        ShowSelected();
    }

    public void PrevItem()
    {
        selected--;
        if (selected < firstId) selected = lastId;
        ShowSelected();
    }

    void ShowSelected()
    {
        nameText.text = catalog.names[selected];
        infoText.text = catalog.info[selected];

        if (!catalog.IsOnSale(selected, wallet))
        {
            priceText.text = "Not on sale right now";
        }
        else if (catalog.kinds[selected] == 2 && wallet.HasItem(selected))
        {
            priceText.text = "You own this. Take one for free";
        }
        else
        {
            priceText.text = catalog.prices[selected] + " coins";
        }
    }

    public void Buy()
    {
        int id = selected;
        string itemName = catalog.names[id];
        int kind = catalog.kinds[id];

        if (!wallet.IsReady())
        {
            statusText.text = "Your coins are still loading, try again in a moment.";
            return;
        }

        if (!catalog.IsOnSale(id, wallet))
        {
            statusText.text = "Sorry, this one isn't on sale right now. " + catalog.info[id];
            return;
        }

        if (kind == 0 && wallet.HasItem(id))
        {
            statusText.text = "You already have the " + itemName + ".";
            return;
        }

        if (kind == 2 && wallet.HasItem(id))
        {
            if (Spawn(id)) statusText.text = "Here's your " + itemName + " again. Hold it and press use to wear it.";
            return;
        }

        if (wallet.GetCoins() < catalog.prices[id])
        {
            statusText.text = "You need " + catalog.prices[id] + " coins for that. Try the quiz booths on Earth!";
            return;
        }

        if (kind == 0)
        {
            wallet.SpendCoins(catalog.prices[id]);
            wallet.GiveItem(id, hereId);
            statusText.text = "You got the " + itemName + "! It's waiting at the baggage claim. You can send it anywhere from a bag drop.";
        }
        else
        {
            if (!Spawn(id)) return;
            wallet.SpendCoins(catalog.prices[id]);
            if (kind == 2)
            {
                wallet.GiveWearable(id);
                statusText.text = "Here's your " + itemName + "! Hold it and press use to wear it. Check the mirror!";
            }
            else
            {
                statusText.text = "Here's your " + itemName + ". Enjoy!";
            }
        }

        if (buySound != null) buySound.Play();
        ShowSelected();
        Refresh();
    }

    bool Spawn(int id)
    {
        return SpawnObject(id) != null;
    }

    GameObject SpawnObject(int id)
    {
        int index = id - firstId;
        if (index < 0 || index >= pools.Length || pools[index] == null)
        {
            statusText.text = "This item isn't set up yet.";
            return null;
        }

        VRCPlayerApi me = Networking.LocalPlayer;
        Networking.SetOwner(me, pools[index].gameObject);
        GameObject item = pools[index].TryToSpawn();

        if (item == null)
        {
            statusText.text = "Sorry, that one has sold out for now. Please wait a minute.";
            return null;
        }

        Networking.SetOwner(me, item);
        Transform at = spawnOverride != null ? spawnOverride : spawnPoint;
        item.transform.position = at.position;
        item.transform.rotation = at.rotation;
        return item;
    }
}

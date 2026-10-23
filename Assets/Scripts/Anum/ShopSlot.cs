using UdonSharp;
using UnityEngine;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ShopSlot : UdonSharpBehaviour
{
    public SouvenirShop shop;
    public int itemId = 0;
    public TextMeshProUGUI tagText;
    public GameObject ownedMark;
    public GameObject lockCover;

    // The info card that opens right above the item when it is pressed: name, story, price, status, and
    // Buy / Try on / Close buttons. Things bought or tried on here appear at spawnHere, in front of the item
    public GameObject infoCard;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardInfo;
    public TextMeshProUGUI cardPrice;
    public TextMeshProUGUI cardStatus;
    public GameObject tryButton;
    public Transform spawnHere;

    void Start()
    {
        if (infoCard != null) infoCard.SetActive(false);
        UpdateLook();
    }

    public override void Interact()
    {
        shop.ShowCard(this);
    }

    public void ShowCard(string itemName, string info, string price, int kind, bool canTry)
    {
        if (infoCard == null) return;
        cardName.text = itemName;
        cardInfo.text = info;
        cardPrice.text = price;
        if (kind == 0) cardStatus.text = "Yours to keep: it waits for you at the baggage claim.";
        else if (kind == 1) cardStatus.text = "Press Buy and it appears right here to carry around.";
        else cardStatus.text = "Press Buy, or try one on in front of the mirror first.";
        if (tryButton != null) tryButton.SetActive(canTry);
        infoCard.SetActive(true);
    }

    public void SetStatus(string status, string price)
    {
        if (cardStatus != null) cardStatus.text = status;
        if (cardPrice != null) cardPrice.text = price;
    }

    public void HideCard()
    {
        if (infoCard != null) infoCard.SetActive(false);
    }

    // Card buttons
    public void BuyHere() { shop.BuyFrom(this); }
    public void TryOnHere() { shop.TryOnFrom(this); }
    public void CloseCard() { HideCard(); }

    public void UpdateLook()
    {
        ShopCatalog catalog = shop.catalog;
        CoinWallet wallet = shop.wallet;

        bool onSale = catalog.IsOnSale(itemId, wallet);
        bool owned = wallet.IsReady() && wallet.HasItem(itemId);

        if (tagText != null)
        {
            if (!onSale) tagText.text = catalog.names[itemId] + "\nNot on sale now";
            else tagText.text = catalog.names[itemId] + "\n" + catalog.prices[itemId] + " coins";
        }

        if (ownedMark != null) ownedMark.SetActive(owned);
        if (lockCover != null) lockCover.SetActive(!onSale);
    }
}

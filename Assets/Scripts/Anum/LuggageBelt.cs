using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Persistence;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LuggageBelt : UdonSharpBehaviour
{
    public ShopCatalog catalog;
    public int hereId = 0;

    public TextMeshProUGUI itemText;
    public TextMeshProUGUI placeText;
    public TextMeshProUGUI statusText;

    public Transform bag;
    public Transform beltStart;
    public Transform beltEnd;
    public float beltTime = 3f;
    public AudioSource beltSound;

    int item = -1;
    int place = -1;
    bool moving;
    float t;

    void Start()
    {
        if (bag != null) bag.gameObject.SetActive(false);
        if (hereId == 0) place = 1;
        else place = 0;
        ShowPlace();
        Refresh();
    }

    bool IsHere(int id)
    {
        if (catalog.kinds[id] != 0) return false;

        int value;
        if (PlayerData.TryGetInt(Networking.LocalPlayer, "item" + id, out value))
        {
            return value == 10 + hereId;
        }
        return false;
    }

    public void Refresh()
    {
        if (item == -1 || !IsHere(item))
        {
            item = -1;
            for (int i = 0; i < catalog.GetCount(); i++)
            {
                if (IsHere(i))
                {
                    item = i;
                    break;
                }
            }
        }
        ShowItem();
        SendCustomEventDelayedSeconds("Refresh", 3f);
    }

    public void NextItem()
    {
        int check = item;
        for (int i = 0; i < catalog.GetCount(); i++)
        {
            check++;
            if (check >= catalog.GetCount()) check = 0;
            if (IsHere(check))
            {
                item = check;
                break;
            }
        }
        ShowItem();
    }

    public void PrevItem()
    {
        int check = item;
        for (int i = 0; i < catalog.GetCount(); i++)
        {
            check--;
            if (check < 0) check = catalog.GetCount() - 1;
            if (IsHere(check))
            {
                item = check;
                break;
            }
        }
        ShowItem();
    }

    public void PickEarth() { PickPlace(0); }
    public void PickAstronomy() { PickPlace(1); }
    public void PickSciTech() { PickPlace(2); }

    void PickPlace(int p)
    {
        if (p == hereId)
        {
            statusText.text = "You're already on " + catalog.placeNames[p] + ". Pick somewhere else.";
            return;
        }
        place = p;
        ShowPlace();
    }

    void ShowPlace()
    {
        placeText.text = "Send to: " + catalog.placeNames[place];
    }

    void ShowItem()
    {
        if (moving) return;

        if (item == -1)
        {
            itemText.text = "Nothing here";
            statusText.text = "Souvenirs you buy here, or send here, can be sent on from this bag drop.";
            return;
        }

        itemText.text = catalog.names[item];
        statusText.text = "This is waiting here on " + catalog.placeNames[hereId] + ". Pick where to send it.";
    }

    public void Send()
    {
        if (moving) return;
        if (item == -1 || !IsHere(item))
        {
            statusText.text = "You don't have anything here to send.";
            return;
        }

        PlayerData.SetInt("item" + item, 10 + place);
        statusText.text = "Your " + catalog.names[item] + " is on its way. It will be waiting for you on " + catalog.placeNames[place] + ".";
        item = -1;

        if (bag != null)
        {
            bag.position = beltStart.position;
            bag.gameObject.SetActive(true);
            t = 0f;
            moving = true;
        }
        if (beltSound != null) beltSound.Play();
    }

    void Update()
    {
        if (!moving) return;

        t += Time.deltaTime / beltTime;
        bag.position = Vector3.Lerp(beltStart.position, beltEnd.position, t);

        if (t >= 1f)
        {
            moving = false;
            bag.gameObject.SetActive(false);
        }
    }
}

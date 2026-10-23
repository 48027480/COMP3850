using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.SDK3.Persistence;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ArrivalLuggage : UdonSharpBehaviour
{
    public ShopCatalog catalog;
    public int hereId = 1;
    public GameObject[] parcels;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI listText;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        VRCPlayerApi me = Networking.LocalPlayer;
        int count = 0;
        string list = "";

        for (int i = 0; i < catalog.GetCount(); i++)
        {
            if (catalog.kinds[i] != 0) continue;

            int value;
            if (PlayerData.TryGetInt(me, "item" + i, out value))
            {
                if (value == 10 + hereId)
                {
                    count++;
                    list = list + catalog.names[i] + "\n";
                }
            }
        }

        for (int i = 0; i < parcels.Length; i++)
        {
            if (parcels[i] != null) parcels[i].SetActive(i < count);
        }

        if (count == 0) countText.text = "Your luggage: nothing here yet.";
        else if (count == 1) countText.text = "Your luggage: 1 souvenir is waiting for you!";
        else countText.text = "Your luggage: " + count + " souvenirs are waiting for you!";

        if (listText != null) listText.text = list;

        SendCustomEventDelayedSeconds("Refresh", 3f);
    }
}

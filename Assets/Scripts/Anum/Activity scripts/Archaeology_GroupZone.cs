using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Archaeology_GroupZone : UdonSharpBehaviour
{
    public UdonSharpBehaviour console;
    public int groupSize = 3;

    int[] playerIds = new int[100];
    int count = 0;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null) return;
        int id = player.playerId;

        for (int i = 0; i < count; i++)
        {
            if (playerIds[i] == id) return;
        }

        if (count < playerIds.Length)
        {
            playerIds[count] = id;
            count = count + 1;
        }
        Refresh();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null) return;
        RemovePlayer(player.playerId);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null) return;
        RemovePlayer(player.playerId);
    }

    void RemovePlayer(int id)
    {
        for (int i = 0; i < count; i++)
        {
            if (playerIds[i] == id)
            {
                playerIds[i] = playerIds[count - 1];
                count = count - 1;
                Refresh();
                return;
            }
        }
    }

    void Refresh()
    {
        if (console != null) console.SendCustomEvent("RefreshView");
    }

    public int GetCount()
    {
        return count;
    }

    public bool HasGroup()
    {
        return count >= groupSize;
    }

    public string WaitText()
    {
        return "This activity needs a group of " + groupSize + ". People here: " + count + "/" + groupSize + ".";
    }
}

using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ZeroGFloat : UdonSharpBehaviour
{
    public RocketSystem rocket;
    public bool arrival;
    public Transform[] items;
    public Vector3[] restPos;
    public Quaternion[] restRot;
    public Vector3[] floatPos;
    public Vector3[] landPos;
    public Quaternion[] landRot;
    public float liftTime = 5f;
    public float fallLead = 4f;
    public float fallTime = 0.9f;
    public float drift = 0.18f;

    float blend;
    float[] phase;
    Vector3[] spin;
    Vector3[] fallFrom;
    bool wasFlying;
    bool fallen;
    float launchedAt;
    float fallStart;

    void Start()
    {
        int n = items == null ? 0 : items.Length;
        phase = new float[n];
        spin = new Vector3[n];
        fallFrom = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            phase[i] = i * 1.7f;
            spin[i] = new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), Random.Range(-30f, 30f));
        }
        if (arrival) PlaceAll(landPos, landRot);
        else PlaceAll(restPos, restRot);
        if (rocket != null && rocket.flying)
        {
            wasFlying = true;
            launchedAt = Time.time;
            blend = 1f;
        }
    }

    void Update()
    {
        if (arrival || items == null || phase == null) return;
        bool flying = rocket != null && rocket.flying;

        if (flying && !wasFlying)
        {
            launchedAt = Time.time;
            fallen = false;
            blend = 0f;
        }
        if (!flying && wasFlying)
        {
            blend = 0f;
            fallen = false;
            PlaceAll(restPos, restRot);
        }
        wasFlying = flying;
        if (!flying) return;

        float landingAt = launchedAt + (rocket.flightDurationSeconds - fallLead);
        if (Time.time >= landingAt)
        {
            if (!fallen)
            {
                fallen = true;
                fallStart = Time.time;
                for (int i = 0; i < items.Length; i++) if (items[i] != null) fallFrom[i] = items[i].localPosition;
            }
            float k = Mathf.Clamp01((Time.time - fallStart) / fallTime);
            float e = k * k;
            for (int i = 0; i < items.Length; i++)
            {
                Transform it = items[i];
                if (it == null || i >= landPos.Length || i >= landRot.Length) continue;
                it.localPosition = Vector3.Lerp(fallFrom[i], landPos[i], e);
                it.localRotation = Quaternion.Slerp(it.localRotation, landRot[i], Mathf.Clamp01(Time.deltaTime * 8f));
            }
            return;
        }

        blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime / liftTime);
        float s = blend * blend * (3f - 2f * blend);
        float t = Time.time;
        for (int i = 0; i < items.Length; i++)
        {
            Transform it = items[i];
            if (it == null || i >= restPos.Length || i >= floatPos.Length) continue;
            float p = phase[i];
            Vector3 wander = new Vector3(Mathf.Sin(t * 0.31f + p), Mathf.Sin(t * 0.23f + p * 1.3f), Mathf.Cos(t * 0.27f + p)) * drift;
            it.localPosition = Vector3.Lerp(restPos[i], floatPos[i] + wander, s);
            it.localRotation = it.localRotation * Quaternion.Euler(spin[i] * (Time.deltaTime * s));
        }
    }

    void PlaceAll(Vector3[] pos, Quaternion[] rot)
    {
        if (items == null || pos == null || rot == null) return;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || i >= pos.Length || i >= rot.Length) continue;
            items[i].localPosition = pos[i];
            items[i].localRotation = rot[i];
        }
    }
}

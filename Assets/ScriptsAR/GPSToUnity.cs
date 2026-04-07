using UnityEngine;

public static class GPSToUnity
{
    const float scale = 100f; // adjust depending on your map

    public static Vector2 GPSDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float latDist = (lat2 - lat1) * 111_000f;
        float lonDist = (lon2 - lon1) * 111_000f * Mathf.Cos(lat1 * Mathf.Deg2Rad);

        return new Vector2(lonDist, latDist) / scale;
    }
}

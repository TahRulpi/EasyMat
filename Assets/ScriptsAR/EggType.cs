using UnityEngine;

/*public enum EggType
{
    Red,
    Green,
    Purple,
    Golden
}

[System.Serializable]
public class EggData
{
    public EggType eggType;
    public double latitude;
    public double longitude;

    public EggData() { }

    public EggData(EggType type, double lat, double lon)
    {
        eggType = type;
        latitude = lat;
        longitude = lon;
    }
}
*/

[System.Serializable]
public class EggData
{
    public EggType eggType;
    public double latitude;
    public double longitude;
    public bool isCollected = false; // NEW: track if collected in AR
}

public enum EggType
{
    Red,
    Green,
    Purple,
    Golden
}


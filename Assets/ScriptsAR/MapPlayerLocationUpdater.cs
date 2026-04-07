using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Location;

public class MapPlayerLocationUpdater : MonoBehaviour
{
    void Update()
    {
        var playerLoc = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation;
        if (playerLoc.IsLocationUpdated)
        {
            // Store latest player location for AR scene
            PlayerLocationHolder.LastKnownLocation = playerLoc.LatitudeLongitude;
        }
    }
}

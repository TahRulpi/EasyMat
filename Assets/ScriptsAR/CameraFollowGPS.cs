using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class CameraFollowGPS : MonoBehaviour
{
    public AbstractMap map;         // Assign your Mapbox map
    public float cameraHeight = 20; // Height above map

    private ILocationProvider locationProvider;

    void Start()
    {
        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        Input.location.Start(); // start GPS service
    }

    void Update()
    {
        if (map == null) return;

        var loc = locationProvider.CurrentLocation;
        if (!loc.IsLocationServiceEnabled || loc.LatitudeLongitude == Vector2d.zero) return;

        // Convert GPS to Unity world position
        Vector3 worldPos = map.GeoToWorldPosition(loc.LatitudeLongitude, true);
        worldPos.y = cameraHeight; // keep camera above map
        transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 3f); // smooth follow
    }
}

using UnityEngine;

public class CamController : MonoBehaviour
{
    [Range(0, 1)]
    public float smoothTime;

    public Transform playerTransform;
    
    private int worldSize;
    public void Spawn(Vector3 pos, int worldSize)
    {
        GetComponent<Transform>().position = pos;
        this.worldSize = worldSize;
    }
    
    private void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;

        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);

        pos.x = Mathf.Clamp(pos.x,
            0 + (GetComponent<Camera>().orthographicSize * 2f),
            worldSize - (GetComponent<Camera>().orthographicSize * 2f));

        GetComponent<Transform>().position = pos;
    }
}

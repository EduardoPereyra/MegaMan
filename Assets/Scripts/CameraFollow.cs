using UnityEngine;

public class CameraFollow: MonoBehaviour
{

    [SerializeField] private Transform player;
    [SerializeField] private float timeOffset;
    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private Vector3 boundsMin;
    [SerializeField] private Vector3 boundsMax;


    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = player.position;
            targetPos.x += offsetPos.x;
            targetPos.y += offsetPos.y;
            targetPos.z = transform.position.z;

            targetPos.x = Mathf.Clamp(targetPos.x, boundsMin.x, boundsMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, boundsMin.y, boundsMax.y);

            float t = 1f - Mathf.Pow(1f - timeOffset, Time.deltaTime * 30);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
    }
}

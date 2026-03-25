using UnityEngine;

public class PlayerExplosionController: MonoBehaviour
{
    public float explosionSpeed = 0.75f;
    GameObject[] explosions = new GameObject[12];
    Vector3[] explosionDirections =
    {
        new Vector3(-1f, 0, 0),
        new Vector3(1f, 0, 0),
        new Vector3(0, -1f, 0),
        new Vector3(0, 1f, 0),
        new Vector3(-0.75f, -0.75f, 0),
        new Vector3(-0.75f, 0.75f, 0),
        new Vector3(0.75f, -0.75f, 0),
        new Vector3(0.75f, 0.75f, 0),
        new Vector3(-0.5f, 0, 0),
        new Vector3(0.5f, 0, 0),
        new Vector3(0, -0.5f, 0),
        new Vector3(0, 0.5f, 0),
    };


    void Start()
    {
        for (int i = 0; i < explosions.Length; i++)
        {
            string explosionName = "Explosion" + (i + 1).ToString();
            explosions[i] = transform.Find(explosionName).gameObject;
        }
    }

    void Update()
    {
        for (int i = 0; i < explosions.Length; i++)
        {
            Vector3 position = explosions[i].transform.position;
            position.x += explosionDirections[i].x * explosionSpeed * Time.deltaTime;
            position.y += explosionDirections[i].y * explosionSpeed * Time.deltaTime;
            position.z += explosionDirections[i].z * explosionSpeed * Time.deltaTime;
            explosions[i].transform.position = position;
        }
    }

}

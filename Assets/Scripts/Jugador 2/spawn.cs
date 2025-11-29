using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab del obstáculo (arrastrar el Cone aquí)")]
    public GameObject conePrefab;

    [Header("Posiciones de carril en X")]
    public float leftLaneX = -5f;
    public float centerLaneX = 0f;
    public float rightLaneX = 5f;

    [Header("Posición sobre la pista")]
    public float spawnZ = 2;   // Distancia delante del jugador
    public float spawnY = 0f;    // Altura del piso (ajústala si el cono flota o se hunde)

    [Header("Escala del obstáculo")]
    public Vector3 obstacleScale = new Vector3(1.86f, 1.73f, 1.63f);

    [Header("Teclas para spawnear")]
    public KeyCode spawnLeftKey = KeyCode.Alpha1;
    public KeyCode spawnCenterKey = KeyCode.Alpha2;
    public KeyCode spawnRightKey = KeyCode.Alpha3;

    void Update()
    {
        if (Input.GetKeyDown(spawnLeftKey))
            SpawnCone(leftLaneX);

        if (Input.GetKeyDown(spawnCenterKey))
            SpawnCone(centerLaneX);

        if (Input.GetKeyDown(spawnRightKey))
            SpawnCone(rightLaneX);
    }

    void SpawnCone(float laneX)
    {
        if (conePrefab == null)
        {
            Debug.LogWarning("⚠️ No se asignó el prefab del cono en el inspector.");
            return;
        }

        Vector3 spawnPos = new Vector3(laneX, spawnY, spawnZ);
        GameObject newCone = Instantiate(conePrefab, spawnPos, Quaternion.identity);

        // Aplica la escala correcta
        newCone.transform.localScale = obstacleScale;

        Debug.Log($"🟠 Cono spawneado en X:{laneX}, Z:{spawnZ}, con escala {obstacleScale}");
    }
}

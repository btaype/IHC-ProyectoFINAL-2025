using UnityEngine;

public class CamaraSaboteadorFollowZ : MonoBehaviour
{
    public Transform player; 
    private float distanciaZ; 

    void Start()
    {
       
        distanciaZ = transform.position.z - player.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Mantenemos la misma distancia en Z
        Vector3 pos = transform.position;
        pos.z = player.position.z + distanciaZ;
        transform.position = pos;
    }
}

using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;

[System.Serializable]
public class EstadoPosicion
{
    public int jugador;
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public int obstaculo;

}

public class EnvioPosJugador : MonoBehaviour
{
    public Client client;
    public Transform jugador;
    public int numeroJugador = 0;
    public float frecuenciaEnvio = 40f;

    private float intervalo;
    private float tiempoAcumulado;

    void Start()
    {
        numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);
        if (client == null)
        {
            client = FindObjectOfType<Client>();
            if (client == null)
                Debug.LogError(" No se encontró el Client en la escena.");
        }

        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (jugador == null)
            {
                Debug.LogWarning(" No se encontró el Transform del jugador. Iniciando búsqueda...");
                StartCoroutine(BuscarPlayer());
            }
        }

        intervalo = 1f / frecuenciaEnvio;
        tiempoAcumulado = 0f;
    }

    IEnumerator BuscarPlayer()
    {
        while (jugador == null)
        {
            var obj = GameObject.Find("Player");
            if (obj != null)
            {
                jugador = obj.transform;
                Debug.Log(" Player encontrado");
                yield break;
            }
            Debug.Log("Buscando Player...");
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (client == null || client.Server == null || jugador == null) return;

        tiempoAcumulado += Time.deltaTime;
        if (tiempoAcumulado >= intervalo)
        {
            tiempoAcumulado = 0f;
            EnviarEstado();
        }
    }

    void EnviarEstado()
    {
      
        if (numeroJugador == 1)
        {
            int posobj = 0;
            if (GlobalData.pausa == true)
            {
                posobj = -3;

            }

            EstadoPosicion estado = new EstadoPosicion
            {
                jugador = numeroJugador,
                pos_x = jugador.position.x,
                pos_y = jugador.position.y,
                pos_z = jugador.position.z,
                obstaculo =   posobj

            };

            string json = JsonUtility.ToJson(estado);
            var writer = new NetDataWriter();
            writer.Put(json);
            client.Server.Send(writer, DeliveryMethod.Sequenced);
           
        }
       
    }
}
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System;
using static UnityEditor.PlayerSettings;

public class RecibirPosJugador : MonoBehaviour
{
    public Client client;
    public Transform jugador;
    public int numeroJugador = 0;

    [Header("Prefabs Obstáculos")]
    public GameObject[] objetosParaColocar;

    void Start()
    {
        numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);
        StartCoroutine(EsperarReferencias());
    }

    IEnumerator EsperarReferencias()
    {
        while (client == null)
        {
            client = FindObjectOfType<Client>();
            if (client != null)
            {
                client.OnMensajeRecibido += ProcesarMensaje;
                Debug.Log("✅ Cliente encontrado y suscrito.");
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        while (jugador == null)
        {
            var obj = GameObject.Find("Player");
            if (obj != null)
            {
                jugador = obj.transform;
                Debug.Log("✅ Player encontrado.");

                Rigidbody rb = jugador.GetComponent<Rigidbody>();
                if (rb != null && numeroJugador == 2)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    Debug.Log("🧲 Física desactivada (jugador 2).");
                }
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnDestroy()
    {
        if (client != null)
            client.OnMensajeRecibido -= ProcesarMensaje;
    }

    private void ProcesarMensaje(string json)
    {
        try
        {
            EstadoPosicion estado = JsonUtility.FromJson<EstadoPosicion>(json);
            if (estado == null) return;

            // 🔹 Jugador 1 mueve su jugador 2
            if (estado.jugador == 1 && jugador != null && numeroJugador == 2)
            {
                if (estado.obstaculo == -3)
                {
                    GlobalData.pausa = true;
                }
                else
                {
                    GlobalData.pausa = false;
                }

                Vector3 nuevaPos = new Vector3(estado.pos_x, estado.pos_y, estado.pos_z);

                jugador.position = Vector3.Lerp(jugador.position, nuevaPos, 0.7f);
            }

            // 🔹 Jugador 2 manda obstáculo → jugador 1 lo crea
            else if (estado.jugador == 2 && numeroJugador == 1)
            {
                if (estado.obstaculo > 0 && objetosParaColocar != null)
                {
                    int index = estado.obstaculo - 1;
                    if (index >= 0 && index < objetosParaColocar.Length)
                    {
                        Vector3 pos = new Vector3(estado.pos_x, estado.pos_y, estado.pos_z);
                        Instantiate(objetosParaColocar[index], pos, Quaternion.identity);
                        Debug.Log($"🧩 Obstáculo recibido del jugador 2: tipo={estado.obstaculo}, pos={pos}");
                    }
                    else if (index + 1 == 5)
                    {
                        GlobalData.mancha = true;
                        Debug.Log($"siiiiiiiiiiiiiiiiiiiiiiiiiestado 5555");
                    }
                    else if (index + 1 == 6)
                    {
                        GlobalData.hielo = true;
                    }
                }
            }
            else if (estado.jugador == 0 && estado.obstaculo==1) {

                GlobalData.inicio = true;
                Debug.Log($"inciccioooooooooooooooooooooooooooooooooo");
            }


        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠ Error al procesar mensaje: {e.Message}\nJSON: {json}");
        }
    }
}

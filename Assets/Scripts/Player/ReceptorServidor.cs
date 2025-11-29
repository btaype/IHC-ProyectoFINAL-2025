using UnityEngine;

public class ReceptorServidor : MonoBehaviour
{
    public void ProcesarMensaje(string msg)
    {
        try
        {
            Debug.Log($"[Servidor] Procesando: {msg}");

            EstadoJugador estado = JsonUtility.FromJson<EstadoJugador>(msg);

            Debug.Log($"[Servidor] Parseado - Carril: {estado.poscarril}, Horizontal: {estado.poshorizontal}, Vel: {estado.velocidad}");

            if (!string.IsNullOrEmpty(estado.poscarril))
            {
                ControladorGeneral.colaMovimientos.Enqueue(estado.poscarril);
                Debug.Log($"✅ Encolado carril: {estado.poscarril}");
            }

            if (!string.IsNullOrEmpty(estado.poshorizontal))
            {
                ControladorGeneral.colaMovimientos.Enqueue(estado.poshorizontal);
                Debug.Log($"✅ Encolado horizontal: {estado.poshorizontal}");
            }

            ControladorGeneral.colaVelocidades.Enqueue(estado.velocidad);
            Debug.Log($"✅ Encolada velocidad: {estado.velocidad}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Servidor] Error: {e.Message}\nJSON: {msg}");
        }
    }
}
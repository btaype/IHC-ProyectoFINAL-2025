using UnityEngine;

public class PauseObjectCleaner : MonoBehaviour
{
    private bool wasPaused = false;

    void Start()
    {
        // Para que puedas probar sin esperar a los dos jugadores
        
        Debug.Log("GlobalData.inicio = true → Listo para probar");
    }

    void Update()
    {
        // PRESIONA LA TECLA P PARA PAUSAR / DESPAUSAR (solo para pruebas)
        if (Input.GetKeyDown(KeyCode.P))
        {
            GlobalData.pausa = !GlobalData.pausa;
            Debug.Log(GlobalData.pausa ? "⏸ JUEGO PAUSADO" : "▶️ JUEGO REANUDADO");
        }

        // Solo cuando se ACTIVA la pausa por primera vez
        if (GlobalData.pausa && !wasPaused)
        {
            DestruirObjetosJugador2();
            wasPaused = true;
        }
        else if (!GlobalData.pausa && wasPaused)
        {
            wasPaused = false; // Permite volver a detectar la próxima pausa
        }
    }

    void DestruirObjetosJugador2()
    {
        int destruidos = 0;

        // Busca TODOS los GameObjects en la escena
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            // Ignora prefabs que no están instanciados
            if (obj.scene.name == null) continue;

            string nombre = obj.name;

            // Detecta los dos objetos reales que crea el Jugador 2
            if (nombre.StartsWith("HotDogStand Limpio(Clone)") || 
                nombre.StartsWith("Barrier(Clone)"))
            {
                Destroy(obj);
                destruidos++;
            }
        }

        Debug.Log($"¡LIMPIEZA COMPLETA! → {destruidos} objetos del Jugador 2 destruidos (HotDogStand y Barrier)");
        
        // Opcional: resetear poderes especiales
        GlobalData.mancha = false;
        GlobalData.hielo = false;
        GlobalData.hielo2 = false;
        GlobalData.mancha2 = false;
        GlobalData.espera = true;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PoderesBotones : MonoBehaviour
{
    public Button botonCeguera;
    public Button botonHielo;

    private void Start()
    {
        if (botonCeguera != null)
        {
            botonCeguera.onClick.RemoveAllListeners();
            botonCeguera.onClick.AddListener(() => {
                GlobalData.mancha = true;   // <-- Activa el poder de mancha
                Debug.Log("Poder MANCHA activado");
            });
        }

        if (botonHielo != null)
        {
            botonHielo.onClick.RemoveAllListeners();
            botonHielo.onClick.AddListener(() => {
                GlobalData.hielo = true;    // <-- Activa el poder de hielo
                Debug.Log("Poder HIELO activado");
            });
        }
    }
}

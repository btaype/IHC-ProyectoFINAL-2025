using UnityEngine;
using UnityEngine.UI;

public class CameraSwapController : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera mainCamera;      // ← Arrastra tu cámara PRINCIPAL
    public Camera minimapCamera;   // ← Arrastra tu MinimapCamera
    
    [Header("UI")]
    public RawImage minimapRawImage;  // ← RawImage del Canvas
    
    [Header("Botón")]
    public Button swapButton;      // ← Botón para intercambiar
    
    [Header("Configuración")]
    public KeyCode swapKey = KeyCode.Tab;  // Tecla alternativa (Tab)
    
    // Estados
    private bool isMainActive = true;  // true = MainCamera activa
    private RenderTexture rt;          // RenderTexture compartido
    
    void Start()
    {
        // Verificar configuraciones
        if (mainCamera == null || minimapCamera == null || minimapRawImage == null)
        {
            Debug.LogError("¡Asigna TODAS las cámaras y RawImage!");
            return;
        }
        
        // Crear RenderTexture si no existe
        if (rt == null)
        {
            rt = new RenderTexture(256, 256, 24);
            minimapRawImage.texture = rt;
        }
        
        // Configurar cámaras iniciales
        ConfigurarCamaras();
        
        // Conectar botón
        if (swapButton != null)
            swapButton.onClick.AddListener(IntercambiarCamaras);
    }
    
    void Update()
    {
        // Intercambio con tecla TAB (opcional)
        if (Input.GetKeyDown(swapKey))
            IntercambiarCamaras();
    }
    
    void IntercambiarCamaras()
    {
        isMainActive = !isMainActive;
        ConfigurarCamaras();
        Debug.Log($"🎛️ Cámaras intercambiadas → {(isMainActive ? "MAIN activa" : "MINIMAP activa")}");
    }
    
    void ConfigurarCamaras()
    {
        if (isMainActive)
        {
            // MAIN activa (fullscreen), Minimap en RawImage
            mainCamera.targetTexture = null;           // Renderiza a pantalla
            mainCamera.depth = 0;                      // Superior
            minimapCamera.targetTexture = rt;          // Renderiza a RawImage
            minimapCamera.depth = -1;                  // Inferior
        }
        else
        {
            // MINIMAP activa (fullscreen), Main en RawImage
            minimapCamera.targetTexture = null;        // Renderiza a pantalla
            minimapCamera.depth = 0;                   // Superior
            mainCamera.targetTexture = rt;             // Renderiza a RawImage
            mainCamera.depth = -1;                     // Inferior
        }
    }
}
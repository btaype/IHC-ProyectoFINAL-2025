# Tutorial para la Compilación del Proyecto

## Estructura de Ramas del Proyecto

Este repositorio se organiza en **tres ramas principales**, cada una con un propósito específico:

###  master  
Contiene el **proyecto de Unity**, donde se encuentra el juego completo.  
Aquí se desarrolla toda la lógica del cliente, escenas, UI y gameplay.

###  server  
Rama dedicada al **servidor multijugador**, responsable de gestionar las conexiones, salas y sincronización entre jugadores.

###  tracker  
Incluye los **scripts y herramientas de tracking** usados para enviar acciones al proyecto Unity  
y permitir el movimiento del personaje dentro del juego.
##  Recomendaciones de Distribución de Carpetas

![Captura](https://github.com/user-attachments/assets/47b6827a-38f0-495b-adb9-7a776365e50b)

## Dependencias
### server
-.NET SDK 8.0.415
-LiteNetLib(dotnet add package LiteNetLib)
###  Tracker (Python)
El módulo **tracker** requiere Python **3.10 o superior** y las siguientes dependencias:
- **opencv-python**
- **numpy**
- **mediapipe**
- **pickle-mixin**
- **jsons**
### Versión de Unity Utilizada
**Unity 6000.0.43f1**
##  Paso 1: Clonar las Ramas del Proyecto
Para seguir la distribución recomendada de carpetas, es necesario clonar cada rama del repositorio en carpetas diferentes.

Ejecuta los siguientes comandos:

```bash
# Clonar la rama principal del juego (Unity)
git clone -b master --single-branch https://github.com/btaype/IHC-ProyectoFINAL-2025.git 

# Clonar la rama del servidor multijugador
git clone -b server --single-branch https://github.com/btaype/IHC-ProyectoFINAL-2025.git 

# Clonar la rama del tracker
git clone -b tracker --single-branch https://github.com/btaype/IHC-ProyectoFINAL-2025.git 

```

##  Paso 2: Compilar y Ejecutar el Servidor

Antes de compilar el servidor, asegúrate de estar dentro de la carpeta correcta, tal como se muestra en la estructura recomendada.

###  1. Abrir la carpeta del servidor
Navega a la carpeta donde clonaste la rama `server`, por ejemplo:

![Captura222](https://github.com/user-attachments/assets/c498b411-df23-4746-9d1e-c38ad0917028)

Abre una terminal o CMD dentro de esa ruta.

###  2. Instalar la dependencia del servidor
El servidor utiliza **LiteNetLib** para manejar las conexiones.  
Instálala ejecutando:

```bash
dotnet add package LiteNetLib
```

###  3. Ejecutar el servidor
```bash
dotnet run
```
##  Paso 3: Ejecutar el Proyecto Unity desde Unity Hub

###  1. Agregar el proyecto al Unity Hub
Una vez clonada la rama `master` (carpeta del juego), debes agregar el proyecto a Unity Hub.

Hazlo así:

1. Abre **Unity Hub**.
2. Haz clic en **Add Project from Disk** (Agregar proyecto desde disco).
3. Navega hasta la carpeta donde clonaste el proyecto Unity.  
   Por ejemplo:
![Captur333a](https://github.com/user-attachments/assets/0b7f3155-862a-4d81-8b93-aba045cdc5e8)

Unity Hub te pedirá:
- **instalar** esa versión, o  
- **abrir** el proyecto con esa versión si ya la tienes instalada
![Ceeeaptura](https://github.com/user-attachments/assets/e0166cf3-c860-4cb3-bc8a-f6be8f92d0d5)

###  3. Ejecutar el proyecto
Cuando el proyecto cargue:
1. Estar en la carpeta principal del proyecto (`Project`), luego ir a **Assets** y finalmente a **Scenes**.
![CaeeeweptDDDDDura](https://github.com/user-attachments/assets/88e9e70d-365f-4c60-9c92-e4ce55316fda)
2. Dentro de la carpeta **Scenes**, encontrarás dos escenas importantes:
   - **MENU_CORREDOR (1):** escena del jugador que corre y evita obstáculos.
   - **MENU_SABOTEADOR (2):** escena del jugador que corre y coloca obstáculos.
   ![2222222Captura](https://github.com/user-attachments/assets/f0a74c69-daf9-4d9c-8249-fc46fce47038)
3. Ejecutar el juego es tan simple como seleccionar cualquiera de los dos menús mencionados y presionar **Play**
![22324242Captura](https://github.com/user-attachments/assets/299e3897-cf34-4f1e-8352-66a0e0c5c7c5)
![CaSSSSSStura](https://github.com/user-attachments/assets/a762d74f-55be-43e5-b939-61d228448130)


##  Paso 4: Ejecutar el Python Tracker
1. Estar dentro de la carpeta donde se encuentra el contenido del tracker (clonado desde la rama `tracker`) y abrir una ventana de **CMD** en esa ruta.
![ssdsdsdsdsfeweaptura](https://github.com/user-attachments/assets/371ee0c4-d101-4fc1-94e7-12f761a6ced9)
2. Instalar las dependencias usando el archivo `requirements.txt` con el siguiente comando:

```bash
pip install -r requirements.txt
Copiar código
```
3. Ejecutar el tracker usando el archivo principal (por ejemplo `tracker7.py`) con:

```bash
python tracker7.py
```
![Capturatttttttttttttt](https://github.com/user-attachments/assets/40fd225c-90f8-4ead-a4a7-067daa98bb05)


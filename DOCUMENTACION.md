# Documentación técnica y funcional de ZombieTech

## 1. Datos generales

| Campo | Información |
|---|---|
| Nombre del proyecto | ZombieTech |
| Autor | Romario Anchaise |
| Tipo de proyecto | Videojuego 2D de plataformas y supervivencia |
| Motor | Unity 6.3 LTS (6000.3.22f1) |
| Lenguaje | C# |
| Plataforma objetivo inicial | Windows |
| Estado | Prototipo jugable en desarrollo |
| Repositorio | https://github.com/romario-anchaise/ZombieTech |

## 2. Descripción del proyecto

ZombieTech es un videojuego 2D ambientado en una ciudad destruida después de un brote zombi. El jugador controla a un superviviente que debe desplazarse por el escenario, saltar sobre obstáculos urbanos y, en futuras versiones, enfrentarse o escapar de enemigos mientras recolecta recursos.

La dirección visual combina pixel art con un ambiente urbano oscuro y posapocalíptico. El prototipo actual se concentra en establecer una base sólida de movimiento, animación, físicas, colisiones, plataformas y ambientación sonora.

## 3. Objetivos

### Objetivo general

Desarrollar un videojuego 2D funcional en Unity en el que el jugador pueda explorar un escenario posapocalíptico utilizando controles de movimiento y salto, animaciones coherentes y objetos del entorno con colisiones.

### Objetivos específicos

- Implementar movimiento horizontal fluido mediante físicas 2D.
- Incorporar salto normal y salto corto según la duración de la pulsación.
- Representar visualmente los estados de reposo, caminata, salto y caída.
- Integrar elementos del fondo como superficies con las que el jugador pueda interactuar.
- Mantener una ambientación coherente mediante arte y música de fondo.
- Preparar una estructura de proyecto que permita agregar enemigos, vidas, objetos y niveles posteriormente.

## 4. Concepto de juego

### Género

Plataformas 2D con elementos de acción y supervivencia.

### Ambientación

Ciudad abandonada con vehículos destruidos, barreras de concreto, edificios deteriorados, vegetación y restos de infraestructura urbana.

### Bucle de juego propuesto

1. El jugador explora el escenario.
2. Supera obstáculos mediante movimiento y saltos.
3. Recoge recursos como medicina, agua, baterías o munición.
4. Evita o combate zombis.
5. Alcanza una salida o punto seguro para completar el nivel.

Actualmente están implementados la exploración, los obstáculos y el combate básico contra un zombi. La recolección de recursos y la salida del nivel forman parte del desarrollo pendiente.

## 5. Controles

| Acción | Teclado |
|---|---|
| Caminar a la izquierda | `A` o flecha izquierda |
| Caminar a la derecha | `D` o flecha derecha |
| Saltar | Barra espaciadora |
| Realizar un salto corto | Soltar la barra espaciadora antes de alcanzar la altura máxima |
| Bajar a través de una plataforma | `S` o flecha abajo |
| Disparar estando quieto | `F` o `Ctrl` izquierdo |

## 6. Funcionalidades implementadas

### 6.1. Movimiento del personaje

El personaje utiliza un componente `Rigidbody2D` para responder a la gravedad y a las colisiones. El movimiento horizontal aplica aceleración y desaceleración para evitar cambios bruscos de velocidad.

Parámetros principales:

| Parámetro | Valor |
|---|---:|
| Velocidad horizontal | 5.5 |
| Aceleración | 45 |
| Desaceleración | 55 |
| Fuerza de salto | 11.5 |
| Gravedad | 3.2 |
| Velocidad máxima de caída | 18 |
| Tiempo de gracia para saltar | 0.12 segundos |
| Buffer de entrada del salto | 0.12 segundos |
| Multiplicador de salto corto | 0.5 |

El tiempo de gracia permite saltar durante un instante después de abandonar una superficie. El buffer conserva durante unas décimas la pulsación realizada justo antes de aterrizar. Ambas técnicas mejoran la respuesta del control.

### 6.2. Animaciones

El controlador de animaciones contiene seis estados:

- `Idle`: personaje en reposo.
- `Walk`: personaje caminando.
- `Jump`: movimiento ascendente.
- `Fall`: movimiento descendente.
- `Shoot`: secuencia de seis frames al disparar una pistola estando quieto.
- `Death`: secuencia de seis frames que termina con el personaje inmóvil.

Las transiciones utilizan los parámetros `Speed`, `VerticalSpeed`, `IsGrounded` y los disparadores `Shoot` y `Die`. El sprite se invierte horizontalmente para permitir movimiento hacia ambos lados sin duplicar todas las imágenes.

### 6.3. Colisiones y plataformas

El suelo principal utiliza un `BoxCollider2D` invisible alineado con la calle del fondo. Los objetos saltables pertenecen a la capa `Ground`, la misma capa consultada por el controlador del personaje.

Se configuraron como plataformas:

- El automóvil destruido del lado izquierdo.
- La barrera de concreto ubicada en la zona central.

Cada objeto utiliza `BoxCollider2D` y `PlatformEffector2D`. Esta configuración permite aterrizar sobre la superficie sin crear paredes invisibles en sus costados. El jugador también puede bajar a través de la plataforma con `S` o flecha abajo.

### 6.4. Música de fondo

La canción `BecomeTheAssassin.mp3` se carga desde `Resources/Audio` al iniciar el juego. El sistema crea automáticamente un `AudioSource` con las siguientes propiedades:

- Reproducción automática al comenzar.
- Repetición continua.
- Audio 2D, sin cambio de volumen por distancia.
- Volumen configurado al 30 %.
- Conservación del reproductor durante cambios de escena.

### 6.5. Escenario

La escena principal utiliza un fondo urbano posapocalíptico ajustado a la altura de la cámara ortográfica. El fondo se renderiza detrás del personaje y de los demás objetos mediante su orden de dibujo.

### 6.6. Disparo animado

La acción de disparo se activa con `F` o `Ctrl` izquierdo únicamente cuando el personaje está en el suelo y permanece quieto. El Animator recibe el parámetro tipo `Trigger` llamado `Shoot` y reproduce `Player_Shoot.anim`, una secuencia no repetitiva de seis fotogramas a 12 FPS.

La hoja original de disparo tenía un tablero blanco y gris incrustado en la propia imagen. Por eso se creó `PlayerShootCheckerboardKey.mat`, basado en `CheckerboardKeySprite.shader`. El controlador aplica ese material durante 0.5 segundos y luego restaura el material normal. La textura también se importa con filtro `Point`, sin mipmaps y sin compresión para impedir que Unity mezcle el fondo con los bordes del personaje.

A los 0.22 segundos de iniciar la animación, `PlayerController2D` instancia `Assets/Prefabs/Bullet.prefab` frente a la pistola. La bala reproduce cuatro fotogramas, se desplaza a 13 unidades por segundo y se destruye después de 2 segundos o al tocar un obstáculo. La dirección depende de la orientación del personaje, por lo que funciona hacia la izquierda y hacia la derecha. El material `BulletAdditive.mat` hace invisible el fondo negro de la hoja mediante el shader `AdditiveProjectile.shader`.

### 6.7. Primer enemigo

El nivel contiene un solo zombi obrero cabezón situado en el lado derecho. Utiliza ocho fotogramas de caminata, seis de ataque, `Rigidbody2D`, `CapsuleCollider2D`, `Animator` y el script `ZombiePatrol.cs`. Mientras exista el objeto `Player`, el enemigo lo persigue por todo el escenario y comienza el ataque al quedar a 0.9 unidades. El golpe se aplica después de 0.32 segundos para que la acción coincida con el frame de contacto.

Cuando recibe el ataque, `PlayerController2D.Die()` detiene el desplazamiento, bloquea nuevas entradas y activa `Die` en el Animator. `Player_Death.anim` reproduce seis fotogramas y permanece en la pose final porque el estado `Death` no tiene transición de salida.

Después de terminar la caída del jugador, `ZombiePatrol` oculta su sprite original, alinea al enemigo con el cuerpo y activa el estado `Eat`. `WorkerZombie_Eat.anim` reproduce en bucle seis fotogramas con ambos personajes y sangre estilizada. Los pivotes se calcularon a partir del borde inferior de cada hoja para que las poses de muerte y alimentación permanezcan apoyadas sobre el suelo.

El enemigo se guardó como `Assets/Prefabs/WorkerZombie.prefab` y la escena contiene exactamente una instancia dentro del objeto organizador `Enemies`. Esto demuestra el uso de Prefabs solicitado por la rúbrica sin llenar el escenario de copias.

El componente `ZombieHealth` asigna 3 puntos de vida al enemigo. Cada proyectil causa 1 punto de daño, muestra un destello rojo como respuesta visual y se destruye al impactar. El tercer disparo cancela el movimiento y cualquier ataque pendiente, desactiva las colisiones y reproduce `WorkerZombie_Death.anim`, una secuencia de seis fotogramas. La instancia se elimina al terminar la animación.

## 7. Arquitectura del proyecto

```text
Assets/
├── Animations/Player/       Animaciones y Animator Controller
├── Animations/              Animaciones del jugador, zombi y proyectil
├── Art/
│   ├── Backgrounds/         Fondo de la ciudad
│   ├── Characters/          Hojas de sprites de personajes
│   └── Projectiles/         Hoja de cuatro frames de la bala
├── Editor/                  Herramientas de configuración automática
├── Materials/               Material normal y material especial de disparo
├── Prefabs/                 Prefabs reutilizables del zombi y la bala
├── Resources/Audio/         Música cargada durante la ejecución
├── Scenes/                  Escena principal
├── Scripts/                 Código ejecutado por el juego
├── Settings/                Configuración de URP 2D e Input System
└── Shaders/                 Filtros visuales para personaje y proyectil
```

## 8. Scripts principales

| Script | Responsabilidad |
|---|---|
| `PlayerController2D.cs` | Lee el teclado, mueve el personaje, controla el salto, detecta el suelo, permite bajar por plataformas y actualiza las animaciones. |
| `BackgroundMusic.cs` | Carga y reproduce la música de fondo de manera automática y persistente. |
| `PlayerSetupBuilder.cs` | Configura sprites, animaciones, físicas, capa de suelo y componentes del jugador desde el editor. |
| `ZombieBackgroundInstaller.cs` | Importa, escala y coloca el fondo en la escena. |
| `EnvironmentPlatformInstaller.cs` | Crea y alinea las superficies saltables con los elementos dibujados en el escenario. |
| `CheckerboardKeySprite.shader` | Descarta los tonos del tablero que venían incrustados en la hoja de disparo. |
| `ZombiePatrol.cs` | Mueve al zombi automáticamente dentro de su zona de patrullaje y actualiza su orientación. |
| `ZombieSetupBuilder.cs` | Importa la hoja del enemigo, crea su animación, material, Animator, prefab y única instancia. |
| `BulletProjectile.cs` | Mueve la bala, controla su tiempo de vida y aplica daño cuando impacta al zombi. |
| `ZombieHealth.cs` | Gestiona los 3 puntos de vida, el destello de impacto y la eliminación del enemigo. |
| `ZombieAudio.cs` | Reproduce voces ambientales, ataques, daño y muerte con pequeñas variaciones de tono. |
| `BulletSetupBuilder.cs` | Divide la hoja de bala, crea su animación, material, controlador y prefab, y lo asigna al jugador. |

Los scripts de la carpeta `Editor` solo se ejecutan dentro del editor de Unity. Los scripts de la carpeta `Scripts` forman parte del juego final.

## 9. Jerarquía principal de la escena

```text
SampleScene
├── Main Camera
├── Global Light 2D
├── Environment
│   ├── Background_ZombieCity
│   └── Jumpable Environment
│       ├── Wrecked Car
│       └── Concrete Barrier
├── GroundCollider
├── Enemies
│   └── Worker Zombie
└── Player
```

El objeto de música se crea durante la ejecución y recibe el nombre `Background Music`.

## 10. Requisitos para ejecutar el proyecto

- Unity Hub.
- Unity 6.3 LTS o una versión compatible.
- Módulo de compilación para Windows si se desea generar un ejecutable.
- Sistema de entrada nuevo de Unity, incluido en las dependencias del proyecto.
- Universal Render Pipeline 2D, incluido en las dependencias del proyecto.

## 11. Cómo abrir y probar el juego

1. Descargar o clonar el repositorio.
2. Abrir Unity Hub.
3. Seleccionar **Add project from disk**.
4. Elegir la carpeta raíz de ZombieTech.
5. Esperar a que Unity importe los recursos y compile los scripts.
6. Abrir `Assets/Scenes/SampleScene.unity`.
7. Presionar el botón **Play**.
8. Probar el movimiento, el salto y las plataformas con los controles indicados.

Si Unity informa que la escena cambió en el disco, se debe seleccionar **Reload** para cargar la versión actualizada.

## 12. Casos de prueba básicos

| Prueba | Procedimiento | Resultado esperado |
|---|---|---|
| Movimiento derecho | Mantener `D` | El personaje avanza y reproduce la animación de caminata. |
| Movimiento izquierdo | Mantener `A` | El personaje avanza a la izquierda y cambia su orientación. |
| Salto completo | Mantener espacio | El personaje asciende, alcanza la altura máxima y cae. |
| Salto corto | Pulsar y soltar rápidamente espacio | El personaje alcanza una altura menor. |
| Aterrizaje | Saltar sobre el suelo | Se activa la animación de reposo o caminata al tocar el piso. |
| Plataforma | Saltar sobre la barrera | El personaje aterriza sobre la superficie visible. |
| Salida lateral | Caminar fuera de una plataforma | El personaje cae sin chocar con una pared invisible. |
| Descenso voluntario | Pulsar `S` sobre una plataforma | El personaje atraviesa la superficie y cae al suelo. |
| Música | Iniciar la escena | La canción comienza y continúa en bucle. |

## 13. Limitaciones actuales

- El zombi persigue y mata al jugador, pero todavía no existe un sistema gradual de vidas o puntos de salud.
- El zombi desaparece después de su animación de muerte; todavía no deja recompensas ni objetos.
- No existen objetos coleccionables ni inventario.
- La cámara todavía no sigue al jugador durante un nivel extenso.
- Solo existe una escena jugable.
- No se han creado menús de inicio, pausa o finalización.
- No se ha generado una compilación ejecutable para distribución.

## 14. Próximas mejoras recomendadas

1. Agregar una opción para reiniciar después de morir.
2. Mostrar la vida del jugador y del enemigo en la interfaz.
3. Agregar recompensas o munición al derrotar al zombi.
4. Añadir botiquines, baterías y otros objetos coleccionables.
5. Incorporar una cámara que siga al personaje y límites de nivel.
6. Crear interfaz con vida, cantidad de objetos y objetivo actual.
7. Agregar efectos de sonido para pasos, salto, daño y enemigos.
8. Diseñar una condición de victoria y una condición de derrota.
9. Añadir menú principal y pantalla de resultados.
10. Realizar pruebas y generar la primera compilación para Windows.

## 15. Recursos y licencias

- El proyecto utiliza recursos gráficos propios o incorporados específicamente para este prototipo.
- La música fue proporcionada para su integración en el proyecto.
- Antes de publicar o comercializar el juego debe verificarse que se cuenta con autorización para distribuir cada imagen, sprite y archivo de audio.
- Los futuros recursos externos deben conservar su archivo de licencia y los créditos exigidos por sus autores.
- Se recomienda priorizar recursos con licencia CC0 o licencias que permitan expresamente su uso en videojuegos.
- Las seis voces del zombi proceden de **Zomby SFX Pack**, creado por saturn91 y publicado con licencia CC0 en OpenGameArt: https://opengameart.org/content/zomby-sfx-pack. La atribución no es obligatoria, pero se conserva `Assets/Audio/Zombie/LICENSE.txt` como registro de procedencia.

## 16. Control de versiones

El código fuente se administra con Git y la rama principal se denomina `main`. Los directorios generados automáticamente por Unity, como `Library`, `Temp`, `Logs` y `obj`, están excluidos mediante `.gitignore`.

Para registrar un nuevo avance se recomienda:

```bash
git status
git add .
git commit -m "Descripción clara del avance"
git push
```

## 17. Estado del avance

El prototipo permite recorrer la escena, saltar, reproducir las animaciones correspondientes, interactuar con superficies del escenario y escuchar música de fondo. Esto constituye la base jugable sobre la cual se incorporarán enemigos, objetivos, interfaz y progresión.

## 18. Procedimiento realizado en Unity

### 18.1. Integración de la música

1. Se copió `BecomeTheAssassin.mp3` a `Assets/Resources/Audio/`.
2. Se creó `Assets/Scripts/BackgroundMusic.cs`.
3. Se añadió el atributo `RuntimeInitializeOnLoadMethod` para iniciar el sistema antes de cargar la escena.
4. El script busca el audio con `Resources.Load<AudioClip>("Audio/BecomeTheAssassin")`.
5. Durante **Play** se crea el objeto `Background Music` con un componente `AudioSource`.
6. Se activó `loop`, se configuró el volumen en `0.3` y se estableció `spatialBlend = 0` para reproducirlo como audio 2D.
7. Se utilizó `DontDestroyOnLoad` para conservar la música al cambiar de escena.

Para comprobarlo en Unity: presionar **Play**, seleccionar `Background Music` en **Hierarchy** y revisar `Audio Source` en **Inspector**.

### 18.2. Preparación de las hojas de fotogramas

Las imágenes se guardaron en `Assets/Art/Characters/Survivor/Processed/`:

| Hoja | División | Uso |
|---|---:|---|
| `Survivor_Walk.png` | 8 sprites | Reposo y caminata |
| `Survivor_JumpFall.png` | 6 sprites | Cuatro frames de subida y dos de caída |
| `Survivor_Shoot.png` | 6 sprites | Secuencia completa del disparo |
| `Survivor_Death.png` | 6 sprites | Reacción al ataque y muerte |

La herramienta `PlayerSetupBuilder.cs` configura automáticamente cada textura como **Sprite (2D and UI)** y **Sprite Mode: Multiple**. Divide horizontalmente la imagen, asigna un pivote personalizado en `(0.5, 0.18)` para alinear los pies y utiliza `300 Pixels Per Unit`.

Los clips generados tienen esta velocidad:

| Clip | FPS | Loop |
|---|---:|---|
| `Player_Idle.anim` | 1 | Sí |
| `Player_Walk.anim` | 10 | Sí |
| `Player_Jump.anim` | 8 | No |
| `Player_Fall.anim` | 6 | No |
| `Player_Shoot.anim` | 12 | No |
| `Player_Death.anim` | 6 | No |

Para ver los fotogramas manualmente: seleccionar una hoja en **Project**, abrir **Inspector**, desplegar la flecha del archivo o pulsar **Sprite Editor**. Para revisar los clips: abrir `Assets/Animations/Player/` y seleccionar cada archivo `.anim`.

### 18.3. Creación del Animator

El archivo `Assets/Animations/Player/PlayerAnimator.controller` contiene los estados `Idle`, `Walk`, `Jump`, `Fall`, `Shoot` y `Death`. Sus parámetros son:

| Parámetro | Tipo | Función |
|---|---|---|
| `Speed` | Float | Cambia entre reposo y caminata |
| `VerticalSpeed` | Float | Distingue subida y caída |
| `IsGrounded` | Bool | Indica si el personaje está en el suelo |
| `Shoot` | Trigger | Inicia una sola secuencia de disparo |
| `Die` | Trigger | Activa la muerte y deja la pose final |

Para verlo en Unity: seleccionar `PlayerAnimator.controller` y abrir **Window > Animation > Animator**.

### 18.4. Configuración física del personaje

En `Hierarchy > Player > Inspector` se agregaron `Sprite Renderer`, `Rigidbody 2D`, `Capsule Collider 2D`, `Animator` y `Player Controller 2D`. El `Rigidbody2D` mueve al personaje mediante velocidad, sin modificar directamente el `Transform`. El `CapsuleCollider2D` representa su cuerpo y la capa `Ground` identifica las superficies válidas.

El suelo usa `BoxCollider2D`. El automóvil y la barrera usan `BoxCollider2D` junto con `PlatformEffector2D`; por eso se puede aterrizar desde arriba, atravesarlos desde abajo y salir por sus costados sin encontrar una pared invisible.

## 19. Código utilizado

### 19.1. Reproducción de música

Fragmento principal de `BackgroundMusic.cs`:

```csharp
AudioClip musicClip = Resources.Load<AudioClip>("Audio/BecomeTheAssassin");
GameObject musicObject = new GameObject("Background Music");
Object.DontDestroyOnLoad(musicObject);

AudioSource audioSource = musicObject.AddComponent<AudioSource>();
audioSource.clip = musicClip;
audioSource.loop = true;
audioSource.volume = 0.3f;
audioSource.spatialBlend = 0f;
audioSource.Play();
```

### 19.2. Movimiento horizontal con físicas

Fragmento de `PlayerController2D.cs` ejecutado en `FixedUpdate`:

```csharp
float targetSpeed = horizontalInput * moveSpeed;
float changeRate = Mathf.Abs(targetSpeed) > 0.01f
    ? acceleration
    : deceleration;

float nextSpeed = Mathf.MoveTowards(
    body.linearVelocityX,
    targetSpeed,
    changeRate * Time.fixedDeltaTime);

body.linearVelocity = new Vector2(nextSpeed, body.linearVelocityY);
```

### 19.3. Salto y salto corto

```csharp
if (jumpBufferCounter > 0f && coyoteCounter > 0f)
{
    body.linearVelocity = new Vector2(body.linearVelocityX, jumpForce);
    jumpBufferCounter = 0f;
    coyoteCounter = 0f;
}

if (!jumpHeld && body.linearVelocityY > 0f)
    body.linearVelocity = new Vector2(
        body.linearVelocityX,
        body.linearVelocityY * shortJumpMultiplier);
```

### 19.4. Activación del disparo

```csharp
bool shootPressed = keyboard.fKey.wasPressedThisFrame ||
                    keyboard.leftCtrlKey.wasPressedThisFrame;

if (shootPressed && isGrounded &&
    Mathf.Abs(horizontalInput) < 0.01f &&
    Time.time >= nextShootTime)
{
    nextShootTime = Time.time + shootCooldown;
    animator.SetTrigger("Shoot");
    StartCoroutine(UseShootMaterialTemporarily());
    bulletSpawnRoutine = StartCoroutine(SpawnBulletAfterDelay());
}
```

La primera corrutina coloca el material especial durante `0.5` segundos y luego devuelve el material normal. La segunda espera `0.22` segundos para sincronizar la salida de la bala con el fogonazo de la pistola, instancia `Bullet.prefab` y le entrega la dirección actual del personaje. `BulletProjectile.OnTriggerEnter2D` busca `ZombieHealth` en el objeto impactado y le aplica 1 punto de daño.

### 19.5. Actualización del Animator

```csharp
animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
animator.SetFloat("VerticalSpeed", body.linearVelocityY);
animator.SetBool("IsGrounded", isGrounded);
```

### 19.6. Herramientas de Editor

`PlayerSetupBuilder.cs`, `ZombieBackgroundInstaller.cs` y `EnvironmentPlatformInstaller.cs` usan la API `UnityEditor`. Automatizan la importación de sprites, creación de clips, estados del Animator, materiales, capas, colliders y objetos de escena. Estos scripts aparecen dentro de `Assets/Editor`, por lo que solo funcionan en el Editor y no se incluyen como lógica del ejecutable.

## 20. Relación con la rúbrica del Hito 1

| Criterio | Evidencia actual | Estado |
|---|---|---|
| Programación C# e interactividad | Campos privados con `[SerializeField]`, nombres consistentes, `Update` para entrada y `FixedUpdate` para física | Implementado |
| Físicas 2D y Game Feel | `Rigidbody2D`, aceleración, desaceleración, salto variable, coyote time y jump buffer | Implementado |
| Colisiones, Tags y Layers | Capa `Ground`, `CapsuleCollider2D`, `BoxCollider2D` y `PlatformEffector2D` | Implementado |
| Gestión de assets | Carpetas separadas para arte, audio, animaciones, materiales, escenas y scripts | Implementado |
| Git | Repositorio público, `.gitignore` de Unity y commits descriptivos | Implementado |
| Personaje con movimiento y acción | Movimiento, salto, descenso por plataformas y animación de disparo | Implementado |
| Mecánica de interacción | El zombi ataca al jugador y las balas le causan daño hasta eliminarlo | Implementado |
| Prefabs repetitivos | `WorkerZombie.prefab` permite crear enemigos reutilizables; la escena usa una instancia | Implementado |
| Inicio y reinicio | La escena inicia con **Play**, pero aún no existe botón o condición de reinicio | Pendiente |
| Video de demostración | Debe grabarse un video de máximo dos minutos mostrando gameplay y explicando una parte del código | Pendiente |

La interacción principal ya es verificable mediante balas que impactan y dañan al zombi. Los siguientes pasos prioritarios son agregar reinicio, una interfaz de vida y comprobar que la consola permanezca libre de errores durante la demostración.

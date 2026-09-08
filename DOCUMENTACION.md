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

Actualmente están implementados los dos primeros pasos. Los demás forman parte del desarrollo pendiente.

## 5. Controles

| Acción | Teclado |
|---|---|
| Caminar a la izquierda | `A` o flecha izquierda |
| Caminar a la derecha | `D` o flecha derecha |
| Saltar | Barra espaciadora |
| Realizar un salto corto | Soltar la barra espaciadora antes de alcanzar la altura máxima |
| Bajar a través de una plataforma | `S` o flecha abajo |

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

El controlador de animaciones contiene cuatro estados:

- `Idle`: personaje en reposo.
- `Walk`: personaje caminando.
- `Jump`: movimiento ascendente.
- `Fall`: movimiento descendente.

Las transiciones utilizan los parámetros `Speed`, `VerticalSpeed` e `IsGrounded`. El sprite se invierte horizontalmente para permitir movimiento hacia ambos lados sin duplicar todas las imágenes.

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

## 7. Arquitectura del proyecto

```text
Assets/
├── Animations/Player/       Animaciones y Animator Controller
├── Art/
│   ├── Backgrounds/         Fondo de la ciudad
│   └── Characters/          Sprites del superviviente
├── Editor/                  Herramientas de configuración automática
├── Materials/               Material utilizado por el personaje
├── Resources/Audio/         Música cargada durante la ejecución
├── Scenes/                  Escena principal
├── Scripts/                 Código ejecutado por el juego
└── Settings/                Configuración de URP 2D e Input System
```

## 8. Scripts principales

| Script | Responsabilidad |
|---|---|
| `PlayerController2D.cs` | Lee el teclado, mueve el personaje, controla el salto, detecta el suelo, permite bajar por plataformas y actualiza las animaciones. |
| `BackgroundMusic.cs` | Carga y reproduce la música de fondo de manera automática y persistente. |
| `PlayerSetupBuilder.cs` | Configura sprites, animaciones, físicas, capa de suelo y componentes del jugador desde el editor. |
| `ZombieBackgroundInstaller.cs` | Importa, escala y coloca el fondo en la escena. |
| `EnvironmentPlatformInstaller.cs` | Crea y alinea las superficies saltables con los elementos dibujados en el escenario. |

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

- Todavía no existen zombis controlados por inteligencia artificial.
- No se ha implementado combate, daño ni sistema de vidas.
- No existen objetos coleccionables ni inventario.
- La cámara todavía no sigue al jugador durante un nivel extenso.
- Solo existe una escena jugable.
- No se han creado menús de inicio, pausa o finalización.
- No se ha generado una compilación ejecutable para distribución.

## 14. Próximas mejoras recomendadas

1. Importar y configurar sprites animados de zombis.
2. Crear patrullaje, persecución y ataque para los enemigos.
3. Implementar salud, daño, muerte y reinicio del jugador.
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


# ZombieTech

Prototipo funcional de videojuego 2D desarrollado con Unity 6.3 LTS.

## Avance actual

- Escenario urbano posapocaliptico.
- Personaje controlable con Rigidbody2D.
- Movimiento horizontal con aceleracion y desaceleracion.
- Salto variable, tiempo de gracia y buffer de entrada.
- Animaciones de reposo, caminar, saltar, caer, disparar y morir.
- Giro automatico del personaje hacia ambos lados.
- Colisiones mediante la capa Ground.
- Suelo invisible alineado con el escenario.
- Automovil destruido y barrera de concreto utilizables como plataformas.
- Plataformas atravesables con S o flecha abajo.
- Musica de fondo automatica y en bucle.
- Disparo de proyectiles animados hacia ambos lados.
- Un zombie obrero cabezon, creado como prefab, que persigue, ataca y reproduce su muerte.
- Sistema de 3 puntos de vida para el zombie; cada bala causa 1 punto de dano y el tercer impacto activa su muerte.
- Animacion de ataque sincronizada con la muerte del personaje.
- Voces del zombie para ambiente, ataque, daño y muerte.
- Animacion en bucle del zombie alimentandose del jugador despues del Game Over.

## Controles

- A o flecha izquierda: caminar a la izquierda.
- D o flecha derecha: caminar a la derecha.
- Espacio: saltar.
- Soltar Espacio anticipadamente: salto corto.
- S o flecha abajo: bajar a traves de una plataforma.
- F o Ctrl izquierdo: disparar una bala estando quieto.

## Documentacion

La descripcion funcional, arquitectura, casos de prueba y proximas mejoras se encuentran en [DOCUMENTACION.md](DOCUMENTACION.md).

La version visual preparada en LaTeX se encuentra en [DOCUMENTACION_ZOMBIETECH.tex](DOCUMENTACION_ZOMBIETECH.tex).

## Abrir el proyecto

1. Abrir Unity Hub.
2. Seleccionar Add project from disk.
3. Elegir la carpeta raiz de este repositorio.
4. Abrir `Assets/Scenes/SampleScene.unity`.
5. Presionar Play.

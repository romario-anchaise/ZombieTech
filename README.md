# ZombieTech

Prototipo funcional de videojuego 2D desarrollado con Unity 6.3 LTS.

## Avance actual

- Escenario urbano posapocaliptico.
- Personaje controlable con Rigidbody2D.
- Movimiento horizontal con aceleracion y desaceleracion.
- Salto variable, tiempo de gracia y buffer de entrada.
- Animaciones de reposo, caminar, saltar y caer.
- Giro automatico del personaje hacia ambos lados.
- Colisiones mediante la capa Ground.
- Suelo invisible alineado con el escenario.
- Automovil destruido y barrera de concreto utilizables como plataformas.
- Plataformas atravesables con S o flecha abajo.
- Musica de fondo automatica y en bucle.
- Animacion del personaje disparando quieto con una pistola.

## Controles

- A o flecha izquierda: caminar a la izquierda.
- D o flecha derecha: caminar a la derecha.
- Espacio: saltar.
- Soltar Espacio anticipadamente: salto corto.
- S o flecha abajo: bajar a traves de una plataforma.
- F o Ctrl izquierdo: reproducir la animacion de disparo estando quieto.

## Documentacion

La descripcion funcional, arquitectura, casos de prueba y proximas mejoras se encuentran en [DOCUMENTACION.md](DOCUMENTACION.md).

La version visual preparada en LaTeX se encuentra en [DOCUMENTACION_ZOMBIETECH.tex](DOCUMENTACION_ZOMBIETECH.tex).

## Abrir el proyecto

1. Abrir Unity Hub.
2. Seleccionar Add project from disk.
3. Elegir la carpeta raiz de este repositorio.
4. Abrir `Assets/Scenes/SampleScene.unity`.
5. Presionar Play.

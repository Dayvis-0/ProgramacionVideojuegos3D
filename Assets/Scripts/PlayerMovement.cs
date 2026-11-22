using UnityEngine;

/// <summary>
/// Movimiento del jugador en el suelo.
/// Lee la entrada, integra la gravedad, mueve un CharacterController y
/// alimenta los parámetros de locomoción del Animator.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // Aceleración hacia abajo en m/s^2. Es negativa porque en Unity el eje +Y
    // apunta hacia arriba.
    public float gravedad = -9.81f;

    // Unidades por segundo caminando.
    public float velocidadCaminar = 3f;

    // Unidades por segundo corriendo (Shift izquierdo mantenido).
    public float velocidadCorrer = 6f;

    // Tecla que dispara el golpe. Se puede cambiar desde el Inspector sin
    // tocar el código.
    public KeyCode teclaGolpe = KeyCode.F;

    // Velocidad vertical acumulada. Se integra cada frame para producir la gravedad.
    private float velocidadY = 0f;

    // Entrada cruda en ambos ejes: x = horizontal (A/D), y = vertical (W/S).
    private Vector2 entradaMovimiento;

    // Estado cacheado. Siempre se calcula ANTES de leerse, así nunca queda un
    // frame atrasado.
    private bool seEstaMoviendo;
    private bool estaCorriendo;

    // Referencias a componentes, resueltas una sola vez en Start.
    private CharacterController controladorPersonaje;
    private Animator animador;

    private void Start()
    {
        // Cacheamos los componentes una sola vez en vez de llamar GetComponent
        // en cada frame. Si a este GameObject le falta alguno, el primer Update
        // va a lanzar una NullReferenceException: agregá ambos en el Inspector.
        controladorPersonaje = GetComponent<CharacterController>();
        animador = GetComponent<Animator>();
    }

    private void Update()
    {
        // 1. Leemos la entrada una sola vez por frame.
        //    GetAxisRaw evita el suavizado que aplica GetAxis, así los umbrales
        //    de animación responden al instante cuando apretás una tecla.
        entradaMovimiento = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        // Disparo del golpe: GetKeyDown detecta la pulsación una sola vez.
        // Con GetKey se dispararía en cada frame mientras esté apretada.
        if (Input.GetKeyDown(teclaGolpe))
        {
            animador.SetTrigger("Punch");
        }

        // 2. Derivamos el estado a partir de la entrada antes de usarlo.
        //    magnitude vale 0 quieto, 1 en un solo eje y ~1.41 en diagonal.
        seEstaMoviendo = entradaMovimiento.magnitude > 0.1f;
        estaCorriendo = Input.GetKey(KeyCode.LeftShift);

        // 3. Elegimos la velocidad efectiva de este frame.
        float velocidadActual = (estaCorriendo && seEstaMoviendo)
            ? velocidadCorrer
            : velocidadCaminar;

        // Si el Animator está reproduciendo el golpe, anulamos la velocidad
        // horizontal para que el personaje no patine mientras pega.
        // La componente Y la sigue manejando la gravedad.
        if (EstaGolpeando())
        {
            velocidadActual = 0f;
        }

        // 4. Movimiento vertical: nos pegamos al suelo y después integramos la gravedad.
        if (controladorPersonaje.isGrounded && velocidadY < 0f)
        {
            // Un valor negativo chico mantiene isGrounded en true sobre
            // pendientes y escaleras. Dejarlo en 0 haría que el controlador
            // "flote" durante un frame.
            velocidadY = -2f;
        }

        // Integración de Euler: cambio de velocidad = aceleración * tiempo.
        // Time.deltaTime hace que la caída no dependa de los FPS.
        velocidadY += gravedad * Time.deltaTime;

        // 5. Desplazamiento horizontal de este frame (metros, no velocidad).
        //    ClampMagnitude mantiene una diagonal en longitud 1 en vez de ~1.41.
        Vector3 direccion = Vector3.ClampMagnitude(
            new Vector3(entradaMovimiento.x, 0f, entradaMovimiento.y), 1f);

        Vector3 movimiento = direccion * velocidadActual * Time.deltaTime;

        // Sobrescribimos Y con el desplazamiento que produjo la gravedad este frame.
        movimiento.y = velocidadY * Time.deltaTime;

        // Move() resuelve colisiones y actualiza isGrounded para el próximo frame.
        controladorPersonaje.Move(movimiento);

        // 6. Empujamos el estado resultante al Animator.
        ActualizarAnimaciones();
    }

    /// <summary>
    /// Devuelve true mientras la capa base del Animator está reproduciendo
    /// el estado Punching.
    /// </summary>
    private bool EstaGolpeando()
    {
        // La animación es la única fuente de verdad de "estoy golpeando".
        // Un bool propio en el script habría que apagarlo a mano cuando
        // termina el clip, y esa es la fuente típica de bugs de estado trabado.
        return animador.GetCurrentAnimatorStateInfo(0).IsName("Punching");
    }

    /// <summary>
    /// Copia el estado actual de movimiento a los parámetros del Animator.
    /// </summary>
    public void ActualizarAnimaciones()
    {
        // Ojo: "seEstaMoviendo" y "estaCorriendo" son los nombres de los
        // parámetros del Animator Controller, NO variables. Deben coincidir
        // exactamente con los que creaste en el Animator.
        animador.SetBool("seEstaMoviendo", seEstaMoviendo);

        // Correr solo tiene sentido si el jugador realmente se está moviendo.
        animador.SetBool("estaCorriendo", estaCorriendo && seEstaMoviendo);
    }
}

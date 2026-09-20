using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float velocidad = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movimiento;

    private string animacionActual = "WalkDown";
    private bool estabaMoviendose = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Empieza quieto mirando hacia abajo
        if (animator != null)
        {
            animator.Play("WalkDown", 0, 0f);
            animator.Update(0f);
            animator.speed = 0f;
        }
    }

    void Update()
    {
        movimiento = Vector2.zero;

        // WASD + Flechas
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            movimiento.x = -1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            movimiento.x = 1;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            movimiento.y = 1;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            movimiento.y = -1;

        movimiento = movimiento.normalized;

        ActualizarAnimacion();
    }

    void FixedUpdate()
    {
        if (movimiento != Vector2.zero)
        {
            rb.MovePosition(
                rb.position +
                movimiento * velocidad * Time.fixedDeltaTime
            );
        }
    }

    void ActualizarAnimacion()
    {
        if (animator == null)
            return;

        bool estaMoviendose = movimiento != Vector2.zero;

        if (estaMoviendose)
        {
            string nuevaAnimacion = animacionActual;

            // Si hay movimiento horizontal, usa izquierda/derecha
            if (movimiento.x > 0)
                nuevaAnimacion = "WalkRight";
            else if (movimiento.x < 0)
                nuevaAnimacion = "WalkLeft";
            else if (movimiento.y > 0)
                nuevaAnimacion = "WalkUp";
            else if (movimiento.y < 0)
                nuevaAnimacion = "WalkDown";

            // Solo reinicia el clip cuando cambia de dirección
            if (nuevaAnimacion != animacionActual || !estabaMoviendose)
            {
                animacionActual = nuevaAnimacion;

                animator.speed = 1f;
                animator.Play(animacionActual, 0, 0f);
            }
        }
        else
        {
            // Quieto: congelar el personaje mirando
            // hacia la última dirección
            if (estabaMoviendose)
            {
                animator.Play(animacionActual, 0, 0f);
                animator.Update(0f);
            }

            animator.speed = 0f;
        }

        estabaMoviendose = estaMoviendose;
    }
}
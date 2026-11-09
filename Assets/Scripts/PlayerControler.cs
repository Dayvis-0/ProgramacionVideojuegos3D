using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    public float velocidad = 10f;
    public float gravedad = -9.81f;
    private float velocidadY = 0f;
    private CharacterController  characControll;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characControll = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float mh = Input.GetAxis("Horizontal");
        float mv = Input.GetAxis("Vertical");

        if (characControll.isGrounded && velocidadY <0f)
        {
            velocidadY = -2f;
        }

        velocidadY += gravedad * Time.deltaTime;

        // characControll.Move(new Vector3(mh, 0f, mv) * Time.deltaTime * velocidad);
        Vector3 movimiento = new Vector3(mh, 0f, mv) * velocidad * Time.deltaTime;
        movimiento.y = velocidadY * Time.deltaTime;

        characControll.Move(movimiento);
    }
}

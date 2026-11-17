using UnityEngine;

public class CursorLock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // centra y bloquea el mouse en el Game
        Cursor.visible = false;                   // oculta el cursor
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None; // libera el mouse
            Cursor.visible = true;
        }
    }
}

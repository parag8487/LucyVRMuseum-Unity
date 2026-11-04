using UnityEngine;

// First-person player: WASD / arrow keys to walk, drag mouse to look, click an exhibit to read about it.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float lookSensitivity = 0.25f;

    CharacterController controller;
    Camera cam;
    float yaw, pitch;
    Vector3 lastMouse, downMouse;
    bool dragging;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        // ----- Look (drag with mouse, or arrow left/right) -----
        if (Input.GetMouseButtonDown(0)) { dragging = true; lastMouse = downMouse = Input.mousePosition; }
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if ((Input.mousePosition - downMouse).magnitude < 6f) TrySelect(Input.mousePosition);
        }
        if (dragging)
        {
            Vector3 d = Input.mousePosition - lastMouse;
            lastMouse = Input.mousePosition;
            yaw += d.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - d.y * lookSensitivity, -70f, 70f);
        }
        if (Input.GetKey(KeyCode.LeftArrow)) yaw -= 90f * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) yaw += 90f * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // ----- Move (WASD / up-down arrows) -----
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.z += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.z -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        Vector3 move = transform.TransformDirection(input.normalized) * moveSpeed;
        controller.SimpleMove(move); // SimpleMove also applies gravity

        // Keyboard shortcut: E selects whatever is in the centre of the screen
        if (Input.GetKeyDown(KeyCode.E)) TrySelect(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
    }

    // Raycasting: shoot an invisible ray from the camera through the mouse position
    void TrySelect(Vector3 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 40f))
        {
            Exhibit ex = hit.collider.GetComponentInParent<Exhibit>();
            if (ex != null) ex.Select();
        }
    }
}

using UnityEngine;

// A museum exhibit: rotates slowly and shows its information when selected.
public class Exhibit : MonoBehaviour
{
    public string title;
    [TextArea] public string description;
    public Vector3 spin = new Vector3(0, 20, 0); // degrees per second

    float popTimer;
    Vector3 baseScale;

    void Start() { baseScale = transform.localScale; }

    void Update()
    {
        transform.Rotate(spin * Time.deltaTime, Space.Self);
        if (popTimer > 0)
        {
            popTimer -= Time.deltaTime;
            float s = 1f + 0.15f * Mathf.Sin((0.5f - popTimer) / 0.5f * Mathf.PI);
            transform.localScale = baseScale * s;
            if (popTimer <= 0) transform.localScale = baseScale;
        }
    }

    public void Select()
    {
        popTimer = 0.5f;
        if (MuseumUI.Instance != null) MuseumUI.Instance.Show(title, description);
    }
}

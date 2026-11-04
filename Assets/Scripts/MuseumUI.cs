using UnityEngine;
using UnityEngine.UI;

// Shows the info panel for a selected exhibit and hides it after a few seconds.
public class MuseumUI : MonoBehaviour
{
    public static MuseumUI Instance;
    public GameObject infoPanel;
    public Text infoTitle;
    public Text infoBody;
    float hideAt;

    void Awake()
    {
        Instance = this;
        if (infoPanel) infoPanel.SetActive(false);
    }

    public void Show(string title, string body)
    {
        infoTitle.text = title;
        infoBody.text = body;
        infoPanel.SetActive(true);
        hideAt = Time.time + 12f;
    }

    void Update()
    {
        if (infoPanel && infoPanel.activeSelf && Time.time > hideAt) infoPanel.SetActive(false);
    }
}

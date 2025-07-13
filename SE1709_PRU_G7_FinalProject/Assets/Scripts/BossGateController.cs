using UnityEngine;

public class BossGateController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Gán boss vào đây")]
    public GameObject bossObject;

    [Header("Gán cổng dịch chuyển vào đây")]
    public GameObject gateObject;

    private bool gateShown = false;

    void Start()
    {
        if (gateObject != null)
            gateObject.SetActive(false); // Ẩn cổng lúc đầu
    }

    void Update()
    {
        if (!gateShown && bossObject == null)
        {
            ShowGate();
        }
    }

    void ShowGate()
    {
        if (gateObject != null)
        {
            gateObject.SetActive(true);
            gateShown = true;
        }
    }
}

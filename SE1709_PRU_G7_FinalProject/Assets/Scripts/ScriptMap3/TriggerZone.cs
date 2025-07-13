using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
public class TriggerZone : MonoBehaviour
{
    public CinemachineCamera playerFollowCam;   // Camera theo dõi người chơi
    public CinemachineCamera mapOverviewCam;    // Camera tổng quan bản đồ

    public Transform triggerPoint;
    public Transform player;
    public float triggerDistance = 1f;

    private GateController gate;
    private bool hasTriggered = false;

    public float overviewDuration = 3f; // Thời gian camera tổng quan

    private void Start()
    {
        gate = FindAnyObjectByType<GateController>();
        if (gate == null)
        {
            // Debug.LogError("Không tìm thấy GateController! Hãy kiểm tra xem GameObject cửa có GateController chưa.");
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, triggerPoint.position);
        // Debug.Log("Khoảng cách đến TriggerPoint: " + distance);

        if (!hasTriggered && distance < triggerDistance)
        {
            // Debug.Log("Trigger Activated!");
            hasTriggered = true;
            StartCoroutine(TriggerEvent());
        }
    }

    IEnumerator TriggerEvent()
    {
        // Debug.Log("Chuyển sang camera Overview");
        SwitchCamera(mapOverviewCam, playerFollowCam);

        yield return new WaitForSeconds(overviewDuration); // Chờ một thời gian quan sát cảnh vật

        // Debug.Log("Chuyển lại camera Player");
        SwitchCamera(playerFollowCam, mapOverviewCam);
    }

    void SwitchCamera(CinemachineCamera activateCam, CinemachineCamera deactivateCam)
    {
        if (activateCam != null && deactivateCam != null)
        {
            activateCam.Priority = 10;
            deactivateCam.Priority = 1;
        }
        else
        {
            // Debug.LogError("Lỗi chuyển camera: Kiểm tra xem có bị null không!");
        }
    }
}

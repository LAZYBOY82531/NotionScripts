using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class ShakeDetector : MonoBehaviour
{
    public float shakeThreshold = 1.5f; // Èçµê °¨Áö ¹Î°¨µµ
    public int requiredShakeCount = 3;  // ÇÊ¿äÇÑ Èçµé±â È½¼ö
    public float directionChangeAngle = 120f; // °¢µµ º¯È­ ±âÁØ
    public bool isShaken = false;

    private XRGrabInteractable grab;
    private bool isGrabbed = false;

    private Vector3 lastPosition;
    private Vector3 lastDirection;
    private int shakeCount = 0;
    [SerializeField] ShakerLiquid shakerLiquid;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        lastPosition = transform.position;
        lastDirection = Vector3.zero;
        shakeCount = 0;
        isShaken = false;
    }
    public void CupOpend()
    {
        isShaken = false;
        shakeCount = 0;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed || isShaken || !shakerLiquid.isClosed) return;

        Vector3 currentPos = transform.position;
        Vector3 delta = currentPos - lastPosition;
        float speed = delta.magnitude / Time.deltaTime;

        if (speed > shakeThreshold)
        {
            Vector3 dir = delta.normalized;
            if (lastDirection != Vector3.zero)
            {
                float angle = Vector3.Angle(lastDirection, dir);
                if (angle > directionChangeAngle)
                {
                    shakeCount++;
                    if (shakeCount >= requiredShakeCount)
                    {
                        isShaken = true;
                        if (shakerLiquid != null)
                        {
                            shakerLiquid.MixedLiquid();
                        }
                    }
                }
            }
            lastDirection = dir;
        }

        lastPosition = currentPos;
    }
}

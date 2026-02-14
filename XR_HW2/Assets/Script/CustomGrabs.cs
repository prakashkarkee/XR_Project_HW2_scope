using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class CustomGrabs : MonoBehaviour
{
    [Header("Extra Credit")]
    [Tooltip("If true, doubles the magnitude of the applied rotation delta.")]
    public bool doubleRotation = false;

    [Header("Keyboard Toggle (Play Mode)")]
    public KeyCode toggleKey = KeyCode.R;

    private XRGrabInteractable grab;
    private Rigidbody rb;

    // We track the *attach transform* of each interactor (hand grab point), not the interactor root.
    private readonly List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> interactors = new();
    private readonly List<Transform> attachTransforms = new();
    private readonly List<Vector3> prevPos = new();
    private readonly List<Quaternion> prevRot = new();

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // IMPORTANT: Our script moves the object. XRGrabInteractable must NOT also drive pose.
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.throwOnDetach = false;

        // Allow multiple hands (2-hand grab)
        grab.selectMode = InteractableSelectMode.Multiple;

        // Physics setup: keep stable at start
        rb.useGravity = true;
        rb.isKinematic = true; // prevents falling away before first grab
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            doubleRotation = !doubleRotation;
            Debug.Log($"[CustomGrab] Double Rotation: {(doubleRotation ? "ON" : "OFF")}");
        }

        if (interactors.Count > 0)
        {
            ApplyTwoHandDeltas();
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is not UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
            return;

        Transform attach = interactor.GetAttachTransform(grab);
        if (attach == null)
            attach = (interactor as MonoBehaviour)?.transform;

        // Prevent duplicates
        int existing = interactors.IndexOf(interactor);
        if (existing >= 0) return;

        interactors.Add(interactor);
        attachTransforms.Add(attach);
        prevPos.Add(attach.position);
        prevRot.Add(attach.rotation);

        // While grabbed, we drive transform manually
        rb.isKinematic = true;

        Debug.Log($"[CustomGrab] Grabbed. Hands = {interactors.Count}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is not UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
            return;

        int idx = interactors.IndexOf(interactor);
        if (idx >= 0)
        {
            interactors.RemoveAt(idx);
            attachTransforms.RemoveAt(idx);
            prevPos.RemoveAt(idx);
            prevRot.RemoveAt(idx);
        }

        // If no hands remain, let physics take over (drop naturally)
        if (interactors.Count == 0)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log($"[CustomGrab] Released. Hands = {interactors.Count}");
    }

    private void ApplyTwoHandDeltas()
    {
        // We apply each hand’s delta around its own pivot (hand attach point).
        // This matches the assignment requirement: translation delta + rotation delta around controller origin.

        for (int i = 0; i < attachTransforms.Count; i++)
        {
            Transform hand = attachTransforms[i];
            if (hand == null) continue;

            Vector3 currP = hand.position;
            Quaternion currR = hand.rotation;

            Vector3 dp = currP - prevPos[i];
            Quaternion dq = currR * Quaternion.Inverse(prevRot[i]);

            if (doubleRotation)
                dq = DoubleRotationMagnitude(dq);

            // 1) Apply controller translation
            transform.position += dp;

            // 2) Apply rotation around controller origin (pivot = hand position)
            // Rotate both object position and rotation
            Vector3 r = transform.position - currP;      // vector from hand to object
            Vector3 r2 = dq * r;                         // rotated vector
            transform.position = currP + r2;             // new position after pivot rotation
            transform.rotation = dq * transform.rotation;

            // Update previous
            prevPos[i] = currP;
            prevRot[i] = currR;
        }
    }

    private static Quaternion DoubleRotationMagnitude(Quaternion q)
    {
        q.ToAngleAxis(out float angle, out Vector3 axis);

        if (axis.sqrMagnitude < 1e-10f || Mathf.Approximately(angle, 0f))
            return Quaternion.identity;

        // ToAngleAxis can return angle > 180; keep it stable
        if (angle > 180f) angle -= 360f;

        return Quaternion.AngleAxis(angle * 2f, axis.normalized);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class GrabMe : MonoBehaviour
{
    public bool doubleRotation = false;
    public KeyCode toggleKey = KeyCode.R;

    private XRGrabInteractable grab;
    private Rigidbody rb;

    private readonly List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> interactors = new();
    private readonly List<Transform> attaches = new();
    private readonly List<Vector3> prevPos = new();
    private readonly List<Quaternion> prevRot = new();

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.throwOnDetach = false;
        grab.selectMode = InteractableSelectMode.Multiple;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            doubleRotation = !doubleRotation;
    }

    private void FixedUpdate()
    {
        if (attaches.Count == 0) return;

        Vector3 totalDeltaPos = Vector3.zero;
        Quaternion totalDeltaRot = Quaternion.identity;

        for (int i = 0; i < attaches.Count; i++)
        {
            Transform h = attaches[i];
            if (!h) continue;

            Vector3 dp = h.position - prevPos[i];
            Quaternion dq = h.rotation * Quaternion.Inverse(prevRot[i]);

            totalDeltaPos += dp;
            totalDeltaRot = dq * totalDeltaRot;

            prevPos[i] = h.position;
            prevRot[i] = h.rotation;
        }

        if (attaches.Count > 1)
            totalDeltaPos /= attaches.Count;

        if (doubleRotation)
            totalDeltaRot = DoubleRotationMagnitude(totalDeltaRot);

        Vector3 newPos = rb.position + totalDeltaPos;
        Quaternion newRot = totalDeltaRot * rb.rotation;

        Vector3 pivot = (attaches.Count == 1)
            ? attaches[0].position
            : 0.5f * (attaches[0].position + attaches[1].position);

        Vector3 r = newPos - pivot;
        r = totalDeltaRot * r;
        newPos = pivot + r;

        rb.MovePosition(newPos);
        rb.MoveRotation(newRot);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactorObject is not UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor) return;

        Transform attach = interactor.GetAttachTransform(grab);
        if (attach == null && interactor is MonoBehaviour mb) attach = mb.transform;

        if (interactors.Contains(interactor)) return;

        interactors.Add(interactor);
        attaches.Add(attach);
        prevPos.Add(attach.position);
        prevRot.Add(attach.rotation);

        rb.isKinematic = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (args.interactorObject is not UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor) return;

        int idx = interactors.IndexOf(interactor);
        if (idx >= 0)
        {
            interactors.RemoveAt(idx);
            attaches.RemoveAt(idx);
            prevPos.RemoveAt(idx);
            prevRot.RemoveAt(idx);
        }

        if (interactors.Count == 0)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private static Quaternion DoubleRotationMagnitude(Quaternion q)
    {
        q.ToAngleAxis(out float angle, out Vector3 axis);
        if (axis.sqrMagnitude < 1e-10f || Mathf.Approximately(angle, 0f))
            return Quaternion.identity;

        if (angle > 180f) angle -= 360f;
        return Quaternion.AngleAxis(angle * 2f, axis.normalized);
    }
}

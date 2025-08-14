using UnityEngine;
using System.Collections;

public class ContinuousPatrolThreeWaypointsSheep : MonoBehaviour
{
    [Header("Animator (in-place)")]
    public Animator animator;
    public string stateName = "RunInPlace";
    public string clipName = "";

    [Header("Waypoints (exactly three)")]
    public Transform waypointA;
    public Transform waypointB;
    public Transform waypointC;

    public enum PathMode { Loop, PingPong }
    public PathMode pathMode = PathMode.PingPong;

    [Header("Rhythm & Movement")]
    public float delayBetweenPlays = 1f;
    public float moveDistancePerLoop = 2f;
    public float arrivalThreshold = 0.05f;

    [Header("Facing / Turns")]
    public float turnPause = 0.2f;
    public float turnDuration = 0.15f;

    [Header("Terrain Alignment")]
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    public float raycastHeight = 2f;
    public float raycastDistance = 5f;
    public float slopeAlignSpeed = 8f; // how quickly tilt/pitch follows the slope

    [Header("Misc")]
    public bool playOnStart = true;
    public bool debugLogs = true;

    Coroutine patrolRoutine;
    float baseClipLength = 0f;

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("[Patrol] Animator is not assigned.");
            enabled = false;
            return;
        }

        if (!ValidateWaypoints(out var wps))
        {
            Debug.LogError("[Patrol] Need 3 valid waypoints assigned (A, B, C).");
            enabled = false;
            return;
        }

        animator.applyRootMotion = false;
        baseClipLength = FindClipLength(string.IsNullOrEmpty(clipName) ? stateName : clipName);

        if (playOnStart)
            patrolRoutine = StartCoroutine(PatrolLoop(wps));
    }

    void OnDisable()
    {
        if (patrolRoutine != null) StopCoroutine(patrolRoutine);
    }

    IEnumerator PatrolLoop(Transform[] wps)
    {
        if (debugLogs)
            Debug.Log($"[Patrol] Starting 3-waypoint patrol. Base len: {baseClipLength:F2}s @ t={Time.time:F2}");

        int current = 0;
        int dir = 1;

        while (true)
        {
            int next = NextIndex(current, ref dir, wps.Length);
            yield return StartCoroutine(AdvanceToWaypoint(wps[current].position, wps[next].position));
            // snap to waypoint but keep Y from ground
            Vector3 snap = wps[next].position;
            if (Physics.Raycast(snap + Vector3.up * raycastHeight, Vector3.down, out var finalHit, raycastDistance, groundMask))
                snap.y = finalHit.point.y;
            transform.position = snap;
            current = next;
        }
    }

    IEnumerator AdvanceToWaypoint(Vector3 from, Vector3 to)
    {
        while (true)
        {
            Vector3 delta = to - transform.position;
            delta.y = 0f;
            float remaining = delta.magnitude;
            if (remaining <= arrivalThreshold) yield break;

            Vector3 stepDir = delta.normalized;

            if (turnPause > 0f) yield return new WaitForSeconds(turnPause);
            yield return StartCoroutine(FaceDirection(stepDir));

            if (delayBetweenPlays > 0f)
            {
                if (debugLogs) Debug.Log($"[Patrol] Delay {delayBetweenPlays:F2}s before step. t={Time.time:F02}");
                yield return new WaitForSeconds(delayBetweenPlays);
            }

            animator.Play(stateName, 0, 0f);
            yield return null;

            var info = animator.GetCurrentAnimatorStateInfo(0);
            float duration = info.length > 0f
                ? info.length
                : Mathf.Max(0.01f, baseClipLength / Mathf.Max(0.01f, animator.speed));

            float stepDist = Mathf.Min(moveDistancePerLoop, remaining);
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + stepDir * stepDist;

            if (debugLogs)
                Debug.Log($"[Patrol] Step for ~{duration:F2}s, dist {stepDist:F2}. End≈{Time.time + duration:F2}");

            // KEY: preserve the current yaw (horizontal facing). We'll only change pitch/roll to match slope.
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).magnitude > 0.001f
                ? new Vector3(transform.forward.x, 0f, transform.forward.z).normalized
                : new Vector3(stepDir.x, 0f, stepDir.z).normalized;

            float t = 0f;
            while (t < duration)
            {
                float k = Mathf.Clamp01(t / duration);
                Vector3 flatPos = Vector3.Lerp(startPos, targetPos, k);
                Vector3 posToSet = flatPos;

                // Raycast to ground to get Y and normal
                if (Physics.Raycast(flatPos + Vector3.up * raycastHeight, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
                {
                    posToSet.y = hit.point.y;

                    // Compute a rotation that preserves yaw (flatForward) but aligns up to the ground normal
                    Quaternion targetSlope = GetSlopeAlignedRotation(flatForward, hit.normal);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetSlope, Time.deltaTime * slopeAlignSpeed);
                }

                transform.position = posToSet;
                t += Time.deltaTime;
                yield return null;
            }

            // Final position: ensure Y is adjusted to terrain to avoid snapping
            Vector3 finalPos = targetPos;
            if (Physics.Raycast(finalPos + Vector3.up * raycastHeight, Vector3.down, out var finalHit, raycastDistance, groundMask))
            {
                finalPos.y = finalHit.point.y;
                Quaternion finalSlope = GetSlopeAlignedRotation(flatForward, finalHit.normal);
                transform.rotation = finalSlope; // final exact alignment
            }
            transform.position = finalPos;
        }
    }

    // face only changes yaw (horizontal), keeps up = world up for the yaw rotation
    IEnumerator FaceDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 1e-5f) yield break;

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude < 1e-5f) yield break;

        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(flatDir.normalized, Vector3.up)
                * Quaternion.Euler(0f, -90f, 0f); // -90 if +X is forward


        if (turnDuration <= 0f)
        {
            transform.rotation = to;
            yield break;
        }

        float t = 0f;
        while (t < turnDuration)
        {
            float k = Mathf.Clamp01(t / turnDuration);
            transform.rotation = Quaternion.Slerp(from, to, k);
            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = to;
    }

    // Build a rotation that preserves horizontal facing but uses 'normal' as up vector.
    Quaternion GetSlopeAlignedRotation(Vector3 flatForward, Vector3 normal)
    {
        // ensure flatForward is valid
        Vector3 f = flatForward;
        f.y = 0f;
        if (f.sqrMagnitude < 1e-6f) f = Vector3.forward;

        // right axis = perpendicular to normal and forward
        Vector3 right = Vector3.Cross(normal, f).normalized;
        // forward on plane = perpendicular to right and normal
        Vector3 forwardOnPlane = Vector3.Cross(right, normal).normalized;
        if (forwardOnPlane.sqrMagnitude < 1e-6f)
            forwardOnPlane = f;

        return Quaternion.LookRotation(forwardOnPlane, normal)
       * Quaternion.Euler(0f, 0f, 0f); // -90° so +X becomes forward

    }

    int NextIndex(int current, ref int dir, int count)
    {
        if (pathMode == PathMode.Loop)
            return (current + 1) % count;
        else
        {
            int next = current + dir;
            if (next >= count) { dir = -1; next = count - 2; }
            else if (next < 0) { dir = 1; next = 1; }
            return next;
        }
    }

    float FindClipLength(string name)
    {
        if (animator.runtimeAnimatorController == null) return 0f;
        foreach (var c in animator.runtimeAnimatorController.animationClips)
            if (c != null && c.name == name) return c.length;
        return 0f;
    }

    bool ValidateWaypoints(out Transform[] wps)
    {
        wps = new Transform[3] { waypointA, waypointB, waypointC };
        return wps[0] && wps[1] && wps[2];
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!waypointA || !waypointB || !waypointC) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(waypointA.position, 0.2f);
        Gizmos.DrawWireSphere(waypointB.position, 0.2f);
        Gizmos.DrawWireSphere(waypointC.position, 0.2f);

        Gizmos.color = Color.cyan;
        if (pathMode == PathMode.Loop)
        {
            Gizmos.DrawLine(waypointA.position, waypointB.position);
            Gizmos.DrawLine(waypointB.position, waypointC.position);
            Gizmos.DrawLine(waypointC.position, waypointA.position);
        }
        else
        {
            Gizmos.DrawLine(waypointA.position, waypointB.position);
            Gizmos.DrawLine(waypointB.position, waypointC.position);
        }
    }
#endif
}

using UnityEngine;
using System.Collections;

public class ContinuousPatrolThreeWaypoints : MonoBehaviour
{
    [Header("Animator (in-place)")]
    public Animator animator;
    [Tooltip("Animator STATE name to play (not just the clip).")]
    public string stateName = "RunInPlace";
    [Tooltip("Optional: clip name to look up a base length. If empty, uses stateName.")]
    public string clipName = "";

    [Header("Waypoints (exactly three)")]
    public Transform waypointA;
    public Transform waypointB;
    public Transform waypointC;

    public enum PathMode { Loop, PingPong }
    [Tooltip("Loop: A→B→C→A…  PingPong: A→B→C→B→A…")]
    public PathMode pathMode = PathMode.PingPong;

    [Header("Rhythm & Movement")]
    [Tooltip("Wait before each plays/move step.")]
    public float delayBetweenPlays = 1f;
    [Tooltip("How far to advance toward the current waypoint per play.")]
    public float moveDistancePerLoop = 2f;
    [Tooltip("Considered arrived when within this distance of the waypoint.")]
    public float arrivalThreshold = 0.05f;

    [Header("Facing / Turns")]
    [Tooltip("Pause right before a facing/turn (cosmetic).")]
    public float turnPause = 0.2f;
    [Tooltip("Smooth face duration before each step. 0 = instant snap.")]
    public float turnDuration = 0.15f;

    [Header("Ground Alignment")]
    public LayerMask groundMask = -1;
    [Tooltip("Height above ground to keep the character.")]
    public float heightOffset = 0.0f;
    [Tooltip("Max raycast distance to find ground.")]
    public float raycastDistance = 5f;

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

    void Update()
    {
        AlignToGround();
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

            transform.position = wps[next].position;
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
                if (debugLogs) Debug.Log($"[Patrol] Delay {delayBetweenPlays:F2}s before step. t={Time.time:F2}");
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

            float t = 0f;
            while (t < duration)
            {
                float k = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(startPos, targetPos, k);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
        }
    }

    IEnumerator FaceDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 1e-5f) yield break;

        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(dir, Vector3.up);

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

    void AlignToGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundMask))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + heightOffset;
            transform.position = pos;
        }
    }

    int NextIndex(int current, ref int dir, int count)
    {
        if (pathMode == PathMode.Loop)
        {
            return (current + 1) % count;
        }
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

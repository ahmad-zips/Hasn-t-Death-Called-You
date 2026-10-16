using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShadowScript : MonoBehaviour
{
    // Hook these from timer / hold-interact scripts
    public static event System.Action<float> OnPlayerTouched;
    public static bool AnyTouching;

    [Header("Detection")]
    public float detectRange = 18f;
    public float loseRange = 30f;
    public float loseDelay = 4f;       // seconds out of range before giving up
    public bool needLineOfSight = false;
    public LayerMask sightBlockers;    // terrain/walls, only used if needLineOfSight

    [Header("Wander")]
    public float wanderRadius = 25f;
    public Vector2 pauseTime = new Vector2(1f, 4f);
    public float wanderSpeed = 1.5f;

    [Header("Chase / Attack")]
    public float chaseSpeed = 3.8f;
    public float attackRange = 1.7f;
    public float attackCooldown = 2f;
    public float timePenalty = 3f;
    public float knockback = 6f;

    [Header("Audio")]
    public AudioClip spotSound;
    public AudioClip attackSound;

    enum State { Wander, Chase, Attack, Stunned }
    State state = State.Wander;

    NavMeshAgent agent;
    AudioSource audioSrc;
    Transform player;
    Vector3 home;
    float nextAttack, stunEnd, pauseUntil, lostTimer;
    bool waiting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSrc = GetComponent<AudioSource>();
        home = transform.position;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        agent.speed = wanderSpeed;
        PickWanderPoint();
    }

    void Update()
    {
        if (!player) return;
        float d = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Wander: DoWander(d); break;
            case State.Chase: DoChase(d); break;
            case State.Attack: DoAttack(d); break;
            case State.Stunned:
                agent.isStopped = true;
                if (Time.time >= stunEnd) { agent.isStopped = false; state = State.Chase; }
                break;
        }
    }

    void LateUpdate() { AnyTouching = false; }

    // ---------- STATES ----------
    void DoWander(float d)
    {
        agent.speed = wanderSpeed;

        if (d <= detectRange && CanSee()) { StartChase(); return; }

        if (waiting)
        {
            if (Time.time >= pauseUntil) { waiting = false; PickWanderPoint(); }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            waiting = true;
            pauseUntil = Time.time + Random.Range(pauseTime.x, pauseTime.y);
        }
    }

    void DoChase(float d)
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (d <= attackRange) { state = State.Attack; agent.isStopped = true; return; }

        if (d > loseRange) lostTimer += Time.deltaTime; else lostTimer = 0f;
        if (lostTimer >= loseDelay) { state = State.Wander; PickWanderPoint(); }
    }

    void DoAttack(float d)
    {
        AnyTouching = true;
        agent.isStopped = true;
        Face(player.position);

        if (Time.time >= nextAttack) Strike();

        if (d > attackRange + 0.6f) { agent.isStopped = false; state = State.Chase; }
    }

    // ---------- ACTIONS ----------
    void StartChase()
    {
        state = State.Chase;
        lostTimer = 0f;
        Play(spotSound);
    }

    void Strike()
    {
        nextAttack = Time.time + attackCooldown;
        Play(attackSound);

        var kb = player.GetComponent<ShadowKnockback>();
        if (kb) kb.Push(player.position - transform.position, knockback);

        OnPlayerTouched?.Invoke(timePenalty);
    }

    public void Stun(float seconds)
    {
        state = State.Stunned;
        stunEnd = Time.time + seconds;
        agent.isStopped = true;
    }

    // ---------- HELPERS ----------
    void PickWanderPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 rnd = home + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    bool CanSee()
    {
        if (!needLineOfSight) return true;
        Vector3 from = transform.position + Vector3.up * 1.2f;
        Vector3 to = player.position + Vector3.up * 1f;
        return !Physics.Linecast(from, to, sightBlockers);
    }

    void Face(Vector3 target)
    {
        Vector3 dir = target - transform.position; dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    void Play(AudioClip c) { if (audioSrc && c) audioSrc.PlayOneShot(c); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(Application.isPlaying ? home : transform.position, wanderRadius);
    }
}
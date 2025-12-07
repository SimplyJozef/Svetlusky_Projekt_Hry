using System.Collections;
using System.Collections.Generic;
using States;
using UnityEngine;
using UnityEngine.AI;

public enum EPatrolStyle
{
    PingPong,
    FromStart
}

public class MonsterAIController : MonoBehaviour
{
    [Header("Vision")]
    public float VisionRadius = 5;
    
    [Range(0, 360)]
    public float VisionAngle = 90;

    [SerializeField]
    private LayerMask _targetMask;
    
    [SerializeField]
    private LayerMask _obstructionMask;

    public float AttackDistance = 1;
    
    [Header("Patrol")]
    public List<Transform> PatrolPoints;
    public float IdleWaitTime = 1;
    public float AcceptanceRadius = 1.0f;
    
    [HideInInspector]
    public NavMeshAgent Agent;
    [HideInInspector]
    public EPatrolStyle PatrolStyle = EPatrolStyle.PingPong;
    [HideInInspector]
    public bool bCanSeeTarget;
    [HideInInspector]
    public Transform Target;
    
    private PatrolState _patrolState = new PatrolState();
    private IState _currentState;
    public IdleState IdleState = new();
    public PatrolState PatrolState = new();
    public ChaseState ChaseState = new();

    public Animator Animator;
    
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        StartCoroutine(FovRoutine());
        ChangeState(_patrolState);
    }

    private void Update()
    {
        if (_currentState is not null)
        {
            _currentState.TickState(this);
        }
    }

    public void ChangeState(IState newState)
    {
        if (_currentState is not null)
        {
            _currentState.OnExit(this);
        }
        _currentState = newState;
        _currentState.OnEnter(this);
    }
    
    private IEnumerator FovRoutine()
    {
        var wait = new WaitForSeconds(.2f);
        
        while (true)
        {
            yield return wait;
            FovCheck();
        }
    }
    
    private void FovCheck()
    {
        var rngChecks = Physics.OverlapSphere(transform.position, VisionRadius, _targetMask);

        if (rngChecks.Length != 0)
        {
            var target = rngChecks[0].transform;
            var dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < VisionAngle / 2)
            {
                var distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, _obstructionMask))
                {
                    bCanSeeTarget = true;
                    Target = target;
                }
                else
                {
                    bCanSeeTarget = false;
                    Target = null;
                }
            }
            else
            {
                bCanSeeTarget = false;
                Target = null;
            }
        }
        else if (bCanSeeTarget)
        {
            bCanSeeTarget = false;
            Target = null;
        }
    }

    // ============================================
    // DAMAGE SYSTEM
    // ============================================

public int damagePerSecond = 30;   // koľko damage za 1 sekundu

private void OnTriggerStay(Collider other)
{
    if (other.CompareTag("Player"))
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // damage sa počíta podľa času medzi frame-mi
            int dmg = Mathf.RoundToInt(damagePerSecond * Time.deltaTime);

            if (dmg > 0)   // aby neposielal 0 dmg
            {
                Debug.Log("Damaging player for: " + dmg);
                health.TakeDamage(dmg);
            }
        }
        else
        {
            Debug.LogError("PlayerHealth NOT FOUND on: " + other.name);
        }
    }
}



}

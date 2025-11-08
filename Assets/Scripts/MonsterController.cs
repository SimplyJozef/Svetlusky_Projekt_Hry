using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum EEnemyState
{
    Idle,
    Patrolling,
    ChasingPlayer
}

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private float _attackDistance;

    public float VisionRadius = 5;
    
    [Range(0, 360)]
    public float VisionAngle = 90;

    [SerializeField]
    private LayerMask _targetMask;
    [SerializeField]
    private LayerMask _obstructionMask;

    public bool bCanSeeTarget { get; private set; }
    public Transform Target { get; private set; }

    
    private NavMeshAgent _agent;
    private float _distance;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(FovRoutine());
    }

    private void Update()
    {
        if (!bCanSeeTarget)
        {
            return;
        }
        _distance = Vector3.Distance(_agent.transform.position, Target.position);
        if (_distance < _attackDistance)
        {
            _agent.isStopped = true;
        }
        else
        {
            _agent.isStopped = false;
            _agent.destination = Target.position;
        }
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EEnemyState
{
    Idle,
    Patrolling,
    ChasingPlayer
}

public enum EPatrolStyle
{
    Reverse,
    FromStart
}

// TODO This is mess, should be refactored
public class MonsterController : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField]
    private List<Transform> _patrolPoints;

    [SerializeField]
    private float _patrolWaitTime;

    [SerializeField]
    private float _acceptanceRadius = 1.0f;

    [SerializeField]
    private EPatrolStyle _patrolStyle = EPatrolStyle.FromStart;

    [Header("Vision")]
    public float VisionRadius = 5;
    
    [Range(0, 360)]
    public float VisionAngle = 90;

    [SerializeField]
    private LayerMask _targetMask;
    
    [SerializeField]
    private LayerMask _obstructionMask;

    [SerializeField]
    private float _attackDistance;

    
    public bool bCanSeeTarget { get; private set; }
    public Transform Target { get; private set; }
    
    private NavMeshAgent _agent;
    private float _distance;
    private int _currentPatrolPoint;
    private EEnemyState _state;

    private Coroutine _waitIdleCoroutine;

    private bool _reversePatrolMoveFwd = true;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(FovRoutine());
        _currentPatrolPoint = 0;
        if (_patrolPoints.Count > 0)
        {
            _agent.destination = _patrolPoints[0].position;
            _state = EEnemyState.Patrolling;
        }
    }

    private void Update()
    {
        if (_state == EEnemyState.ChasingPlayer)
        {
            ChasePlayer();
        }
        if (_state == EEnemyState.Idle)
        {
            if (_waitIdleCoroutine is null)
            {
                _waitIdleCoroutine = StartCoroutine(WaitIdle());
            }
            return;
        }
        if (_state == EEnemyState.Patrolling)
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // Agent has path computed
        if (!_agent.pathPending)
        {
            // Is agent close enough to the target
            if (_agent.remainingDistance <= _acceptanceRadius)
            {
                // Agent doesn't have a path or has stopped
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude <= 0.1f)
                {
                    switch (_patrolStyle)
                    {
                        case EPatrolStyle.Reverse:
                            if (_currentPatrolPoint == 0)
                            {
                                _reversePatrolMoveFwd = true;
                            }
                            else if (_currentPatrolPoint == _patrolPoints.Count - 1)
                            {
                                _reversePatrolMoveFwd = false;
                            }

                            _currentPatrolPoint += _reversePatrolMoveFwd ? 1 : -1;
                            break;
                        case EPatrolStyle.FromStart:
                            _currentPatrolPoint = (_currentPatrolPoint + 1) % _patrolPoints.Count;
                            break;
                    }

                    _state = EEnemyState.Idle;
                }
            }
        }
    }

    private void ChasePlayer()
    {
        // If was in idle, stop the coroutine
        if (_waitIdleCoroutine is not null)
        {
            StopCoroutine(_waitIdleCoroutine);
            _waitIdleCoroutine = null;
        }
        // If can't see the player, go the idle state
        if (!bCanSeeTarget)
        {
            _state = EEnemyState.Idle;
            _agent.isStopped = false;
            return;
        }
        // Check if is in attack distance, if yes stop (TODO add attack or something)
        _distance = Vector3.Distance(_agent.transform.position, Target.position);
        if (_distance < _attackDistance)
        {
            _agent.isStopped = true;
        }
        // Otherwise set players new position as destination
        else
        {
            _agent.isStopped = false;
            _agent.destination = Target.position;
        }
    }

    IEnumerator WaitIdle()
    {
        yield return new WaitForSeconds(_patrolWaitTime);
        _state = EEnemyState.Patrolling;
        _agent.destination = _patrolPoints[_currentPatrolPoint].position;
        _waitIdleCoroutine = null;
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
                    _state = EEnemyState.ChasingPlayer;
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

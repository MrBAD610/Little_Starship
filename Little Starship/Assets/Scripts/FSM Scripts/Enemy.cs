using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;
using UnityEngine.AI;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    private StateMachine<EnemyState, StateEvent> EnemyFSM;
    private Animator Animator;
    private NavMeshAgent Agent;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        EnemyFSM = new StateMachine<EnemyState, StateEvent>();

        EnemyFSM.AddState(EnemyState.Idle, new IdleState(false, this));
        EnemyFSM.AddState(EnemyState.Chase, new ChaseState(true, this));
        EnemyFSM.AddState(EnemyState.Attack, new AttackState(true, this));

        EnemyFSM.SetStartState(EnemyState.Idle);

        EnemyFSM.Init();
    }

    private void Update()
    {
        EnemyFSM.OnLogic();
    }
}

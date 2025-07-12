using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerController Player;

    [Header("Attack Parameters")]
    [SerializeField]
    private float AttackCooldown = 2;

    [Header("Sensors")]
    [SerializeField]
    private PlayerSensor FollowPlayerSensor;
    [SerializeField]
    private PlayerSensor AttackPlayerSensor;

    [Space]
    [Header("Debug Info")]
    [SerializeField]
    private bool IsInChasingRange;
    [SerializeField]
    private bool IsInAttackRange;
    [SerializeField]
    private float LastAttackTime;

    private StateMachine<EnemyState, StateEvent> EnemyFSM;
    private Animator Animator;
    private NavMeshAgent Agent;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        EnemyFSM = new StateMachine<EnemyState, StateEvent>();

        // Add States
        EnemyFSM.AddState(EnemyState.Idle, new IdleState(false, this));
        EnemyFSM.AddState(EnemyState.Chase, new ChaseState(true, this, Player.transform));
        EnemyFSM.AddState(EnemyState.Attack, new AttackState(true, this, OnAttack));
        //EnemyFSM.SetStartState(EnemyState.Idle);

        // Chase Transitions
        EnemyFSM.AddTriggerTransition(StateEvent.DetectPlayer, new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase));
        EnemyFSM.AddTriggerTransition(StateEvent.LostPlayer, new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle));
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase, (transition) => IsInChasingRange && Vector3.Distance(Player.transform.position, transform.position) > Agent.stoppingDistance));
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle, (transition) => !IsInChasingRange || Vector3.Distance(Player.transform.position, transform.position) <= Agent.stoppingDistance));

        // Attack Transitions
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Attack, ShouldAttack, forceInstantly: true));
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Attack, ShouldAttack, forceInstantly: true));
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Attack, EnemyState.Chase, IsNotWithinIdleRange));
        EnemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Attack, EnemyState.Idle, IsWithinIdleRange));

        EnemyFSM.Init();
    }

    private void Start()
    {
        FollowPlayerSensor.OnPlayerEnter += FollowPlayerSensor_OnPlayerEnter;
        FollowPlayerSensor.OnPlayerExit += FollowPlayerSensor_OnPlayerExit;

        AttackPlayerSensor.OnPlayerEnter += AttackPlayerSensor_OnPlayerEnter;
        AttackPlayerSensor.OnPlayerExit += AttackPlayerSensor_OnPlayerExit;
    }

    private void FollowPlayerSensor_OnPlayerExit(Vector3 lastKnownPosition) => IsInChasingRange = false;
    private void FollowPlayerSensor_OnPlayerEnter(Transform player) => IsInChasingRange = true;

    private bool ShouldAttack(Transition<EnemyState> Transition) =>
        LastAttackTime + AttackCooldown <= Time.time
        && IsInAttackRange;
    
    private bool IsWithinIdleRange(Transition<EnemyState> Transition) =>
        Agent.remainingDistance <= Agent.stoppingDistance;
    
    private bool IsNotWithinIdleRange(Transition<EnemyState> Transition) =>
        !IsWithinIdleRange(Transition);

    private void AttackPlayerSensor_OnPlayerExit(Vector3 lastKnownPosition) => IsInAttackRange = false;
    private void AttackPlayerSensor_OnPlayerEnter(Transform player) => IsInAttackRange = true;
    
    private void OnAttack(State<EnemyState, StateEvent> State) 
    {
        transform.LookAt(Player.transform.position);
        LastAttackTime = Time.time;
    }

    private void Update()
    {
        EnemyFSM.OnLogic();
    }
}

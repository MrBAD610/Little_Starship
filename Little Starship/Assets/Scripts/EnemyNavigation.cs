using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    private GameObject player;
    private Transform playerLocation;
    private NavMeshAgent agent;
    private Transform agentLocation;

    private EnemyReferences enemyReferences;

    [Header("Stats")]
    [SerializeField] public float pathUpdateDelay = 0.2f;
    [SerializeField] public float detectionRadius = 5f;

    private float pathUpdateDeadline;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool hasAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        enemyReferences = GameObject.Find("Enemy References").GetComponent<EnemyReferences>();
        player = enemyReferences.playerReference;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        playerLocation = enemyReferences.playerLocationReference;
        agentLocation = transform;
    }

    void Update()
    {
        bool canDetect = Vector3.Distance(agentLocation.position, playerLocation.position) <= detectionRadius;

        if (canDetect)
        {
            UpdatePath();
        }
    }

    private void UpdatePath()
    {
        if (Time.time >= pathUpdateDeadline)
        {
            Debug.Log("Updating Path");
            pathUpdateDeadline = Time.time + pathUpdateDelay;
            agent.SetDestination(playerLocation.position);
        }
    }
}

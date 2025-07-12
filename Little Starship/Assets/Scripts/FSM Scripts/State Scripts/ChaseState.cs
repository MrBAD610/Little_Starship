using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyStateBase
{
    private Transform Target;

    public ChaseState(bool needsExitTime, Enemy Enemy, Transform Target) : base(needsExitTime, Enemy)
    {
        this.Target = Target;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Agent.enabled = true;
        Agent.isStopped = false;
        Animator.Play("Walk");
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if (!RequestedExit)
        {
            // can add more complex movement prediction here like in https://www.youtube.com/watch?v=1Jkg8cKLsC0&list=PLllNmP7eq6TSkwDN8OO0E8S6CWybSE_xC&index=46
            Agent.SetDestination(Target.position);
        }
        else if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            fsm.StateCanExit(); // In case that we were requested to exit, we will continue moving to the last known position proir to transitioning out to idle
        }
    }
}

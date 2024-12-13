using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : EnemyStateBase
{
    public IdleState(bool needsExitTime, Enemy Enemy) : base(needsExitTime, Enemy)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Agent.isStopped = true;
        Animator.Play("Idle");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityHFSM;
using System;
using UnityEngine;

public class AttackState : EnemyStateBase
{
    public AttackState(
        bool needsExitTime,
        Enemy Enemy,
        Action<State<EnemyState, StateEvent>> onEnter,
        float ExitTime = 0.33f) : base(needsExitTime, Enemy, ExitTime, onEnter) {}

    public override void OnEnter()
    {
        Agent.isStopped = true;
        base.OnEnter();
        Animator.Play("Attack");
    }
}

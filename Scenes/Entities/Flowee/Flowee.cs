using Godot;
using System;

public partial class Flowee : Entity
{

	public override void _PhysicsProcess(double delta)
	{
		
	}

	public override void AttackArea_Body(Node3D body)
    {
        if(canAttack)	
        {
            canAttack = false;
            isAttacking = true;
			animationTree.Set("parameters/Transition/transition_request", "Attack");
            animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }

	void SpawnBullets()
    {            
        canAttack = true;
    }

    public override void PlayHurt()
    {
        animationTree.Set("parameters/Transition/transition_request", "Hurt");
		animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
}

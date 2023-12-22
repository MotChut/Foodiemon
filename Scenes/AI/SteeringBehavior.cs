using Godot;

public partial class SteeringBehavior : Node
{
	Entity entity;

    public SteeringBehavior(Entity entity)
    {
        this.entity = entity;
    }

    public Vector3 Seek()
    {
        return entity.GlobalPosition.DirectionTo(entity.targetPos);
    }

    public Vector3 Wander()
    {
        return entity.pack.ChooseWanderDirection();
    }

    public Vector3 Evade()
    {
        Vector3 direction = Vector3.Zero;
        float interest;
        for(int i = 0; i < entity.statsSettings.nRaycasts; i++)
        {
            RayCast3D ray = (RayCast3D)entity.raycastsNode.GetChild(i);
            if(ray.IsColliding())
            {
                interest = entity.statsSettings.raycastLength - ray.GlobalPosition.DistanceTo(ray.GetCollisionPoint());
                direction -= ray.GlobalPosition.DirectionTo(ray.GetCollisionPoint()) * interest;
            }
        }

        return direction;
    }
}

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    public enum CollisionLayer
    {
        Ground = 1 << 0,
        Obstacle = 1 << 1,
    }
}
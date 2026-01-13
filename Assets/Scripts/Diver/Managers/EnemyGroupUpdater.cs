using Unity.Collections;
using Unity.Jobs;

public static class EnemyGroupUpdater
{
    public const float WaterSurfaceHeight = 0f;

    public static void UpdateEnemyGroup(FlockGroup group, float deltaTime)
    {
        var enemies = group.Enemies;
        int count = enemies.Length;

        var handle0 = new JobHandle();
        var flockHandle = UpdateFlockGroup(handle0, enemies, count, group, deltaTime);
        var avoidHandle = AvoidPlayer(flockHandle, enemies, count, group, deltaTime);
        var animHandle = UpdateAnimation(avoidHandle, enemies, count, group, deltaTime);

        animHandle.Complete();
    }

    public static JobHandle UpdateFlockGroup(JobHandle handle, NativeList<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var flockJob = new FlockJob
        {
            Enemies = enemies.AsArray(),
            Settings = group.Settings,
            OriginPoint = group.OriginPoint,
            MaxDistanceSq = group.MaxDistanceSq,
            DeltaTime = deltaTime,
            MaxY = WaterSurfaceHeight
        };

        return flockJob.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle AvoidPlayer(JobHandle handle, NativeList<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var flockJob = new AvoidJob
        {
            Enemies = enemies.AsArray(),
            MyPosition = RigControl.Instance.transform.position,
            DeltaTime = deltaTime,
        };

        return flockJob.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle UpdateAnimation(JobHandle handle, NativeList<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var animJob = new AnimationUpdateJob
        {
            Enemies = enemies.AsArray(),
            DeltaTime = deltaTime
        };

        return animJob.ScheduleByRef(count, 64, handle);
    }
}
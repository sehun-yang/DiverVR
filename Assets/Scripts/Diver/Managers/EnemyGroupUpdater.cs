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
        var handle1 = UpdateFlockGroup(handle0, enemies, count, group, deltaTime);
        var handle2 = UpdateAnimation(handle1, enemies, count, group, deltaTime);

        handle2.Complete();
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

        return flockJob.Schedule(count, 32, handle);
    }

    public static JobHandle UpdateAnimation(JobHandle handle, NativeList<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var animJob = new AnimationUpdateJob
        {
            Enemies = enemies.AsArray(),
            DeltaTime = deltaTime
        };

        return animJob.Schedule(count, 64, handle);
    }
}
using Unity.Collections;
using Unity.Jobs;

public static class EnemyGroupUpdater
{
    public const float WaterSurfaceHeight = 0f;

    public static void UpdateEnemyGroup(FlockGroup group, float deltaTime)
    {
        var enemies = group.Enemies;
        int count = enemies.Length;
        var enemiesArray = enemies.AsArray();

        var handle = new JobHandle();
        handle = UpdateFlockGroup(handle, enemiesArray, count, group, deltaTime);
        handle = AvoidPlayer(handle, enemiesArray, count, group, deltaTime);
        handle = Inhale(handle, enemiesArray, count, group, deltaTime);
        handle = UpdateAnimation(handle, enemiesArray, count, group, deltaTime);

        handle.Complete();
    }

    public static JobHandle UpdateFlockGroup(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new FlockJob
        {
            Enemies = enemies,
            Settings = group.Settings,
            OriginPoint = group.OriginPoint,
            MaxDistanceSq = group.MaxDistanceSq,
            DeltaTime = deltaTime,
            MaxY = WaterSurfaceHeight
        };

        return job.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle AvoidPlayer(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new AvoidJob
        {
            Enemies = enemies,
            MyPosition = RigControl.Instance.transform.position,
            DeltaTime = deltaTime,
        };

        return job.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle Inhale(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            var job = new InhaleJob
            {
                Enemies = enemies,
                InhaleOrigin = RelativePositionControl.Instance.MyPlayerControl.RightHandPosition,
                MaxInhaleRange = ModuleManager.Instance.InhaleModule.MaxInhaleRange,
                InhaleStrength = ModuleManager.Instance.InhaleModule.InhaleStrength,
                DeltaTime = deltaTime,
            };

            return job.ScheduleByRef(count, 32, handle);
        }
        else
        {
            return handle;
        }
    }


    public static JobHandle UpdateAnimation(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new AnimationUpdateJob
        {
            Enemies = enemies,
            DeltaTime = deltaTime
        };

        return job.ScheduleByRef(count, 64, handle);
    }
}
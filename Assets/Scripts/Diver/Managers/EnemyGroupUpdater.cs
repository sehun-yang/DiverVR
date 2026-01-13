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

        NativeArray<bool> isDead = default;
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            isDead = new NativeArray<bool>(count, Allocator.TempJob);
            handle = MarkDeadEnemies(handle, enemiesArray, count, isDead);
        }

        handle.Complete();

        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            RemoveDeadEnemies(group, isDead);
            isDead.Dispose();
        }
    }

    private static void RemoveDeadEnemies(FlockGroup group, NativeArray<bool> isDead)
    {
        for (int i = isDead.Length - 1; i >= 0; i--)
        {
            if (isDead[i])
            {
                group.Enemies.RemoveAtSwapBack(i);
            }
        }
    }

    private static JobHandle MarkDeadEnemies(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, NativeArray<bool> isDead)
    {
        var job = new InhaleMarkDeadEnemiesJob
        {
            Enemies = enemies,
            InhaleOrigin = RelativePositionControl.Instance.MyPlayerControl.RightHandPosition,
            CaptureDistanceSq = 0.1f * 0.1f,
            IsDead = isDead
        };

        return job.ScheduleByRef(count, 64, handle);
    }

    private static JobHandle UpdateFlockGroup(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
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

    private static JobHandle AvoidPlayer(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new AvoidJob
        {
            Enemies = enemies,
            MyPosition = RigControl.Instance.transform.position,
            DeltaTime = deltaTime,
        };

        return job.ScheduleByRef(count, 32, handle);
    }

    private static JobHandle Inhale(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            var job = new InhaleJob
            {
                Enemies = enemies,
                InhaleOrigin = RelativePositionControl.Instance.MyPlayerControl.RightHandPosition,
                MaxInhaleRange = ModuleManager.Instance.InhaleModule.MaxInhaleRange,
                InhaleStrength = ModuleManager.Instance.InhaleModule.InhaleStrength,
                ForwardDirection = RelativePositionControl.Instance.MyPlayerControl.RightArmForward,
                ConeAngle = ModuleManager.Instance.InhaleModule.ConeAngle,
                DeltaTime = deltaTime,
            };

            return job.ScheduleByRef(count, 32, handle);
        }
        else
        {
            return handle;
        }
    }

    private static JobHandle UpdateAnimation(JobHandle handle, NativeArray<EnemyInstance> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new AnimationUpdateJob
        {
            Enemies = enemies,
            DeltaTime = deltaTime
        };

        return job.ScheduleByRef(count, 64, handle);
    }
}
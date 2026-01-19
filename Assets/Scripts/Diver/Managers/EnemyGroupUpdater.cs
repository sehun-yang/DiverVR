using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class EnemyGroupUpdater
{
    public const float WaterSurfaceHeight = 0f;

    public static void RemoveDeadEnemies(RenderGroup group, NativeArray<bool> isDead)
    {
        int writeIndex = 0;
        int deadCount = 0;
        
        for (int readIndex = 0; readIndex < group.Count; readIndex++)
        {
            if (isDead[readIndex])
            {
                EnemyManager.Instance.NotifyDead(group.DataContainer.EnemyArcheTypeArray[readIndex].SpawnerId, readIndex);
                deadCount++;
            }
            else
            {
                if (writeIndex != readIndex)
                {
                    group.DataContainer.PullArray(writeIndex, readIndex);
                }
                writeIndex++;
            }
        }
        
        group.Count = writeIndex;
    }

    public static JobHandle MarkDeadEnemies(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, NativeArray<bool> isDead)
    {
        var job = new InhaleMarkDeadEnemiesJob
        {
            Enemies = enemies,
            InhaleOrigin = RelativePositionControl.Instance.MyPlayerControl.RightHandPosition,
            CaptureDistanceSq = 0.2f * 0.2f,
            IsDead = isDead
        };

        return job.ScheduleByRef(count, 64, handle);
    }

    public static JobHandle MarkDeadEnemiesWithHealth(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, NativeArray<bool> isDead)
    {
        var job = new HealthDeadEnemiesJob
        {
            Enemies = enemies,
            IsDead = isDead
        };

        return job.ScheduleByRef(count, 64, handle);
    }

    public static JobHandle UpdateFlockGroup(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, FlockGroup group, float deltaTime)
    {
        var job = new FlockJob
        {
            Enemies = enemies,
            MaxCount = group.Count,
            Settings = group.Settings,
            OriginPoint = group.OriginPoint,
            MaxDistanceSq = group.MaxDistanceSq,
            DeltaTime = deltaTime,
            MaxY = WaterSurfaceHeight
        };

        return job.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle AvoidPlayer(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, float deltaTime)
    {
        var job = new AvoidJob
        {
            Enemies = enemies,
            MyPosition = RigControl.Instance.transform.position,
            DeltaTime = deltaTime,
        };

        return job.ScheduleByRef(count, 32, handle);
    }

    public static JobHandle PhysicsCollisionJob(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, float deltaTime, Vector3 gravity)
    {
        var raycastCommands = new NativeArray<SpherecastCommand>(count, Allocator.TempJob);
        var raycastHits = new NativeArray<RaycastHit>(count, Allocator.TempJob);

        var job = new PhysicsRaycastJob
        {
            Enemies = enemies,
            Commands = raycastCommands,
            DeltaTime = deltaTime
        };

        handle = job.ScheduleByRef(count, 64, handle);
        SpherecastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, handle).Complete();

        var processJob = new PhysicsCollisionMovementJob
        {
            Enemies = enemies,
            Hits = raycastHits,
            DeltaTime = deltaTime,
            Gravity = gravity
        };

        processJob.ScheduleByRef(count, 64).Complete();

        raycastCommands.Dispose();
        raycastHits.Dispose();

        return default;
    }

    public static JobHandle ScaleTo(JobHandle handle, NativeArray<EnemyArcheType> enemies, NativeArray<ScaleArcheType> scales, int count, float deltaTime)
    {
        var job = new ScaleJob
        {
            Enemies = enemies,
            Scales = scales,
            DeltaTime = deltaTime
        };

        return job.ScheduleByRef(count, 128, handle);
    }

    public static JobHandle Inhale(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, float deltaTime, Vector3 gravity)
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
                Gravity = gravity,
                DeltaTime = deltaTime
            };

            return job.ScheduleByRef(count, 32, handle);
        }
        else
        {
            return handle;
        }
    }

    public static JobHandle InhaleDamage(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, float deltaTime, float damage)
    {
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            var job = new InhaleDamageJob
            {
                Enemies = enemies,
                MaxDamage = damage,
                InhaleOrigin = RelativePositionControl.Instance.MyPlayerControl.RightHandPosition,
                MaxInhaleRange = ModuleManager.Instance.InhaleModule.MaxInhaleRange,
                ForwardDirection = RelativePositionControl.Instance.MyPlayerControl.RightArmForward,
                ConeAngle = ModuleManager.Instance.InhaleModule.ConeAngle,
                DeltaTime = deltaTime
            };

            return job.ScheduleByRef(count, 32, handle);
        }
        else
        {
            return handle;
        }
    }

    public static void InhalePostProcess(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, RenderGroup group)
    {
        NativeArray<bool> isDead = default;
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            isDead = new NativeArray<bool>(count, Allocator.TempJob);
            handle = MarkDeadEnemies(handle, enemies, count, isDead);
        }

        handle.Complete();

        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            RemoveDeadEnemies(group, isDead);
            isDead.Dispose();
        }
    }

    public static void InhaleDamagePostProcess(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, RenderGroup group)
    {
        NativeArray<bool> isDead = default;
        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            isDead = new NativeArray<bool>(count, Allocator.TempJob);
            handle = MarkDeadEnemiesWithHealth(handle, enemies, count, isDead);
        }

        handle.Complete();

        if (ModuleManager.Instance.InhaleModule.Enabled)
        {
            RemoveDeadEnemies(group, isDead);
            isDead.Dispose();
        }
    }

    public static JobHandle UpdateAnimation(JobHandle handle, NativeArray<EnemyArcheType> enemies, int count, float deltaTime)
    {
        var job = new AnimationUpdateJob
        {
            Enemies = enemies,
            DeltaTime = deltaTime
        };

        return job.ScheduleByRef(count, 64, handle);
    }
}
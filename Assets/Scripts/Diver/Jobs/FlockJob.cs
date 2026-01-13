using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FlockJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<EnemyInstance> Enemies;
    [ReadOnly] public FlockSettings Settings;
    [ReadOnly] public float3 OriginPoint;
    [ReadOnly] public float MaxDistanceSq;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float MaxY;

    public void Execute(int index)
    {
        var enemy = Enemies[index];
        
        float3 myPos = enemy.Position;
        float3 myVel = enemy.Velocity;

        float3 separation = float3.zero;
        float3 alignment = float3.zero;
        float3 cohesion = float3.zero;
        int neighborCount = 0;

        for (int i = 0; i < Enemies.Length; i++)
        {
            if (i == index) continue;

            var other = Enemies[i];
            float3 otherPos = other.Position;
            float dist = math.distance(myPos, otherPos);

            if (dist < Settings.NeighborDistance)
            {
                neighborCount++;
                alignment += other.Velocity;
                cohesion += otherPos;

                if (dist < Settings.SeparationDistance && dist > 0.001f)
                {
                    separation += (myPos - otherPos) / dist;
                }
            }
        }

        float3 steer = float3.zero;

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            alignment = math.normalizesafe(alignment) * Settings.MaxSpeed;
            steer += (alignment - myVel) * Settings.AlignmentWeight;

            cohesion /= neighborCount;
            float3 cohesionDir = math.normalizesafe(cohesion - myPos);
            steer += cohesionDir * Settings.CohesionWeight;

            steer += separation * Settings.SeparationWeight;
        }

        float3 toCenter = OriginPoint - myPos;
        float distToCenterSq = math.lengthsq(toCenter);
        if (distToCenterSq > MaxDistanceSq)
        {
            steer += math.normalizesafe(toCenter) * Settings.ReturnWeight;
        }

        float3 newVel = myVel + steer * DeltaTime;
        float speed = math.length(newVel);

        if (speed > Settings.MaxSpeed)
        {
            newVel = math.normalizesafe(newVel) * Settings.MaxSpeed;
        }
        else if (speed < Settings.MinSpeed && speed > 0.001f)
        {
            newVel = math.normalizesafe(newVel) * Settings.MinSpeed;
        }

        if (myPos.y > MaxY * 0.99f)
        {
            newVel = new float3(newVel.x, -math.abs(newVel.y), newVel.z);
        }

        enemy.Velocity = newVel;
        enemy.Position += newVel * DeltaTime;

        if (math.lengthsq(newVel) > 0.001f)
        {
            float3 dir = math.normalizesafe(newVel);
            quaternion targetRot = quaternion.LookRotationSafe(dir, math.up());
            enemy.Rotation = math.slerp(enemy.Rotation, targetRot, DeltaTime * Settings.RotationSpeed);
        }

        Enemies[index] = enemy;
    }
}

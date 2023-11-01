using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

namespace CharacterController
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct BloodSystem : ISystem
    {

        //查询bloodController
        EntityQuery bloodControllerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            bloodControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW<BloodComponent>().Build();

            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(bloodControllerQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hpJob=new HpLimitJob
            {
                bloodType =SystemAPI.GetComponentTypeHandle<BloodComponent>()
            };
            state.Dependency = hpJob.ScheduleParallel(bloodControllerQuery, state.Dependency);
        }
        
        
        [BurstCompile]
        struct HpLimitJob : IJobChunk
        {
            //public float deltaTime;
            public ComponentTypeHandle<BloodComponent> bloodType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                //如果是false就断言
                Assert.IsFalse(useEnabledMask);
                
                //获取chunk中的bloodComponent
                var bloods = chunk.GetNativeArray(ref bloodType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var blood = bloods[i];
                    if(blood.hp<=0)
                    {
                        // todo:死亡
                    }
                    blood.hp = math.clamp(blood.hp, 0, blood.maxHp);
                    bloods[i] = blood;
                }
            }
        }
    }
    
}



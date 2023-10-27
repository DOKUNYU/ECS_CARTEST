using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics.Systems;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;


namespace CharacterController
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct ShootSystem : ISystem
    {
        //查询characterController
        EntityQuery characterControllerQuery;
        //获取生成之后的车的实体
        private EntityQuery carQuery;
        
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //查找charactercontrollerInternal
            characterControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW< CharacterControllerInternal>()
                .WithAll<PhysicsCollider>().Build();
            carQuery = SystemAPI.QueryBuilder().WithAllRW< Car>().Build();
            //存在ShootSetting才运行
            state.RequireForUpdate<ShootSetting>();
            //query结束后采运行
            state.RequireForUpdate(characterControllerQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> carEntity = carQuery.ToEntityArray(Allocator.Persistent);
            
            //绑定shoot
            var bindingJob = new ShootBindingJob()
            {
                InternalLookUp=SystemAPI.GetComponentLookup<CharacterControllerInternal>(),
                ShootSettingLookUp=SystemAPI.GetComponentLookup<ShootSetting>(),
                ChildLookUp=SystemAPI.GetBufferLookup<Child>(),
                CarEntities=carEntity
            };
            state.Dependency = bindingJob.Schedule(state.Dependency);

        }

        [BurstCompile]
        struct ShootBindingJob : IJob
        {
            public ComponentLookup<CharacterControllerInternal> InternalLookUp;
            public ComponentLookup<ShootSetting> ShootSettingLookUp;
            public BufferLookup<Child> ChildLookUp;
            public NativeArray<Entity> CarEntities;
            
            public void Execute()
            {
                foreach(var car in CarEntities)
                {
                    if (InternalLookUp[car].ShootInit== true)
                    {
                        continue;
                    }
                    else
                    {
                        var pitch = InternalLookUp[car].Pitch;
                        //获取pitch的child
                        var child = ChildLookUp[pitch];
                        foreach (var target in child)
                        {
                            if (ShootSettingLookUp.HasComponent(target.Value))
                            {
                                var carInternal = InternalLookUp.GetRefRW(car);
                                carInternal.ValueRW.Shoot = target.Value;
                                carInternal.ValueRW.ShootInit = true;
                            }
                        }
                    }
                }
            }
        }
        
        [BurstCompile]
        struct ShootJob : IJob
        {
            public ComponentLookup<LocalTransform> PitchTransformLookUp;
            public ComponentLookup<CharacterControllerInternal> InternalLookUp;
            public ComponentLookup<ShootSetting> ShootSettingLookUp;
            public NativeArray<Entity> CarEntities;

            public void Execute()
            {
                //获取和修改pitch
                foreach (var car in CarEntities)
                {
                    
                }
            }

        }

    }
}

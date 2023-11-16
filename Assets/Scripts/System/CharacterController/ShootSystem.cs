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
using Unity.Physics.Extensions;


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
        private EntityQuery bulletQuery;
        private float time;
        
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //查找charactercontrollerInternal
            characterControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW< CharacterControllerInternal>()
                .WithAll<PhysicsCollider>().Build();
            carQuery = SystemAPI.QueryBuilder().WithAllRW< Car>().Build();
            bulletQuery= SystemAPI.QueryBuilder().WithAllRW< Bullet>().Build();
            //存在ShootSetting才运行
            state.RequireForUpdate<ShootSetting>();
            //query结束后采运行
            state.RequireForUpdate(characterControllerQuery);
            time = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> carEntity = carQuery.ToEntityArray(Allocator.Persistent);
            

            //绑定shoot到角色上
            var bindingJob = new ShootBindingJob()
            {
                InternalLookUp=SystemAPI.GetComponentLookup<CharacterControllerInternal>(),
                ShootSettingLookUp=SystemAPI.GetComponentLookup<ShootSetting>(),
                ChildLookUp=SystemAPI.GetBufferLookup<Child>(),
                CarEntities=carEntity
            };
            JobHandle bindingHandle= bindingJob.Schedule(state.Dependency);
            bindingHandle.Complete();
            
            //获取输入+发射
            //获取internal
            foreach (var car in carEntity)
            {
                ComponentLookup<CharacterControllerInternal> internalLookUp = SystemAPI.GetComponentLookup<CharacterControllerInternal>();
                ComponentLookup<ShootSetting> shootSettingLookUp = SystemAPI.GetComponentLookup<ShootSetting>();
                //R0
                var ccInternal = internalLookUp[car];
                var ccInput = ccInternal.Input;
                if (ccInput.Fire >0.5f && (Time.time-time>0.5f)) //浮点运算问题
                {
                    //获取子弹prefabs
                    var bulletPrefabs = shootSettingLookUp[ccInternal.Shoot].BulletPrefab;
                    //生成 但是用ecb
                    var ecb = new EntityCommandBuffer(Allocator.Temp);
                    var bulletEntity = ecb.Instantiate(bulletPrefabs);
                    ecb.AddComponent<Bullet>(bulletEntity,new Bullet()
                    {
                        Shooter = car,
                        IsInit = false,
                        IsCollision = false
                    });
                    ecb.Playback(state.EntityManager);
                    Debug.Log("完成prefabs生成");

                    time = Time.time;
                }
            }
            
            //查找没有初始化的子弹
            NativeArray<Entity> bullets = bulletQuery.ToEntityArray(Allocator.Persistent);
            foreach (var bullet in bullets)
            {
                var bulletComponentData = SystemAPI.GetComponentRW<Bullet>(bullet);
                if (!bulletComponentData.ValueRO.IsInit)
                {
                    var shootingJob = new ShootJob()
                    {
                        LocalToWorldLookUp=SystemAPI.GetComponentLookup<LocalToWorld>(),
                        LocalTransformLookUp = SystemAPI.GetComponentLookup<LocalTransform>(),
                        InternalLookUp=SystemAPI.GetComponentLookup<CharacterControllerInternal>(),
                        BulletEntity = bullet,
                        Car = bulletComponentData.ValueRW.Shooter,
                    };
                    JobHandle shootJobHandle= shootingJob.Schedule(state.Dependency);
                    shootJobHandle.Complete();
                    bulletComponentData.ValueRW.IsInit = true;
                }
                
                var applyForceJob = new ApplyForceJob()
                {
                    LocalTransformLookUp = SystemAPI.GetComponentLookup<LocalTransform>(),
                    BulletEntity = bullet,
                    VelocityLookUp=SystemAPI.GetComponentLookup<PhysicsVelocity>(),
                    MassLookUp = SystemAPI.GetComponentLookup<PhysicsMass>()
                };
                JobHandle applyForceJobHandle= applyForceJob.Schedule(state.Dependency);
                applyForceJobHandle.Complete();
                
            }
            
            


        }

        /// <summary>
        /// 绑定shoot组件到CharacterControllerInternal
        /// </summary>
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
                        if (pitch != Entity.Null)
                        {
                            //获取pitch的child
                            var child = ChildLookUp[pitch];
                            foreach (var target in child)
                            {
                                if (ShootSettingLookUp.HasComponent(target.Value))
                                {
                                    var carInternal = InternalLookUp[car];
                                    carInternal.Shoot = target.Value;
                                    carInternal.ShootInit = true;
                                    InternalLookUp[car] = carInternal;
                                }
                            }
                        }
                        
                    }
                }
            }
        }
        
        /// <summary>
        /// 改变发射位置
        /// </summary>
        [BurstCompile]
        struct ShootJob : IJob
        {
            public ComponentLookup<LocalToWorld> LocalToWorldLookUp;
            public ComponentLookup<LocalTransform> LocalTransformLookUp;
            public ComponentLookup<CharacterControllerInternal> InternalLookUp;
            public Entity BulletEntity;
            public Entity Car;

            public void Execute()
            {
                //位置设定
                var bulletTransform = LocalTransformLookUp[BulletEntity];
                bulletTransform.Position = LocalToWorldLookUp[InternalLookUp[Car].Shoot].Position;
                bulletTransform.Rotation = LocalToWorldLookUp[InternalLookUp[Car].Shoot].Rotation;
                LocalTransformLookUp[BulletEntity] = bulletTransform;
            }

        }

        /// <summary>
        /// 改变发射位置
        /// </summary>
        [BurstCompile]
        struct ApplyForceJob : IJob
        {
            public ComponentLookup<LocalTransform> LocalTransformLookUp;
            public ComponentLookup<PhysicsVelocity> VelocityLookUp;
            public ComponentLookup<PhysicsMass> MassLookUp;
            public Entity BulletEntity;

            public void Execute()
            {
                //位置指定
                var bulletTransform = LocalTransformLookUp[BulletEntity];
                //施力
                var pv = VelocityLookUp.GetRefRW(BulletEntity);
                var pm = MassLookUp[BulletEntity];

                var impulse = bulletTransform.Forward() * 1f;
                pv.ValueRW.ApplyImpulse(pm, bulletTransform.Position, bulletTransform.Rotation, impulse,
                    bulletTransform.Position);

            }
        }
    }
}

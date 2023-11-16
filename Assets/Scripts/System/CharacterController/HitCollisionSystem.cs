using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Identity;


namespace CharacterController
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct HitCollisionSystem : ISystem
    {
        //查询bloodController
        EntityQuery bloodControllerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            bloodControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW<BloodComponent>().Build();
            
            state.RequireForUpdate<ShootSetting>();
            state.RequireForUpdate(bloodControllerQuery);
            state.RequireForUpdate<Bullet>();
            state.RequireForUpdate<Harm>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new HitEventJob
            {
                harmData = SystemAPI.GetComponentLookup<Harm>(),
                blood = SystemAPI.GetComponentLookup<BloodComponent>(),
                bullets = SystemAPI.GetComponentLookup<Bullet>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }
        
        [BurstCompile]
        struct HitEventJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentLookup<Harm> harmData;
            public ComponentLookup<BloodComponent> blood;
            public ComponentLookup<Bullet> bullets;

            public void Execute(CollisionEvent collisionEvent)
            {
                Entity entityA = collisionEvent.EntityA;
                Entity entityB = collisionEvent.EntityB;

                bool isABullet = harmData.HasComponent(entityA);
                bool isBBullet = harmData.HasComponent(entityB);
                

                //A是子弹
                if (isABullet)
                {
                    var bulletData = bullets[entityA];
                    if (!bulletData.IsCollision)
                    {
                        //扣血
                        var damage = harmData[entityA].injury;
                        var victimBlood = blood[entityB];
                        victimBlood.hp-=damage;
                        blood[entityB] = victimBlood;
                        //状态改变
                        bulletData.IsCollision = true;
                        bullets[entityA] = bulletData;
                        Debug.Log("击中！！ :"+entityB.Index);
                    }
                }
                //B是子弹
                else if(isBBullet)
                {
                    var bulletData = bullets[entityB];
                    if (!bulletData.IsCollision)
                    {
                        //扣血
                        var damage = harmData[entityB].injury;
                        var victimBlood = blood[entityA];
                        victimBlood.hp -= damage;
                        blood[entityA] = victimBlood;
                        //状态改变
                        bulletData.IsCollision = true;
                        bullets[entityB] = bulletData;
                        Debug.Log("击中！！ :"+entityA.Index);
                    }
                }
                //其他情况不是子弹撞击
                
            }
        }
    }
}

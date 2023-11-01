using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;


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
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }
        
        [BurstCompile]
        struct HitEventJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentLookup<Harm> harmData;
            public ComponentLookup<BloodComponent> blood;

            public void Execute(CollisionEvent collisionEvent)
            {
                Entity entityA = collisionEvent.EntityA;
                Entity entityB = collisionEvent.EntityB;

                bool isABullet = harmData.HasComponent(entityA);

                Debug.Log("击中！！");

                //A是子弹
                if (isABullet)
                {
                    var damage = harmData[entityA].injury;
                    var victimBlood = blood[entityB];
                    victimBlood.hp-=damage;
                    blood[entityB] = victimBlood;
                }
                //B是子弹
                else
                {
                    var damage = harmData[entityB].injury;
                    var victimBlood = blood[entityA];
                    victimBlood.hp-=damage;
                    blood[entityA] = victimBlood;
                }
                
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Net;
using CharacterController;
using Infrastructure.utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UI;

namespace UI
{
    [RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(HitCollisionSystem))]
public partial struct HPUISystem : ISystem
{
    //绑定发送者、接收者还有事件
    struct displayUI
    {
        public Entity UIEntity;

        public Entity SenderEntity;
        //  public Entity HitEventEntity;
    }


    //查询bloodController
    EntityQuery sendEntitiesQuery;
    EntityQuery UIDisplayQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    { 
        sendEntitiesQuery = SystemAPI.QueryBuilder().WithAllRW<TargetEntity>().Build();
        UIDisplayQuery = SystemAPI.QueryBuilder().WithAllRW<DisplayUITag>().Build();

        //var sentEntities = sentEntitiesQuery.ToEntityArray(Allocator.Temp);

        state.RequireForUpdate(sendEntitiesQuery);
        state.RequireForUpdate(UIDisplayQuery);
        state.RequireForUpdate<BloodComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeArray<Entity> sendEntities = sendEntitiesQuery.ToEntityArray(Allocator.Persistent);
        NativeArray<Entity> UIEntities = UIDisplayQuery.ToEntityArray(Allocator.Persistent);

        NativeArray<displayUI> eventArray =
            new NativeArray<displayUI>(UIEntities.Length, Allocator.Temp);

        GetEntityMapping(state.EntityManager, eventArray, UIEntities, sendEntities);


        for (int idx = 0; idx < eventArray.Length; ++idx)
        {
            var mapping = eventArray[idx];

            if (!state.EntityManager.Exists(mapping.UIEntity) ||
                !state.EntityManager.Exists(mapping.SenderEntity))
            {
                continue;
            }

            //var displayTransform = state.EntityManager.GetComponentData<LocalTransform>(mapping.DisplayEntity);
            //var buffer = state.EntityManager.GetBuffer<StatefulCollisionEvent>(mapping.SenderEntity);

            var health=state.EntityManager.GetComponentData<BloodComponent>(mapping.SenderEntity);
            var hpBar = state.EntityManager.GetComponentObject<Image>(mapping.UIEntity);

            hpBar.fillAmount = health.hp / health.maxHp;

        }
       
    }


    // for (var i = 0; i < sendEntities.Length; ++i)
    // {
    //     //UI对应的物体
    //     var sentEntity = sendEntities[i];
    //
    //     //UI绑定的值（这里是血量）
    //     var health = state.EntityManager.GetComponentData<BloodComponent>(sentEntity);
    //
    //     //UI的buffer
    //     var targetBuffer = state.EntityManager.GetBuffer<TargetEntity>(sentEntity);
    //
    //     for (var j = 0; i < targetBuffer.Length; ++j)
    //     {
    //         //UI
    //         var hpBar = targetBuffer[j];
    //         var showUI = state.EntityManager.GetComponentObject<Image>(hpBar.Value);
    //         showUI.fillAmount = health.hp / health.maxHp;
    //     }
    //
    // }



    static void GetEntityMapping(EntityManager entityManager, NativeArray<displayUI> eventArray,
        NativeArray<Entity> UIEntities, NativeArray<Entity> sentEntities)
    {
        for (int i = 0; i < eventArray.Length; ++i)
        {
            displayUI ce = new displayUI();
            ce.UIEntity = UIEntities[i];

            for (int sourceIdx = 0, count = sentEntities.Length; sourceIdx < count; ++sourceIdx)
            {
                var sentEntity = sentEntities[sourceIdx];
                var targetBuffer = entityManager.GetBuffer<TargetEntity>(sentEntity);
                for (int targetIdx = 0; targetIdx < targetBuffer.Length; ++targetIdx)
                {
                    if (targetBuffer[targetIdx].Value == UIEntities[i])
                    {
                        /*if (entityManager.HasComponent<StatefulCollisionEvent>(sentEntity))
                            ce.CollisionEventEntity = sentEntity;
                        else*/
                        ce.UIEntity = sentEntity;
                    }
                }
            }

            eventArray[i] = ce;
        }
    }

}
}



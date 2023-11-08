using System.Collections;
using System.Collections.Generic;
using System.Net;
using CharacterController;
using Infrastructure.utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UI;
using UnityEngine;
using UI;

namespace UI
{
    public class OperatorPanel : MonoBehaviour
{
    //query
    private EntityQuery BaseBloodRedQuery;
    private EntityQuery BaseBloodBlueQuery;
    //血条
    public Image BaseBloodRed;
    public Image BaseBloodBlue;
    void Start()
    {
        //query
        BaseBloodRedQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BaseRed>());
        BaseBloodBlueQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BaseBlue>());
    }
    void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(BaseBloodRedQuery))
            BaseBloodRedQuery.Dispose();
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(BaseBloodBlueQuery))
            BaseBloodBlueQuery.Dispose();
    }

    void Update()
    {
        //等待世界初始化完成，查询完成且不为空
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(BaseBloodRedQuery) &&
            !BaseBloodRedQuery.IsEmpty && 
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(BaseBloodBlueQuery)&&
            !BaseBloodBlueQuery.IsEmpty)
        {
            //获得默认世界基地实体
            var BaseRedEntity = BaseBloodRedQuery.GetSingletonEntity();
            var BaseBlueEntity = BaseBloodBlueQuery.GetSingletonEntity();

            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(BaseRedEntity, new BaseBloodUI()
            {
                BaseBlood = BaseBloodRed
            });
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(BaseBlueEntity, new BaseBloodUI()
            {
                BaseBlood = BaseBloodBlue
            });

        }
    }
}

[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(SmoothlyTrackCameraTargetSystem))]
[BurstCompile]
partial class UIDisplaySystem : SystemBase
{

    [BurstCompile]
    protected override void OnUpdate()
    {
        //红基地
        foreach (var(_,bloodUI, blood)
                 in SystemAPI.Query<BaseRed,BaseBloodUI,BloodComponent>())
        {
            bloodUI.BaseBlood.fillAmount = blood.hp / blood.maxHp;
        }
        //蓝基地
        foreach (var(_,bloodUI, blood)
                 in SystemAPI.Query<BaseBlue,BaseBloodUI,BloodComponent>())
        {
            bloodUI.BaseBlood.fillAmount = blood.hp / blood.maxHp;
        }
    }
}

}

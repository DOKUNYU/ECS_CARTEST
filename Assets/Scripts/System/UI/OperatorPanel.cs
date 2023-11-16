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
using Identity;

namespace UI
{
    public class OperatorPanel : MonoBehaviour
{
    //query
    private EntityQuery _baseBloodRedQuery;
    private EntityQuery _baseBloodBlueQuery;
    //血条
    public Image BaseBloodRed;
    public Image BaseBloodBlue;
    void Start()
    {
        //query
        _baseBloodRedQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BaseRed>());
        _baseBloodBlueQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BaseBlue>());
    }
    void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_baseBloodRedQuery))
            _baseBloodRedQuery.Dispose();
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_baseBloodBlueQuery))
            _baseBloodBlueQuery.Dispose();
    }

    void Update()
    {
        //等待世界初始化完成，查询完成且不为空
        if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_baseBloodRedQuery) &&
            !_baseBloodRedQuery.IsEmpty && 
            World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_baseBloodBlueQuery)&&
            !_baseBloodBlueQuery.IsEmpty)
        {
            //获得默认世界基地实体
            var baseRedEntity = _baseBloodRedQuery.GetSingletonEntity();
            var baseBlueEntity = _baseBloodBlueQuery.GetSingletonEntity();

            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(baseRedEntity, new BaseBloodUI()
            {
                BaseBlood = BaseBloodRed
            });
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(baseBlueEntity, new BaseBloodUI()
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

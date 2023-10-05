using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace init
{
    public partial struct SpwanSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpwanSetting>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            
            var setting = SystemAPI.GetSingleton<SpwanSetting>();
            Debug.Log("tran.pos: "+setting.Position);
            
            var query=SystemAPI.QueryBuilder().WithAll<SpwanSetting>().Build();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var cars = new NativeArray<Entity>(1, Allocator.Temp);
            ecb.Instantiate(setting.Prefab, cars);
            ecb.Playback(state.EntityManager);
            Debug.Log("创建完成");
            EntityQuery carQuery = state.GetEntityQuery(typeof(Car));
            NativeArray<Entity> carEntity = carQuery.ToEntityArray(Allocator.Temp);
            Debug.Log("len "+carEntity.Length);
            foreach (var car in carEntity)
            {
                state.EntityManager.SetComponentData(car,new LocalTransform
                {
                    Position = setting.Position,
                    Rotation = setting.Rotation,
                    Scale = 1
                });
            }
            Debug.Log("修改完成");
            
        }
    }
    
}

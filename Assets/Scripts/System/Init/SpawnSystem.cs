using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Identity;

namespace Init
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SpawnSystem : ISystem
    {
        private EntityQuery carQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            carQuery=SystemAPI.QueryBuilder().WithAllRW< Car>().Build();
            //存在SpwanSetting才运行
            state.RequireForUpdate<SpawnSetting>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //只运行这一次onupdate  下次就没了
            state.Enabled = false;
            
            //获取spwaner setting
            var setting = SystemAPI.GetSingleton<SpawnSetting>();
            //Debug.Log("tran.pos: "+setting.Position);
            
            //var query=SystemAPI.QueryBuilder().WithAll<SpwanSetting>().Build();
            //创造ecb 在ecb中生成entity
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var cars = new NativeArray<Entity>(1, Allocator.Temp);
            ecb.Instantiate(setting.Prefab, cars);
            ecb.Playback(state.EntityManager);
            Debug.Log("创建完成");
            
            //获取生成之后的车的实体
            NativeArray<Entity> carEntity = carQuery.ToEntityArray(Allocator.Temp);
            
            //改变生成位置
            foreach (var car in carEntity)
            {
                state.EntityManager.SetComponentData(car,new LocalTransform
                {
                    Position = setting.Position,
                    Rotation = setting.Rotation,
                    Scale = 1
                });
            }
            
        }
    }
    
}

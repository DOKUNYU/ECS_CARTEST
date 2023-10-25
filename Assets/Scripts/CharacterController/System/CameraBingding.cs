using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace  CharacterController
{
    public class CameraBingding : MonoBehaviour
    {
        EntityQuery _CameraProxyQuery;

        void Start()
        {
            //query
            _CameraProxyQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
                new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<CameraProxy>()
                    .WithNone<MainCamera>());
        }

        void OnDestroy()
        {
            if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
                World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_CameraProxyQuery))
                _CameraProxyQuery.Dispose();
        }

        void Update()
        {
            //等待世界初始化完成，查询完成且不为空
            if (World.DefaultGameObjectInjectionWorld?.IsCreated == true &&
                World.DefaultGameObjectInjectionWorld.EntityManager.IsQueryValid(_CameraProxyQuery) &&
                !_CameraProxyQuery.IsEmpty)
            {
                //获得默认世界摄像机
                var cameraEntity = _CameraProxyQuery.GetSingletonEntity();

                World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(cameraEntity, new MainCamera()
                {
                    Transform = transform
                });
                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<CameraProxy>(cameraEntity);
            }
        }
    }


    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [BurstCompile]
    partial class SmoothlyTrackCameraTargetSystem : SystemBase
    {

        [BurstCompile]
        protected override void OnUpdate()
        {
            

            PhysicsWorld world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

            var mostRecentTime = SystemAPI.GetSingletonBuffer<MostRecentFixedTime>();
            var timeAhead = (float)(SystemAPI.Time.ElapsedTime - mostRecentTime[0].ElapsedTime);

            //之前给又proxy的实体加了maincamera的组件 然后把proxy卸掉了 现在应该是有个摄像机transform 自己有自己的transform
            foreach (var(mainCamera, localTran)
                     in SystemAPI.Query<MainCamera, RefRO<LocalToWorld>>())
            {
                //摄像机位置旋转
                var worldPosition = (float3)mainCamera.Transform.position;
                var worldRotation = mainCamera.Transform.rotation;
                
                //新位置旋转
                float3 newPosition = localTran.ValueRO.Position;
                quaternion newRotation = localTran.ValueRO.Rotation;
                
                //插值
                newPosition = math.lerp(worldPosition, newPosition, 0.9f);
                newRotation = Quaternion.Lerp(worldRotation, newRotation, 0.9f);

                mainCamera.Transform.SetPositionAndRotation(newPosition, newRotation);
            }
        }
    }
    }


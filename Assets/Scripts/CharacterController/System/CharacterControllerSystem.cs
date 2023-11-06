using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using Init;

namespace CharacterController
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(SpawnSystem))]
    public partial struct CharacterControllerSystem : ISystem
    {
        //不知道干什么的 既然用到了就用一下吧
        //Tau好像是力矩。。damping是阻尼
        const float k_DefaultTau = 0.4f;
        const float k_DefaultDamping = 0.9f;
        
        //查询characterController
        EntityQuery characterControllerQuery;
        EntityQuery ptzControllerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            characterControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW<CharacterController, CharacterControllerInternal>()
                .WithAllRW<LocalTransform>()
                .WithAll<PhysicsCollider>().Build();
            ptzControllerQuery= SystemAPI.QueryBuilder()
                .WithAllRW<PtzController>()
                .WithAllRW<LocalTransform>().Build();
            state.RequireForUpdate(characterControllerQuery);
            state.RequireForUpdate(ptzControllerQuery);
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //获取生成之后的车的实体
            EntityQuery carQuery = state.GetEntityQuery(typeof(Car));
            NativeArray<Entity> carEntity = carQuery.ToEntityArray(Allocator.Persistent);
            
            //获取和修改pitch
            foreach (var car in carEntity)
            {
                if (SystemAPI.GetComponent<CharacterControllerInternal>(car).Pitch != Entity.Null)
                {
                    continue;
                }
                else
                {
                    //判斷是否有child
                    if (SystemAPI.HasBuffer<Child>(car))
                    {
                        //获取实体上的child
                        var child = SystemAPI.GetBuffer<Child>(car);
                        //从child获取pitch
                        for (int i = 0; i < child.Length; i++)
                        {
                            //检查该子组件是不是有ptz
                            if (SystemAPI.HasComponent<PtzYaw>(child[i].Value))
                            {
                                var yawChild = SystemAPI.GetBuffer<Child>(child[i].Value);
                                for (int j = 0; j < yawChild.Length; j++)
                                {
                                    if (SystemAPI.HasComponent<PtzPitch>(yawChild[j].Value))
                                    {
                                        var carInternal = SystemAPI.GetComponentRW<CharacterControllerInternal>(car);
                                        carInternal.ValueRW.Pitch= yawChild[j].Value;
                                        carInternal.ValueRW.PitchTransform= SystemAPI.GetComponentRW<LocalTransform>(yawChild[j].Value).ValueRW;
                                        //carInternal.ValueRW.PitchTransformRW = SystemAPI.GetComponentRW<LocalTransform>(yawChild[j].Value);
                                        carInternal.ValueRW.PtzController= SystemAPI.GetComponentRW<PtzController>(yawChild[j].Value).ValueRW;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            var pitchJob = new PitchChangeJob()
            {
                PitchTransformLookUp = SystemAPI.GetComponentLookup<LocalTransform>(),
                internalLookUp = SystemAPI.GetComponentLookup<CharacterControllerInternal>(),
                CarEntities = carEntity
            };
            state.Dependency = pitchJob.Schedule(state.Dependency);
            
            
            
            //获取controller的chunk
            var chunks = characterControllerQuery.ToArchetypeChunkArray(Allocator.TempJob);
            //？
            
            var deferredImpulses = new NativeStream(chunks.Length, Allocator.TempJob);
            //获取物理世界实体
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            float dt = SystemAPI.Time.DeltaTime;
            
            //角色控制有关的job
            var ccJob = new CharacterControllerJob
            {
                CharacterControllerHandle = SystemAPI.GetComponentTypeHandle<CharacterController>(true),
                CharacterControllerInternalHandle = SystemAPI.GetComponentTypeHandle<CharacterControllerInternal>(),
                PhysicsColliderHandle = SystemAPI.GetComponentTypeHandle<PhysicsCollider>(true),
                LocalTransformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
                StatefulCollisionEventHandle = SystemAPI.GetBufferTypeHandle<StatefulCollisionEvent>(),
                StatefulTriggerEventHandle = SystemAPI.GetBufferTypeHandle<StatefulTriggerEvent>(),

                DeltaTime = dt,
                PhysicsWorldSingleton = physicsWorldSingleton,
                DeferredImpulseWriter = deferredImpulses.AsWriter()
            };
            
            
            state.Dependency = ccJob.ScheduleParallel(characterControllerQuery, state.Dependency);
            /*var pcJob = new PtzControllerJob
            {
                LocalTransformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
                PtzControllerHandle = SystemAPI.GetComponentTypeHandle<PtzController>(true)
            };*/
            
            //平滑运动？
            var smoothedCharacterControllerQuery = SystemAPI.QueryBuilder()
                .WithAllRW<CharacterControllerInternal, PhysicsGraphicalSmoothing>().Build();
            var copyVelocitiesHandle =
                new CopyVelocityToGraphicalSmoothingJob().ScheduleParallel(smoothedCharacterControllerQuery,
                    state.Dependency);
            
            var applyJob = new ApplyDefferedPhysicsUpdatesJob()
            {
                Chunks = chunks,
                DeferredImpulseReader = deferredImpulses.AsReader(),
                PhysicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
                PhysicsMassLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true),
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            };
            
            state.Dependency = applyJob.Schedule(state.Dependency);
            state.Dependency = deferredImpulses.Dispose(state.Dependency);
            state.Dependency = JobHandle.CombineDependencies(state.Dependency, copyVelocitiesHandle);
        }

        // override the behavior of CopyPhysicsVelocityToSmoothing
        [BurstCompile]
        partial struct CopyVelocityToGraphicalSmoothingJob : IJobEntity
        {
            public void Execute(in CharacterControllerInternal ccInternal,
                ref PhysicsGraphicalSmoothing smoothing)
            {
                smoothing.CurrentVelocity = ccInternal.Velocity;
            }
        }

       

        [BurstCompile]
        struct PitchChangeJob : IJob
        {
            public ComponentLookup<LocalTransform> PitchTransformLookUp;
            public ComponentLookup<CharacterControllerInternal> internalLookUp;
            public NativeArray<Entity> CarEntities;

            public void Execute()
            {
                //获取和修改pitch
                foreach (var car in CarEntities)
                {
                    if (internalLookUp[car].Pitch != Entity.Null)
                    {
                        var pitch = internalLookUp[car].Pitch;
                        var pitchTransform = PitchTransformLookUp.GetRefRW(pitch);
                        var newRotation = internalLookUp[car].PitchTransform.Rotation;
                        newRotation = Quaternion.Lerp(pitchTransform.ValueRO.Rotation, newRotation, 0.9f);
                        pitchTransform.ValueRW.Rotation = newRotation;
                        continue;
                    }
                }
            }

        }

        [BurstCompile]
        struct CharacterControllerJob : IJobChunk
        {
            public float DeltaTime;
            
            //一些query
            [ReadOnly] public PhysicsWorldSingleton PhysicsWorldSingleton;
            public ComponentTypeHandle<CharacterControllerInternal> CharacterControllerInternalHandle; 
            public ComponentTypeHandle<LocalTransform> LocalTransformHandle;  
            [ReadOnly] public ComponentTypeHandle<CharacterController> CharacterControllerHandle;
            [ReadOnly] public ComponentTypeHandle<PhysicsCollider> PhysicsColliderHandle;
            
            public BufferTypeHandle<StatefulCollisionEvent> StatefulCollisionEventHandle;
            public BufferTypeHandle<StatefulTriggerEvent> StatefulTriggerEventHandle;
            
            //记录冲量。避免两个character作用于同一个body
            [NativeDisableParallelForRestriction] public NativeStream.Writer DeferredImpulseWriter;
            


            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                //如果是false就断言
                Assert.IsFalse(useEnabledMask);
                
                //获取四个组件的chunk
                var chunkCCData = chunk.GetNativeArray(ref CharacterControllerHandle);
                var chunkCCInternalData = chunk.GetNativeArray(ref CharacterControllerInternalHandle);
                var chunkPhysicsColliderData = chunk.GetNativeArray(ref PhysicsColliderHandle);
                var chunkLocalTransformData = chunk.GetNativeArray(ref LocalTransformHandle);
                
                //有没有创造collision和trigger事件
                var hasChunkCollisionEventBufferType = chunk.Has(ref StatefulCollisionEventHandle);
                var hasChunkTriggerEventBufferType = chunk.Has(ref StatefulTriggerEventHandle);
                
                //获取buffer
                BufferAccessor<StatefulCollisionEvent> collisionEventBuffers = default;
                BufferAccessor<StatefulTriggerEvent> triggerEventBuffers = default;
                if (hasChunkCollisionEventBufferType) collisionEventBuffers = chunk.GetBufferAccessor(ref StatefulCollisionEventHandle);
                if (hasChunkTriggerEventBufferType)triggerEventBuffers = chunk.GetBufferAccessor(ref StatefulTriggerEventHandle);
                
                
                //从块索引获取buffer
                DeferredImpulseWriter.BeginForEachIndex(unfilteredChunkIndex);

                //块索引
                for (int i = 0; i < chunk.Count; i++)
                {
                    //获取对应组件值
                    var ccComponentData = chunkCCData[i];
                    var ccInternalData = chunkCCInternalData[i];
                    var collider = chunkPhysicsColliderData[i];
                    var localTransform = chunkLocalTransformData[i];
                    
                    //获取pitch
                    var pitch = ccInternalData.Pitch;
                    var pitchLocalTransform = ccInternalData.PitchTransform;
                    var ptzController = ccInternalData.PtzController;
                    
                    
                    //dynamic buffer
                    DynamicBuffer<StatefulCollisionEvent> collisionEventBuffer = default;
                    DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer = default;
                    if (hasChunkCollisionEventBufferType)collisionEventBuffer = collisionEventBuffers[i];
                    if (hasChunkTriggerEventBufferType)triggerEventBuffer = triggerEventBuffers[i];
                    
                    //Collision filter
                    if (!collider.IsValid || collider.Value.Value.GetCollisionFilter().IsEmpty)continue;
                     
                    
                    //竖直方向 原来应该是跳跃的时候用？
                    var up = math.select(math.up(), -math.normalize(ccComponentData.Gravity),
                        math.lengthsq(ccComponentData.Gravity) > 0f);
                    
                    // Character step input
                    //具体参数输入
                    Util.StepInput stepInput = new Util.StepInput
                    {
                        PhysicsWorldSingleton = PhysicsWorldSingleton,
                        DeltaTime = DeltaTime,
                        Up = up,
                        Gravity = ccComponentData.Gravity,
                        MaxIterations = ccComponentData.MaxIterations,
                        Tau = k_DefaultTau,
                        Damping = k_DefaultDamping,
                        SkinWidth = ccComponentData.SkinWidth,
                        ContactTolerance = ccComponentData.ContactTolerance,
                        MaxSlope = ccComponentData.MaxSlope,
                        RigidBodyIndex = PhysicsWorldSingleton.PhysicsWorld.GetRigidBodyIndex(ccInternalData.Entity),
                        CurrentVelocity = ccInternalData.Velocity.Linear,
                        MaxMovementSpeed = ccComponentData.MaxMovementSpeed
                    };
                    
                    // 储存当前位置？
                    RigidTransform transform = new RigidTransform
                    {
                        pos = localTransform.Position,
                        rot = localTransform.Rotation
                    };
                    
                    //碰撞事件的list
                    NativeList<StatefulCollisionEvent> currentFrameCollisionEvents = default;
                    if (ccComponentData.RaiseCollisionEvents != 0)currentFrameCollisionEvents = new NativeList<StatefulCollisionEvent>(Allocator.Temp);
                    
                    // trigger
                    NativeList<StatefulTriggerEvent> currentFrameTriggerEvents = default;
                    if (ccComponentData.RaiseTriggerEvents != 0)currentFrameTriggerEvents = new NativeList<StatefulTriggerEvent>(Allocator.Temp);
                    
                    
                    // 检查是否离地
                    Util.CheckSupport(in PhysicsWorldSingleton, ref collider, stepInput, transform,
                        out ccInternalData.SupportedState, out float3 surfaceNormal, out float3 surfaceVelocity,
                        currentFrameCollisionEvents);
                    
                    // 处理输入
                    float3 desiredVelocity = ccInternalData.Velocity.Linear;
                    HandleUserInput(ccComponentData, ptzController,stepInput.Up, surfaceVelocity, ref ccInternalData,
                        ref desiredVelocity);
                    
                    
                    // Calculate actual velocity with respect to surface
                    // 计算相对于表面的实际速度
                    if (ccInternalData.SupportedState == Util.CharacterSupportState.Supported)
                    {
                        CalculateMovement(ccInternalData.CurrentRotationAngle, stepInput.Up, ccInternalData.IsJumping,
                            ccInternalData.Velocity.Linear, desiredVelocity, surfaceNormal, surfaceVelocity,
                            out ccInternalData.Velocity.Linear);
                    }
                    else
                    {
                        ccInternalData.Velocity.Linear = desiredVelocity;
                    }
                    
                    // World collision + integrate
                    Util.CollideAndIntegrate(stepInput, ccComponentData.CharacterMass,
                        ccComponentData.AffectsPhysicsBodies != 0,
                        ref collider, ref transform, ref ccInternalData.Velocity.Linear, ref DeferredImpulseWriter,
                        currentFrameCollisionEvents, currentFrameTriggerEvents);
                    
                    // Update collision event status
                    if (currentFrameCollisionEvents.IsCreated)UpdateCollisionEvents(currentFrameCollisionEvents, collisionEventBuffer);
                    if (currentFrameTriggerEvents.IsCreated) UpdateTriggerEvents(currentFrameTriggerEvents, triggerEventBuffer);
                    
                    // Write back and orientation integration
                    localTransform.Position = transform.pos;
                    localTransform.Rotation = quaternion.AxisAngle(up, ccInternalData.CurrentRotationAngle);//轴  角
                    var currRotation = quaternion.AxisAngle(math.left(), ccInternalData.CurrentPitchRotationAngle);
                    if (currRotation.value.x > 0.45f)
                    {
                        currRotation.value.x = 0.45f;
                        currRotation.value.w = 0.888f;
                        ccInternalData.CurrentPitchRotationAngle = 49.31f;
                    }

                    if (currRotation.value.x < 0)
                    {
                        currRotation.value.x = 0;
                        currRotation.value.w = 0.999f;
                        ccInternalData.CurrentPitchRotationAngle = 50.3f;
                    }
                    pitchLocalTransform.Rotation = currRotation;

                    //通过这个写回internaldata
                    ccInternalData.PitchTransform = pitchLocalTransform;
                    
                    // Write back to chunk data
                    {
                        chunkCCInternalData[i] = ccInternalData;
                        chunkLocalTransformData[i] = localTransform;

                    }
                }

                DeferredImpulseWriter.EndForEachIndex();
            }

            

            /// <summary>
            /// 处理用户输入
            /// </summary>
            /// <param name="cc"></param>
            /// <param name="up"></param>
            /// <param name="surfaceVelocity"></param>
            /// <param name="ccInternal"></param>
            /// <param name="linearVelocity"></param>
            private void HandleUserInput(CharacterController cc,PtzController ptz, float3 up,
                float3 surfaceVelocity,
                ref CharacterControllerInternal ccInternal, ref float3 linearVelocity)
            {
                // Reset jumping state and unsupported velocity
                if (ccInternal.SupportedState == Util.CharacterSupportState.Supported)
                {
                    ccInternal.IsJumping = false;
                    ccInternal.UnsupportedVelocity = float3.zero;
                }

                // Movement and jumping
                bool shouldJump = false;
                float3 requestedMovementDirection = float3.zero;
                {
                    float3 forward = math.forward(quaternion.identity);
                    float3 right = math.cross(up, forward);

                    float horizontal = ccInternal.Input.Movement.x;
                    float vertical = -ccInternal.Input.Movement.y;
                    bool haveInput = (math.abs(horizontal) > float.Epsilon) || (math.abs(vertical) > float.Epsilon);
                    if (haveInput)
                    {
                        float3 localSpaceMovement = forward * vertical + right * horizontal;
                        float3 worldSpaceMovement =
                            math.rotate(quaternion.AxisAngle(up, ccInternal.CurrentRotationAngle),
                                localSpaceMovement);
                        requestedMovementDirection = math.normalize(worldSpaceMovement);
                    }

                }

                // Turning
                {
                    float horizontal = ccInternal.Input.Looking.x;
                    float vertical =ccInternal.Input.Looking.y;
                    bool haveInputX = (math.abs(horizontal) > float.Epsilon);
                    bool haveInputY = (math.abs(vertical) > float.Epsilon);
                    if (haveInputX)
                    {
                        var userRotationSpeedX = horizontal * cc.RotationSpeed;
                        ccInternal.Velocity.Angular = -userRotationSpeedX * up;
                        ccInternal.CurrentRotationAngle += userRotationSpeedX * DeltaTime;
                    }
                    else
                    {
                        ccInternal.Velocity.Angular = 0f;
                    }
                    
                    if (haveInputY)
                    {
                        var userRotationSpeedY = vertical * ptz.RotationSpeed;
                        ccInternal.Velocity.Angular = -userRotationSpeedY * math.left();
                        ccInternal.CurrentPitchRotationAngle += userRotationSpeedY * DeltaTime;
                    }

                }

                // Apply input velocities
                {
                    if (shouldJump)
                    {
                        // Add jump speed to surface velocity and make character unsupported
                        ccInternal.IsJumping = true;
                        ccInternal.SupportedState = Util.CharacterSupportState.Unsupported;
                        ccInternal.UnsupportedVelocity = surfaceVelocity + cc.JumpUpwardsSpeed * up;
                    }
                    else if (ccInternal.SupportedState != Util.CharacterSupportState.Supported)
                    {
                        // Apply gravity
                        ccInternal.UnsupportedVelocity += cc.Gravity * DeltaTime;
                    }
                    
                    // If unsupported then keep jump and surface momentum
                    linearVelocity = requestedMovementDirection * cc.MovementSpeed +
                        (ccInternal.SupportedState != Util.CharacterSupportState.Supported
                            ? ccInternal.UnsupportedVelocity
                            : float3.zero);
                }
            }
            
            /// <summary>
            /// 收集碰撞事件，放进Buffer
            /// </summary>
            /// <param name="collisionEvents"></param>
            /// <param name="collisionEventBuffer"></param>
            private void UpdateCollisionEvents(NativeList<StatefulCollisionEvent> collisionEvents,
                DynamicBuffer<StatefulCollisionEvent> collisionEventBuffer)
            {
                var previousFrameCollisionEvents =
                    new NativeList<StatefulCollisionEvent>(collisionEventBuffer.Length, Allocator.Temp);

                for (int i = 0; i < collisionEventBuffer.Length; i++)
                {
                    var collisionEvent = collisionEventBuffer[i];
                    if (collisionEvent.State != StatefulEventState.Exit)
                    {
                        previousFrameCollisionEvents.Add(collisionEvent);
                    }
                }

                var eventsWithState = new NativeList<StatefulCollisionEvent>(collisionEvents.Length, Allocator.Temp);
                StatefulSimulationEventBuffers<StatefulCollisionEvent>.GetStatefulEvents(previousFrameCollisionEvents,
                    collisionEvents, eventsWithState);

                collisionEventBuffer.Clear();
                for (int i = 0; i < eventsWithState.Length; i++)
                {
                    collisionEventBuffer.Add(eventsWithState[i]);
                }
            }
            
            private void UpdateTriggerEvents(NativeList<StatefulTriggerEvent> triggerEvents,
                DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer)
            {
                var previousFrameTriggerEvents =
                    new NativeList<StatefulTriggerEvent>(triggerEventBuffer.Length, Allocator.Temp);

                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];
                    if (triggerEvent.State != StatefulEventState.Exit)
                    {
                        previousFrameTriggerEvents.Add(triggerEvent);
                    }
                }

                var eventsWithState = new NativeList<StatefulTriggerEvent>(triggerEvents.Length, Allocator.Temp);

                StatefulSimulationEventBuffers<StatefulTriggerEvent>.GetStatefulEvents(previousFrameTriggerEvents,
                    triggerEvents, eventsWithState);

                triggerEventBuffer.Clear();

                for (int i = 0; i < eventsWithState.Length; i++)
                {
                    triggerEventBuffer.Add(eventsWithState[i]);
                }
            }
            
            private void CalculateMovement(float currentRotationAngle, float3 up, bool isJumping,
                float3 currentVelocity, float3 desiredVelocity, float3 surfaceNormal, float3 surfaceVelocity,
                out float3 linearVelocity)
            {
                float3 forward = math.forward(quaternion.AxisAngle(up, currentRotationAngle));

                quaternion surfaceFrame;

                float3 binorm;
                {
                    binorm = math.cross(forward, up);
                    binorm = math.normalize(binorm);

                    float3 tangent = math.cross(binorm, surfaceNormal);
                    tangent = math.normalize(tangent);

                    binorm = math.cross(tangent, surfaceNormal);
                    binorm = math.normalize(binorm);

                    surfaceFrame = new quaternion(new float3x3(binorm, tangent, surfaceNormal));
                }

                float3 relative = currentVelocity - surfaceVelocity;

                relative = math.rotate(math.inverse(surfaceFrame), relative);

                float3 diff;
                {
                    float3 sideVec = math.cross(forward, up);
                    float fwd = math.dot(desiredVelocity, forward);
                    float side = math.dot(desiredVelocity, sideVec);
                    float len = math.length(desiredVelocity);
                    float3 desiredVelocitySF = new float3(-side, -fwd, 0.0f);
                    desiredVelocitySF = math.normalizesafe(desiredVelocitySF, float3.zero);
                    desiredVelocitySF *= len;
                    diff = desiredVelocitySF - relative;
                }

                relative += diff;

                linearVelocity = math.rotate(surfaceFrame, relative) + surfaceVelocity +
                                 (isJumping ? math.dot(desiredVelocity, up) * up : float3.zero);
            }
        }
        
        
        
        [BurstCompile]
        struct ApplyDefferedPhysicsUpdatesJob : IJob
        {
            // Chunks can be deallocated at this point
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            public NativeStream.Reader DeferredImpulseReader;
            public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;
            [ReadOnly] public ComponentLookup<PhysicsMass> PhysicsMassLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

            public void Execute()
            {
                int index = 0;
                int maxIndex = DeferredImpulseReader.ForEachCount;
                DeferredImpulseReader.BeginForEachIndex(index++);
                while (DeferredImpulseReader.RemainingItemCount == 0 && index < maxIndex)
                {
                    DeferredImpulseReader.BeginForEachIndex(index++);
                }

                while (DeferredImpulseReader.RemainingItemCount > 0)
                {
                    // Read the data
                    var impulse = DeferredImpulseReader.Read<DeferredCharacterImpulse>();
                    while (DeferredImpulseReader.RemainingItemCount == 0 && index < maxIndex)
                    {
                        DeferredImpulseReader.BeginForEachIndex(index++);
                    }

                    PhysicsVelocity pv = PhysicsVelocityLookup[impulse.Entity];
                    PhysicsMass pm = PhysicsMassLookup[impulse.Entity];
                    LocalTransform t = LocalTransformLookup[impulse.Entity];

                    // Don't apply on kinematic bodies
                    if (pm.InverseMass > 0.0f)
                    {
                        // Apply impulse

                        pv.ApplyImpulse(pm, t.Position, t.Rotation, impulse.Impulse, impulse.Point);

                        // Write back
                        PhysicsVelocityLookup[impulse.Entity] = pv;
                    }
                }
            }
        }
    }
    
}


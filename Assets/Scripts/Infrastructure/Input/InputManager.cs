// in order to circumvent API breakages that do not affect physics, some packages are removed from the project on CI
// any code referencing APIs in com.unity.inputsystem must be guarded behind UNITY_INPUT_SYSTEM_EXISTS
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Burst;
using Unity.Entities;
using CharacterController;
using Unity.Jobs;
using UnityEngine.InputSystem.HID;

namespace Infrastructure.Input
{
        /// <summary>
        /// 获取输入
        /// </summary>
        [UpdateInGroup(typeof(InitializationSystemGroup))]
        partial class InputBase : SystemBase
#if ENABLE_INPUT_SYSTEM
            ,
            CarControls.IMainActions
#endif
    {
            //查询input
            EntityQuery m_CharacterControllerInputQuery;
            //EntityQuery m_CharacterGunInputQuery;
        
#pragma warning disable 649
        
            //移动和转向
            Vector2 m_CharacterMovement;
            Vector2 m_CharacterLooking;
            private float m_CharacterFire;
        
#pragma warning restore 649
            
        //创造create     
        protected override void OnCreate()
        {
#if ENABLE_INPUT_SYSTEM

            //新建input类
            m_InputActions = new CarControls();
            m_InputActions.main.SetCallbacks(this);
#endif
                
            //获取Input的输入
            m_CharacterControllerInputQuery = GetEntityQuery(typeof(CharacterController.Input));
            
                
        }
#if ENABLE_INPUT_SYSTEM

        //创建新的输入对象
        CarControls m_InputActions;

        //开始停止输入接收
        protected override void OnStartRunning() => m_InputActions.Enable();
        protected override void OnStopRunning() => m_InputActions.Disable();
    
        //绑定输入函数
        void CarControls.IMainActions.OnMovement(InputAction.CallbackContext context) => m_CharacterMovement = context.ReadValue<Vector2>();
        void CarControls.IMainActions.OnCamera(InputAction.CallbackContext context) => m_CharacterLooking = context.ReadValue<Vector2>();
        
        void CarControls.IMainActions.OnFire(InputAction.CallbackContext context)=> m_CharacterFire= context.ReadValue<float>();
                
#endif

        protected override void OnUpdate()
        {
            // 如果没有实体存在 那就创造一个带Input的实体
            if (m_CharacterControllerInputQuery.CalculateEntityCount() == 0)
            {
                EntityManager.CreateEntity(typeof(CharacterController.Input));
            }
            //设置单例
            m_CharacterControllerInputQuery.SetSingleton(new CharacterController.Input
            {
                Looking = m_CharacterLooking,
                Movement = m_CharacterMovement,
                Fire = m_CharacterFire
            });
        }
    }
        // This input system simply applies the same character input
// information to every character controller in the scene
        [RequireMatchingQueriesForUpdate]
        [UpdateInGroup(typeof(InitializationSystemGroup))]
        [UpdateAfter(typeof(InputBase))]
        public partial struct CharacterControllerInputSystem : ISystem
        {
            [BurstCompile]
            public void OnCreate(ref SystemState state)
            {
                state.RequireForUpdate<CharacterController.Input>();
            }

            [BurstCompile]
            public void OnUpdate(ref SystemState state)
            {
                //执行输入的实时传递
                state.Dependency = new InputJob
                {
                    // Read user input
                    Input = SystemAPI.GetSingleton<CharacterController.Input>()
                }.ScheduleParallel(state.Dependency);
            }
        }

        public partial struct InputJob : IJobEntity
        {
            public CharacterController.Input Input;

            public void Execute(ref CharacterControllerInternal cc)
            {
                cc.Input.Movement = Input.Movement*0.5f;
                cc.Input.Movement.x = -cc.Input.Movement.x;
                cc.Input.Looking = Input.Looking*0.07f;
                cc.Input.Looking.y=-cc.Input.Looking.y;
                cc.Input.Fire = Input.Fire;
            }
        }
    
}


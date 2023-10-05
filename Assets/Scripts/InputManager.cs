// in order to circumvent API breakages that do not affect physics, some packages are removed from the project on CI
// any code referencing APIs in com.unity.inputsystem must be guarded behind UNITY_INPUT_SYSTEM_EXISTS
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;

namespace Infrastructure.Input
{
    partial class InputManager : SystemBase
#if UNITY_INPUT_SYSTEM_EXISTS
        ,
        InputActions.IMainActions,
#endif
    {
        EntityQuery m_CharacterControllerInputQuery;
        //EntityQuery m_CharacterGunInputQuery;
#pragma warning disable 649
        Vector2 m_CharacterMovement;
        Vector2 m_CharacterLooking;
#pragma warning restore 649
        protected override void OnCreate()
        {
#if UNITY_INPUT_SYSTEM_EXISTS
            m_InputActions = new CarControls();
            m_InputActions.main.SetCallbacks(this);
#endif
            //m_CharacterControllerInputQuery = GetEntityQuery(typeof(CharacterController.Input));
#if UNITY_INPUT_SYSTEM_EXISTS
        CarControls m_InputActions;

        protected override void OnStartRunning() => m_InputActions.Enable();

        protected override void OnStopRunning() => m_InputActions.Disable();
#endif
                
        }

        protected override void OnUpdate()
        {
                
        }

    }
    
}


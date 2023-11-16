using Unity.Entities;
using UnityEngine;

namespace CharacterController
{
    public class CameraAuthoring : MonoBehaviour
    {

        class Baker : Baker<CameraAuthoring>
        {
            public override void Bake(CameraAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraProxy() { });
            }
        }
    }
    
}

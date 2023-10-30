using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Init
{
    public class SpwanerAuthoring : MonoBehaviour
    {
        public GameObject prefabs;
        public Transform Trans;

        public class Baker : Baker<SpwanerAuthoring>
        {
            public override void Bake(SpwanerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //spwan
                AddComponent(entity,new SpwanSetting
                {
                    Prefab = GetEntity(authoring.prefabs, TransformUsageFlags.Dynamic),
                    Position = authoring.Trans.position,
                    Rotation = authoring.Trans.rotation
                });
                
            }
        }
    }
    
}


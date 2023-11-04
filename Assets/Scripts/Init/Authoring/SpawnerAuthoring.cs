using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Init
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefabs;
        public Transform Trans;

        public class Baker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //spwan
                AddComponent(entity,new SpawnSetting
                {
                    Prefab = GetEntity(authoring.prefabs, TransformUsageFlags.Dynamic),
                    Position = authoring.Trans.position,
                    Rotation = authoring.Trans.rotation
                });
                
            }
        }
    }
    
}


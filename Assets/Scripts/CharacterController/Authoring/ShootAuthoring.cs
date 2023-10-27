using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CharacterController
{
    public class ShootAuthoring : MonoBehaviour
    {
        public GameObject prefabs;

        public class Baker : Baker<ShootAuthoring>
        {
            public override void Bake(ShootAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var bullet = GetEntity(authoring.prefabs, TransformUsageFlags.Dynamic);
                //bullet spwanSetting
                AddComponent(entity,new ShootSetting
                {
                    BulletPrefab = bullet,
                    SpwanPrefab = entity
                });
                
            }
        }

    }
}

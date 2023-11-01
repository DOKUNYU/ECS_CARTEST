using System.Collections;
using System.Collections.Generic;
using CharacterController;
using Infrastructure.utils;
using Unity.Entities;
using UnityEngine;

public class DisplayUIAuthoring : MonoBehaviour,IReceiveEntity
{
    class Baker : Baker<DisplayUIAuthoring>
    {
        public override void Bake(DisplayUIAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DisplayUITag());
        }
    }
    
    
}

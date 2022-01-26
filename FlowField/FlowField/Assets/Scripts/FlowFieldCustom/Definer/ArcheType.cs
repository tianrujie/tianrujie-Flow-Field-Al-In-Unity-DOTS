using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public static class ArchyType
{
    public static EntityArchetype AgentArchetype;

    public static void Initalize()
    {
        if (AgentArchetype.Valid)
            return;
        
        AgentArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(NonUniformScale),
            typeof(Rotation),
            typeof(LocalToWorld));
    }
}

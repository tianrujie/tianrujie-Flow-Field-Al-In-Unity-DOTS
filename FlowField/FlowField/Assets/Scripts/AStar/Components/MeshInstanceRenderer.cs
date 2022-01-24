using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Render Mesh with Material (must be instanced material) by object to world matrix.
/// Specified by TransformMatrix associated with Entity.
/// </summary>
[SerializeField]
public struct MeshInstanceRenderer : ISharedComponentData, IEquatable<MeshInstanceRenderer>
{
    public Mesh                 mesh;
    public Material             material;    
    public int                  subMesh;
    public float                data;

    public ShadowCastingMode    castShadows;
    public bool                 receiveShadows;

    public bool Equals(MeshInstanceRenderer other)
    {
        return Equals(mesh, other.mesh) && Equals(material, other.material);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is MeshInstanceRenderer && Equals((MeshInstanceRenderer)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((mesh != null ? mesh.GetHashCode() : 0) * 397) ^ (material != null ? material.GetHashCode() : 0);
        }
    }
}

public struct ColorWrapper : IComponentData
{
    public UnityEngine.Color Value;
}

public class Const
{
    public static int Common = 1 << 1;
    public static int Alert = 1 << 2;
    public static int Boom = 1 << 3;
}

public struct CellState : IComponentData
{
    public int Index;
    public int State;
}
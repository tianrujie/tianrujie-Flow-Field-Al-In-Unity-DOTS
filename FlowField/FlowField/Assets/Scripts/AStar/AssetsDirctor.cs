using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AssetsDirctor : MonoBehaviour
{
    public Material cellMat;
    public Mesh cellMesh;
    public MeshInstanceRenderer MeshInstanceRenderer;
    public EntityArchetype CellArchetype;
    private static AssetsDirctor _instance;
    public static AssetsDirctor Instance
    {
        get => _instance;
        set => _instance = value;
    }

    public void Awake()
    {
        AssetsDirctor.Instance = this;
        AssetsDirctor.Instance.InitAssets();
        
        CellArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation), 
            typeof(MeshInstanceRenderer),
            typeof(ColorWrapper),
            typeof(CellState));
    }

    public void Start()
    {
        
    }

    public void InitAssets()
    {
        MeshInstanceRenderer = new MeshInstanceRenderer()
        {
            mesh = cellMesh,
            material = cellMat,
        };
    }
    
    //_entityManager.SetComponentData(soldierEntity,new Rotation() {Value = Quaternion.Euler(CameraRotateAngle.x,  CameraRotateAngle.y, 0)});
}

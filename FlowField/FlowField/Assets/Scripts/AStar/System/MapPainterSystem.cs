using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Assert = UnityEngine.Assertions.Assert;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

[DisableAutoCreation]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class MapPainterSystem : ComponentSystem
{
    public MapDirector director;
    // Instance renderer takes only batches of 1023
    Matrix4x4[] m_MatricesArray = new Matrix4x4[1023];
    //Main Color 
    private readonly Vector4[] colorArray = new Vector4[1023];
    MaterialPropertyBlock properties = new MaterialPropertyBlock();
    List<MeshInstanceRenderer> CacheduniqueRendererComponent = new List<MeshInstanceRenderer>(100);
    
    public EntityQuery MapUnitQuery;
    
    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
    public unsafe static void CopyMatrices
        (NativeArray<LocalToWorld> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        // @TODO: This is using unsafe code because the Unity DrawInstances API takes a Matrix4x4[] instead of NativeArray.
        // We want to use the ComponentDataArray.CopyTo method
        // because internally it uses memcpy to copy the data,
        // if the nativeslice layout matches the layout of the component data. It's very fast...
        fixed (Matrix4x4* matricesPtr = outMatrices)
        {
            // LocalToWorld* localPtr = (LocalToWorld*)matricesPtr;
            Assert.AreEqual(sizeof(Matrix4x4), sizeof(LocalToWorld));
            var matricesSlice =
                NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<LocalToWorld>(matricesPtr, sizeof(Matrix4x4),
                    length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice,
                AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            NativeSlice<LocalToWorld> l = new NativeSlice<LocalToWorld>(transforms, beginIndex, length);
            matricesSlice.CopyFrom(l);
        }
    }
    
    int colorProp = Shader.PropertyToID("_Color");
    
    protected override void OnCreate()
    {
        base.OnCreate();
        MapUnitQuery = GetEntityQuery(typeof(LocalToWorld),typeof(Translation),typeof(Scale),typeof(Rotation), typeof(MeshInstanceRenderer),typeof(ColorWrapper),typeof(CellState));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
    }
    
    protected override void OnUpdate()
    {
        Entities.WithAll<ColorWrapper,CellState>().ForEach((Entity entity, ref ColorWrapper colorWrapper,ref CellState cellState) =>
        {
            if (director.alertCells.Contains(cellState.Index))
            {
                colorWrapper.Value = Color.green;
                return;
            }
            
            if (director.boomCells.Contains(cellState.Index))
            {
                colorWrapper.Value = Color.red;
                return;
            }
            
            colorWrapper.Value = Color.gray;
        });
        
        //获取所有唯一的MeshInstanceRender 组件
        EntityManager.GetAllUniqueSharedComponentData(CacheduniqueRendererComponent);

        for (int i = 0; i < CacheduniqueRendererComponent.Count; i++)
        {
            var renderer = CacheduniqueRendererComponent[i];
            if (renderer.mesh == null || renderer.mesh.vertexCount == 0)
                continue;
                
            if (renderer.material == null)
                continue;
            //设置Query筛选器
            MapUnitQuery.SetSharedComponentFilter(renderer);
            var transforms = MapUnitQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            var colors = MapUnitQuery.ToComponentDataArray<ColorWrapper>(Allocator.TempJob);

            int beginIndex = 0;
            while (beginIndex < transforms.Length)
            {
                properties.Clear();
                var length = math.min(m_MatricesArray.Length, transforms.Length - beginIndex);
                CopyMatrices(transforms,beginIndex,length,m_MatricesArray);

                int k = 0;
                for (int j = beginIndex; j < beginIndex + length; j++)
                {
                    var color = colors[j].Value;
                    colorArray[k++] = new Vector4(color.r, color.g, color.b, color.a);
                }
                
                properties.SetVectorArray(colorProp,colorArray);
                Graphics.DrawMeshInstanced(renderer.mesh,renderer.subMesh,renderer.material,m_MatricesArray,length
                ,properties,renderer.castShadows,renderer.receiveShadows);

                beginIndex += length;
            }
            
            transforms.Dispose();
            colors.Dispose();
        }
        
        CacheduniqueRendererComponent.Clear();
    }
}



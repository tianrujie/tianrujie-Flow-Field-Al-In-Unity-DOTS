using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Camp
{
    Enemy = 0,
    Self = 1,
}
public class AgentWrapper : MonoBehaviour
{
    public GameObject _prefab;
    public float Radius = 3;
    public float MaxSpeed = 5;
    public float destinationMoveSpeed = 0.01f;
    public Camp UnitCamp;
    
    public Entity _entityPrefab;
    private Entity _agent;
    private EntityManager _entityManager;
    private BlobAssetStore _blobAssetStore;
    
    private void Awake()
    {
        _blobAssetStore = new BlobAssetStore();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
        _entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_prefab, settings);
    }

    // Start is called before the first frame update
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreatAgent();
    }

    private void CreatAgent()
    {
        MovementData newEntityMovementData = new MovementData
        {
            moveSpeed = MaxSpeed,
            destinationReached = false,
            destinationMoveSpeed = destinationMoveSpeed
        };
        
        AgentData agentData = new AgentData()
        {
            Camp = (byte)UnitCamp,
            Radius = Radius,
        };
        
        ORCATag tag = new ORCATag();
        
        var _agent = _entityManager.Instantiate(_entityPrefab);
        _entityManager.AddComponentData(_agent, newEntityMovementData);
        _entityManager.SetComponentData(_agent, new Translation() {Value = transform.position});
        _entityManager.AddComponentData(_agent,agentData);
        _entityManager.AddComponentData(_agent,tag);
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _blobAssetStore.Dispose();
    }
}

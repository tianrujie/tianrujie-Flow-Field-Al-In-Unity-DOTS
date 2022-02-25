using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace TMG.ECSFlowField
{
    public class CEntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _unitPrefab;
        [SerializeField] private int _numUnitsPerSpawn;
        [SerializeField] private float2 _maxSpawnPos;
        [SerializeField] private float _agentRadius;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _destinationMoveSpeed;
        
        private Entity _entityPrefab;
        private EntityManager _entityManager;
        private List<Entity> _unitsInGame;
        private BlobAssetStore _blobAssetStore;
        

        private void Awake()
        {
            _blobAssetStore = new BlobAssetStore();
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            _unitPrefab.transform.localScale = new Vector3(_agentRadius*2,_agentRadius*2,_agentRadius*2);
            _entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_unitPrefab, settings);
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityManager.AddComponent<EntityMovementData>(_entityPrefab);
            _unitsInGame = new List<Entity>();
        }
        
        private bool _leftTabDown = false;
        private void Update()
        {
            //generate agents
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                MovementData newEntityMovementData = new MovementData
                    {
                        moveSpeed = _moveSpeed,
                        destinationReached = false,
                        destinationMoveSpeed = _destinationMoveSpeed
                    };
                
                AgentData agentData = new AgentData()
                {
                     Camp = Const1.CAMP_A,
                     Radius = _agentRadius,
                };
        
                FlowFieldTag tag = new FlowFieldTag();
                for (int i = 0; i < _numUnitsPerSpawn; i++)
                {
                    var newUnit = _entityManager.Instantiate(_entityPrefab);
                    _entityManager.AddComponentData(newUnit, newEntityMovementData);
                    _unitsInGame.Add(newUnit);
                    float3 newPosition = new float3(Random.Range(-_maxSpawnPos.x, _maxSpawnPos.x), 0, Random.Range(-_maxSpawnPos.y, _maxSpawnPos.y));
                    _entityManager.SetComponentData(newUnit, new Translation {Value = newPosition});
                    _entityManager.AddComponentData(newUnit,agentData);
                    _entityManager.AddComponentData(newUnit,tag);
                }
            }

            //clear agents
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                foreach (Entity entity in _unitsInGame)
                {
                    _entityManager.DestroyEntity(entity);
                }
                _unitsInGame.Clear();
            }
        }
        
        private void OnDestroy()
        {
            _blobAssetStore.Dispose();        
        }
    }
}
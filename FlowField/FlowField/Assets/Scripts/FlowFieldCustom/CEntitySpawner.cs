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
            _entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_unitPrefab, settings);
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityManager.AddComponent<EntityMovementData>(_entityPrefab);
            _unitsInGame = new List<Entity>();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EntityMovementData newEntityMovementData = new EntityMovementData
                    {
                        moveSpeed = _moveSpeed,
                        destinationReached = false,
                        destinationMoveSpeed = _destinationMoveSpeed
                    };
                for (int i = 0; i < _numUnitsPerSpawn; i++)
                {
                    var newUnit = _entityManager.Instantiate(_entityPrefab);
                    _entityManager.SetComponentData(newUnit, newEntityMovementData);
                    _unitsInGame.Add(newUnit);
                    float3 newPosition = new float3(Random.Range(-_maxSpawnPos.x, _maxSpawnPos.x), 0, Random.Range(-_maxSpawnPos.y, _maxSpawnPos.y));
                    _entityManager.SetComponentData(newUnit, new Translation {Value = newPosition});
                }
            }

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
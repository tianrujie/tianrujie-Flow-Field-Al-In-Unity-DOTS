using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Director1 : MonoBehaviour
{
    public Transform tarTransform;
    public bool AutoRandomTargetPos = false;
    private static Director1 _ins;
    private EntityManager _entityManager;
    public static Director1 Ins
    {
        get => _ins;
        set => _ins = value;
    }
    
    
    private void Awake()
    {
        Ins = this;
        SharedDataContainer.Init();
        SharedDataContainer.InitMapGrid();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        // var world = World.DefaultGameObjectInjectionWorld;
        // var simulationSystemGroup = world.GetExistingSystem<SimulationSystemGroup>();
        // if (world.GetExistingSystem<FollowTargetSystem>() == null)
        // {
        //     var sys = world.CreateSystem<FollowTargetSystem>();
        //     sys.Enabled = true;
        //     simulationSystemGroup.AddSystemToUpdateList(sys);
        // }
        //
        // if (world.GetExistingSystem<FollowTargetSystem>() != null)
        //     world.GetExistingSystem<FollowTargetSystem>().Enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (AutoRandomTargetPos)
        {
            if (((int) Time.time) % 5 == 0)
                tarTransform.position = new Vector3(Random.Range(-90,90),0, Random.Range(-45,45));
        }
    }
    
    private void OnDestroy()
    {
        SharedDataContainer.Clear();
    }
}

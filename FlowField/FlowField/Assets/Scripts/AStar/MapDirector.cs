#define DOTS_MODEL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using ResourceManager = System.Resources.ResourceManager;
using Unity.Transforms;

public class MapNode
{
    public int x;
    public int y;
    public GameObject nodeObj;
    public Renderer renderer;
}

public class MapDirector : MonoBehaviour
{
    public  GameObject _unitRes;
    public  GameObject _root;
    public Text _text;
    
    public  Vector2Int MapSize;
    
    public Dictionary<int,Dictionary<int,MapNode>> mapNodes = new Dictionary<int, Dictionary<int, MapNode>>();

    //public Random random = new Random();

    public AStarPathFinder finder;
    public List<int> alertCells = new List<int>();
    public List<int> boomCells = new List<int>();
    
    private static MapDirector _instance;

    public static MapDirector Instance
    {
        get { return _instance; }
    }

    public int TOneDimessionParam;
    public int2 TTwoDimessionParam;
    public int TOneDimessionParamBackUp;
    public int2 TTwoDimessionParamBackUp;
    private void Awake()
    {
        _instance = this;
        finder = new AStarPathFinder();
        
#if !UNITY_EDITOR
            Unity.Entities.DefaultWorldInitialization.Initialize("WodWorld", false);
#endif

        if (World.DefaultGameObjectInjectionWorld.GetExistingSystem<MapPainterSystem>() == null)
        {
            var PresentationSystemGroup = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PresentationSystemGroup>();
            var mpSystem = World.DefaultGameObjectInjectionWorld.CreateSystem<MapPainterSystem>();
            mpSystem.director = this;
            PresentationSystemGroup.AddSystemToUpdateList(mpSystem);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(InitMapSync());
        InitMap();  
        //StartCoroutine(StartCheckUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void AddressableLoadTest()
    {
        var loadHandle = Addressables.LoadAssetAsync<TextAsset>("conf_a");
        await loadHandle.Task;
        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            TextAsset textAsset = loadHandle.Result;
            Debug.Log("AddressableLoadTest Sucess, Content is :" + textAsset.text);
            _text.text = textAsset.text;
        }
        else if (loadHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("AddressableLoadTest Failed ");
            _text.text = "AddressableLoadTest Failed";
        }
    }

    IEnumerator StartCheckUpdate()
    {   
        //初始化Addressable
        AsyncOperationHandle <IResourceLocator> asyncOperationInitHandle = Addressables.InitializeAsync();
        yield return asyncOperationInitHandle;
        //连接服务器检查更新
        AsyncOperationHandle <List<string>> asyncOperationCheckHandle = Addressables.CheckForCatalogUpdates(false);
        yield return asyncOperationCheckHandle;

        if (asyncOperationCheckHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = asyncOperationCheckHandle.Result;
            Debug.Log("catalogs count:"+catalogs.Count.ToString());
            if (catalogs != null && catalogs.Count > 0)
            {
                //开始更新资源
                AsyncOperationHandle <List<IResourceLocator>> asyncOperationUpdateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return asyncOperationUpdateHandle;
                
                //总大小
                float allsize = 0.0f;
                float downloadsize = 0.0f;
                var IsDownLoad = false;
                //标记有更新文件
                List<IResourceLocator> resourceLocators = asyncOperationUpdateHandle.Result;
                if (resourceLocators.Count > 0)
                {
                    IsDownLoad = true;
                }
                //遍历下载请求
                foreach (var v in resourceLocators)
                {
                    List<object> keys = new List<object>();
                    keys.AddRange(v.Keys);
                    
                    var asyncOperationDownloadHandle = Addressables.GetDownloadSizeAsync(keys);
                    yield return asyncOperationDownloadHandle;
                    
                    long size = asyncOperationDownloadHandle.Result;
                    if (size > 0)
                    {
                        //文件大小
                        float mb = size / 1024.0f / 1024.0f;
                        allsize += mb;
                        float lastloadmb = 0.0f;
                        //开始下载
                        var downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union,false);
                        while (!downloadHandle.IsDone)
                        {
                            //进度计算
                            downloadsize += (mb * downloadHandle.PercentComplete - lastloadmb);
                            lastloadmb = mb * downloadHandle.PercentComplete;
                            yield return null;
                        }
                        //下载进度更新
                        downloadsize += (mb - lastloadmb);
                        //释放
                        Addressables.Release(downloadHandle);
                    }
                }
                
                Addressables.Release(asyncOperationUpdateHandle);
            }
            
            Addressables.Release(asyncOperationCheckHandle);
            
            System.GC.Collect();
            AddressableLoadTest();
        }
        else
        {
            Debug.LogError("CheckforCatalogUpdate failed! " + asyncOperationCheckHandle.DebugName);
        }
    }
    
    public IEnumerator InitMapSync()
    {
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
#if DOTS_MODEL
                CreatANodeEntitiy(i + 1, j + 1);
#else
                CreatANodeGameObject(i + 1, j + 1);
                yield return new WaitForSeconds(0.02f);
#endif
            }
        }
        
        yield return null;
    }
    
    public void InitMap()
    {
        for (int j = 0; j < MapSize.y; j++)
        {
            for (int i = 0; i < MapSize.x; i++)
            {
#if DOTS_MODEL
                CreatANodeEntitiy(i + 1, j + 1);
#else
                CreatANodeGameObject(i + 1, j + 1);
#endif
                
            }
        }
    }

    public void CreatANodeGameObject(int x, int y)
    {
        ///Debug.Log("creat " + x + " " + y);
        MapNode node = new MapNode();
        node.x = x;
        node.y = y;
        node.nodeObj = GameObject.Instantiate(_unitRes);
        node.nodeObj.transform.parent = _root.transform;
        node.nodeObj.transform.localPosition = GetPos(x,y);
        node.nodeObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        node.renderer = node.nodeObj.GetComponent<Renderer>();
        node.renderer.material.SetColor("_MainColor",RandomAColor());
        
        if (!mapNodes.ContainsKey(x))
            mapNodes.Add(x,new Dictionary<int, MapNode>());
        if (!mapNodes[x].ContainsKey(y))
            mapNodes[x].Add(y,node);
    }
    
    public void CreatANodeEntitiy(int x, int y)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var fristCell = entityManager.CreateEntity(AssetsDirctor.Instance.CellArchetype);
        entityManager.SetSharedComponentData(fristCell,AssetsDirctor.Instance.MeshInstanceRenderer);
        entityManager.SetComponentData(fristCell,new Translation(){Value = GetPos(x,y)});
        entityManager.SetComponentData(fristCell,new Rotation(){Value = Quaternion.Euler(90,  0, 0)});
        entityManager.SetComponentData(fristCell,new ColorWrapper(){Value = Color.gray});
        entityManager.SetComponentData(fristCell,new LocalToWorld());
        entityManager.SetComponentData(fristCell,new Scale(){Value = 0.9f});
        entityManager.SetComponentData(fristCell,new CellState()
        {
            Index = finder.OneDimensionIndex(x - 1,y - 1),
            State = Const.Common
        });
    }
    
    public Color RandomAColor()
    {
        return new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),0.5f);
    }

    public Vector3 GetPos(int x, int y)
    {
        return new Vector3(x - MapSize.x/2,0,y - MapSize.y/2);
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        
        // if (GUILayout.Button("Test Case", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     AStarPathFinder finder = new AStarPathFinder();
        //     Debug.Log($"TOneDimessionParam {TOneDimessionParam}  TTwoDimessionParam {TTwoDimessionParam} ");
        //     
        //     finder.TwoDimensionIndex(TOneDimessionParam, out var x, out var y);
        //     var index = finder.OneDimensionIndex(TTwoDimessionParam.x, TTwoDimessionParam.y);
        //     Debug.Log($" TOneDimessionParam {TOneDimessionParam} to [{x},{y}] ---- TTwoDimessionParam {TTwoDimessionParam} to [{index}]");
        //
        //     Debug.Log($" TOneDimessionParam  is in IsInRange(one) {finder.IsInRange(TOneDimessionParam)} IsInRange(two) {finder.IsInRange(x,y)} !");
        //     Debug.Log($" TTwoDimessionParam  is in IsInRange(one) {finder.IsInRange(index)} IsInRange(two) {finder.IsInRange(TTwoDimessionParam.x, TTwoDimessionParam.y)} !");
        // }
        //
        // if (GUILayout.Button("GetNeighbors", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     AStarPathFinder finder = new AStarPathFinder();
        //     var index = finder.OneDimensionIndex(TTwoDimessionParam.x, TTwoDimessionParam.y);
        //     List<int> neighbours = new List<int>();
        //     finder.GetNeighbors(index, ref neighbours);
        //     StringBuilder sb = new StringBuilder();
        //     sb.Append($" [{TTwoDimessionParam.x},{TTwoDimessionParam.y}]`s neighbores: \n");
        //     for (int i = 0; i < neighbours.Count; i++)
        //     {
        //         var n = neighbours[i];
        //         finder.TwoDimensionIndex(n,out var a,out var b);
        //         sb.Append($" [{a},{b}] ");
        //     }
        //     Debug.Log(sb.ToString());
        // }
        //
        // if (GUILayout.Button("Heuristic", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     AStarPathFinder finder = new AStarPathFinder();
        //     Debug.Log($"Heuristic of TOneDimessionParam {TOneDimessionParam} to  {TOneDimessionParamBackUp} cost: {finder.Heuristic(TOneDimessionParam, TOneDimessionParamBackUp)} !");
        // }

        // if (GUILayout.Button("TestMap", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     alertCells.Clear();
        //     boomCells.Clear();
        //     var point = finder.OneDimensionIndex(TTwoDimessionParam.x, TTwoDimessionParam.y);
        //     boomCells.Add(point);
        // }
        if (GUILayout.Button("FindPath", GUILayout.Height(50), GUILayout.Width(100)))
        {
            // MinHeap<MinHeapNode> frontier = new MinHeap<MinHeapNode>();
            // frontier.Add(new MinHeapNode()
            // {
            //     index = 1,
            //     cost = 10,
            // });
            //
            // frontier.Add(new MinHeapNode()
            // {
            //     index = 2,
            //     cost = 5,
            // });
            // frontier.Add(new MinHeapNode()
            // {
            //     index = 3,
            //     cost = 12,
            // });
            //
            // var size = frontier.GetSize();
            // for (int i = 0; i < size; i++)
            // {
            //     var min = frontier.PopMin();
            //     Debug.Log($" Pop index: {min.index}  cost: {min.cost} ");
            // }

            
            List<int> path = new List<int>();
            var start = finder.OneDimensionIndex(TTwoDimessionParam.x, TTwoDimessionParam.y);
            var stop = finder.OneDimensionIndex(TTwoDimessionParamBackUp.x, TTwoDimessionParamBackUp.y);
            finder.FindPath(start, stop, ref path);

            alertCells.Clear();
            boomCells.Clear();

            var length = finder.frontier.GetSize();
            for (int i = 0; i < length; i++)
            {
                var node = finder.frontier.PopMin();
                alertCells.Add(node.index);
            }

            for (int i = 0; i < path.Count; i++)
            {
                boomCells.Add(path[i]);
            }
            
            Debug.Log("FindPath Clicked!");
            
        }
        //
        // if (GUILayout.Button("FindPath Step+", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     Debug.Log("FindPath Step+!");
        // }
        //
        //
        // if (GUILayout.Button("FindPath Step-", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     Debug.Log("FindPath Step-!");
        // }
        //
        // if (GUILayout.Button("ClearPath", GUILayout.Height(50), GUILayout.Width(100)))
        // {
        //     Debug.Log("ClearPath!");
        // }
        
        GUILayout.EndVertical();
    }
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AStarPathFinder
{
    public MinHeap<MinHeapNode> frontier = new MinHeap<MinHeapNode>();
    private Dictionary<int, int> came_from = new Dictionary<int, int>();
    private Dictionary<int, float> cost_so_far = new Dictionary<int, float>();
    public List<int> close_list = new List<int>();
    private List<int> obstacles = new List<int>();
    private List<int> neighbours = new List<int>();
    
    /// <summary>
    /// 寻找start to end 的相对较优路径
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="pathNodes">返回的路点列表</param>
    /// <returns></returns>
    public bool FindPath(int start, int end, ref List<int> pathNodes)
    {
        Clear();
        
        if (start.Equals(end))
            return true;
        
        //初始化节点
        frontier.Add(new MinHeapNode()
        {
            index = start,
        });
        came_from.Add(start,-1);
        cost_so_far.Add(start,0);

        bool bSucess = false;
        MinHeapNode current = new MinHeapNode();
        while (frontier.GetSize() > 0)
        {
            current = frontier.PopMin();
            Debug.Log($"Pop [{current.index}]");

            //找到终点，
            if (current.index == end)
            {
                bSucess = true;
                break;
            }
            
            //加入closelist
            close_list.Add(current.index);
            
            //遍历邻居节点
            GetNeighbors(current.index,ref neighbours);
            for (int i = 0; i < neighbours.Count; i++)
            {
                var next = neighbours[i];
                //跳过已遍历节点、跳过阻挡
                if(close_list.Contains(next) || IsObstacle(next))
                    continue;
                                
                //记录新节点
                if (!came_from.ContainsKey(next))
                {
                    came_from.Add(next,current.index);
                    cost_so_far.Add(next,cost_so_far[current.index] + GraphicsCost(current.index, next));
                    frontier.Add(new MinHeapNode()
                    {
                        index = next,
                        cost = cost_so_far[next] + Heuristic(next, end)
                    });
                    Debug.Log($"Add Node next:{next} cost_so_far: {cost_so_far[next]} f(next): {cost_so_far[next] + Heuristic(next, end)}");
                }
                else
                {
                    //修正已经在frontier中的next 最佳cost
                    var new_cost = cost_so_far[current.index] + GraphicsCost(current.index, next);
                    if (cost_so_far[next] > new_cost)
                    {
                        cost_so_far[next] = new_cost;
                        came_from[next] = current.index;
                        frontier.Add(new MinHeapNode()
                        {
                            index = next,
                            cost = cost_so_far[next] + Heuristic(next, end)
                        });
                        
                        Debug.Log($"Update Node cost of {next} cost_so_far: {cost_so_far[next]} f(next): {new_cost + Heuristic(next, end)}");

                    }
                }
                    
            }
        }

        //生成路径
        if (bSucess && current != null)
        {
            int curIdx = current.index;
            while (came_from.TryGetValue(curIdx,out var parent))
            {
                pathNodes.Insert(0,curIdx);
                if (parent == -1)
                {
                    pathNodes.Insert(0,parent);
                    break;
                }
                
                curIdx = parent;
            }
        }
        
        
        return bSucess;
    }

    /// <summary>
    /// 启发式估值方法
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public float Heuristic(int a,int b)
    {
        TwoDimensionIndex(a, out var x1, out var y1);
        TwoDimensionIndex(b, out var x2, out var y2);
        
        return math.abs(x1 - x2) + math.abs(y1 - y2);    //曼哈顿
        //return math.max(math.abs(x1 - x2), math.abs(y1 - y2));    //切比雪夫
        //return math.pow(math.abs(x1 - x2), 2) + math.pow(math.abs(y1 - y2), 2);//欧式
    }

    public float GraphicsCost(int a,int b)
    {
        TwoDimensionIndex(a, out var x1, out var y1);
        TwoDimensionIndex(b, out var x2, out var y2);

        if (x1 != x2 && y1 != y2)
            return math.SQRT2;
        
        return 1f;
    }

    /// <summary>
    /// 一维坐标转二维
    /// </summary>
    /// <param name="index"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void TwoDimensionIndex(int index, out int x, out int y)
    {
        x = index % MapDirector.Instance.MapSize.x;
        y = index / MapDirector.Instance.MapSize.x;
    }

    /// <summary>
    /// 二维坐标转一维
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="index"></param>
    public int OneDimensionIndex(int x,int y)
    {
        return  x + y * MapDirector.Instance.MapSize.x;
    }
    
    /// <summary>
    /// 获取邻居节点
    /// </summary>
    /// <param name="index"></param>
    /// <param name="neighbours"></param>
    public void GetNeighbors(int index, ref List<int> neighbours)
    {
        if (neighbours == null)
            neighbours.Clear();
        
        neighbours.Clear();
        if(!IsInRange(index))
            return;

        TwoDimensionIndex(index, out var x, out var y);
        if (IsInRange(x-1,y-1))
            neighbours.Add(OneDimensionIndex(x-1, y-1));
        if (IsInRange(x,y-1))
            neighbours.Add(OneDimensionIndex(x,y-1));
        if (IsInRange(x+1,y-1))
            neighbours.Add(OneDimensionIndex(x+1,y-1));
        if (IsInRange(x+1,y))
            neighbours.Add(OneDimensionIndex(x+1,y));
        if (IsInRange(x+1,y+1))
            neighbours.Add(OneDimensionIndex(x+1,y+1));
        if (IsInRange(x,y+1))
            neighbours.Add(OneDimensionIndex(x,y+1));
        if (IsInRange(x-1,y+1))
            neighbours.Add(OneDimensionIndex(x-1,y+1));
        if (IsInRange(x-1,y))
            neighbours.Add(OneDimensionIndex(x-1,y));
    }

    /// <summary>
    /// 是否在合法范围内
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool IsInRange(int index)
    {
        return index >= 0 && index < MapDirector.Instance.MapSize.x * MapDirector.Instance.MapSize.y;
    }
    
    /// <summary>
    /// 是否在合法范围内
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsInRange(int x,int y)
    {
        return x >= 0 && x < MapDirector.Instance.MapSize.x && y >= 0 && y < MapDirector.Instance.MapSize.y;
    }

    public void Clear()
    {
        frontier.Clear();
        came_from.Clear();
        cost_so_far.Clear();
        close_list.Clear();
        obstacles.Clear();
        neighbours.Clear();

    }
    
    private bool IsObstacle(int index)
    {
        return obstacles.Contains(index);
    }
}
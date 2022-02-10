using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace TMG.ECSFlowField
{
    [AlwaysUpdateSystem]
    public class FlowFieldGenerateSystem : SystemBase
    {
        private EntityCommandBufferSystem _ecbSystem;
        private NativeQueue<int> checkList;
        private NativeList<int> neighbours;
        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
            checkList = new NativeQueue<int>(Allocator.Persistent);
            neighbours = new NativeList<int>(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            if (checkList.IsCreated)
                checkList.Clear();
            if (neighbours.IsCreated) 
                neighbours.Clear();
            
            //update block
            //@todo
            
            //for every target

            //Update Cell`s BestValue
            var cellsData = SharedDataContainer.Cells;
            if (SharedDataContainer.IdxLegal(SharedDataContainer.TargetCell))
            {
                //reset the targetCell`s Data
                var destination = cellsData[SharedDataContainer.TargetCell];
                destination.BestCost = 0;
                destination.BestDir = float2.zero;
                destination.TargetCellVersion = SharedDataContainer.TargetCellVersion;
                cellsData[SharedDataContainer.TargetCell] = destination;
                
                checkList.Enqueue(SharedDataContainer.TargetCell);
                while (checkList.Count > 0)
                {
                    var curCellIdx = checkList.Dequeue();
                    var curCellData = cellsData[curCellIdx];
                    neighbours.Clear();
                    
                    //update Neighbour`s BestCost
                    SharedDataContainer.GetNeighbours(curCellData.I_2T,ref neighbours);
                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        var neighbourIdx = neighbours[i];
                        var neighbourData = cellsData[neighbourIdx];
                        
                        //Block不进行遍历
                        if (neighbourData.IsBlock)
                            continue;
                        
                        //更换目标后重置BestCost
                        if (neighbourData.TargetCellVersion != SharedDataContainer.TargetCellVersion)
                        {
                            neighbourData.TargetCellVersion = SharedDataContainer.TargetCellVersion;
                            neighbourData.BestCost = int.MaxValue;
                        }
                        
                        if (curCellData.BestCost + neighbourData.Cost < neighbourData.BestCost)
                        {
                            neighbourData.BestCost = curCellData.BestCost + neighbourData.Cost;
                            cellsData[neighbourIdx] = neighbourData;
                            checkList.Enqueue(neighbourIdx);
                        }
                    }
                    
                    //update curnode best dir
                     var nei = Const1.Neighbours4Dir;
                     int blockState = 0;
                     int leftCellIdx =
                         SharedDataContainer.Index2To1(curCellData.I_2T.x + nei[0].x, curCellData.I_2T.y + nei[0].y);
                     var leftCellDataBestCost = SharedDataContainer.IdxLegal(leftCellIdx) ? cellsData[leftCellIdx].BestCost : curCellData.BestCost;
                     if (AdaptBlock(ref leftCellDataBestCost,curCellData.BestCost))
                         blockState = blockState | Const1.BL;
                     
                     int rightCellIdx =
                         SharedDataContainer.Index2To1(curCellData.I_2T.x + nei[1].x, curCellData.I_2T.y + nei[1].y);
                     var rightCellDataBestCost = SharedDataContainer.IdxLegal(rightCellIdx) ? cellsData[rightCellIdx].BestCost : curCellData.BestCost;
                     if (AdaptBlock(ref rightCellDataBestCost, curCellData.BestCost))
                         blockState = blockState | Const1.BR;
                     
                     int topCellIdx =
                         SharedDataContainer.Index2To1(curCellData.I_2T.x + nei[2].x, curCellData.I_2T.y + nei[2].y);
                     var topCellDataBestCost = SharedDataContainer.IdxLegal(topCellIdx) ? cellsData[topCellIdx].BestCost : curCellData.BestCost;
                     if (AdaptBlock(ref topCellDataBestCost, curCellData.BestCost))
                         blockState = blockState | Const1.BT;
                     
                     int bottomCellIdx =
                         SharedDataContainer.Index2To1(curCellData.I_2T.x + nei[3].x, curCellData.I_2T.y + nei[3].y);
                     var bottomCellDataBestCost = SharedDataContainer.IdxLegal(bottomCellIdx) ? cellsData[bottomCellIdx].BestCost : curCellData.BestCost;
                     if (AdaptBlock(ref bottomCellDataBestCost, curCellData.BestCost))
                         blockState = blockState | Const1.BB;
                     
                     if (curCellIdx != SharedDataContainer.TargetCell)
                     {
                         //与目标点十字交叉的地图边界点,舍弃丢失边界方向的速度计算
                         //其余地图边界点丢失方向BestCost取curCellData.BestCost
                         bool isCrossTCol = curCellData.I_2T.x == destination.I_2T.x;
                         bool isCrossTRow = curCellData.I_2T.y == destination.I_2T.y;
                         bool missLeftOrRight = leftCellIdx == -1 || rightCellIdx == -1;
                         bool missTopOrBottom = topCellIdx == -1 || bottomCellIdx == -1;
                         
                         bool resetGradientX = (isCrossTCol && missLeftOrRight);
                         bool resetGradientY = (isCrossTRow && missTopOrBottom);
                                                                                     
                         curCellData.BestDir = new float2( resetGradientX ? 0 :leftCellDataBestCost - rightCellDataBestCost,
                             resetGradientY ? 0 :topCellDataBestCost - bottomCellDataBestCost);
                         
                         OptimizeBlockDir(ref curCellData.BestDir,blockState);
                     }
                     else
                     {
                         curCellData.BestDir = float2.zero; 
                     }
                     
                     cellsData[curCellIdx] = curCellData;
                }
            }
            
            //EntityCommandBuffer commandBuffer = _ecbSystem.CreateCommandBuffer();
            // Entities.ForEach((Entity entity) =>
            // {
            //     GridDebug.instance.AddToList(cellData);
            //     commandBuffer.RemoveComponent<AddToDebugTag>(entity);
            // }).Run();
        }

        /// <summary>
        /// 相邻Cell为阻挡的情况下，该方向邻居的BestCost使用CurNode填充
        /// </summary>
        /// <param name="cost"></param>
        /// <param name="curNodeBestCost"></param>
        private bool AdaptBlock(ref int cost, int curNodeBestCost)
        {
            if (cost == Const1.BlockCost)

            {
                cost = curNodeBestCost;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除BestDir在Block方向上的速度分量
        /// </summary>
        /// <param name="bestDir"></param>
        /// <param name="bl"></param>
        private void OptimizeBlockDir(ref float2 bestDir, int bl)
        {
            if ((bl & Const1.BL) != 0 && bestDir.x < 0)
                bestDir.x = 0;
            
            if ((bl & Const1.BR) != 0 && bestDir.x > 0)
                bestDir.x = 0;

            if ((bl & Const1.BT) != 0 && bestDir.y < 0)
                bestDir.y = 0;
            
            if ((bl & Const1.BB) != 0 && bestDir.y > 0)
                bestDir.y = 0;
        }

        protected override void OnDestroy()
        {
            //Dispose Container
            if (checkList.IsCreated)
                checkList.Dispose();
            if (neighbours.IsCreated)
                neighbours.Dispose();
        }
    }
}
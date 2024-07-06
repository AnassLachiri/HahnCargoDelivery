using HahnCargoDelivery.Models;

namespace HahnCargoDelivery.Helpers;

public class DjikstraHelper
{
    
    public static List<int> GetShortestPath(Grid grid, int startNodeId, int endNodeId)
    {
        var distances = grid.Nodes.ToDictionary(node => node.Id, node => int.MaxValue);
        var previous = new Dictionary<int, int?>();
        var unvisited = new HashSet<int>(grid.Nodes.Select(node => node.Id));

        distances[startNodeId] = 0;

        while (unvisited.Count > 0)
        {
            var currentNodeId = unvisited.OrderBy(node => distances[node]).First();
            unvisited.Remove(currentNodeId);

            if (currentNodeId == endNodeId)
                break;

            foreach (var neighborId in GridHelper.GetNeighbors(grid, currentNodeId))
            {
                if (!unvisited.Contains(neighborId))
                    continue;

                var tentativeDistance = distances[currentNodeId] + GridHelper.GetEdgeCost(grid, currentNodeId, neighborId);

                if (tentativeDistance < distances[neighborId])
                {
                    distances[neighborId] = tentativeDistance;
                    previous[neighborId] = currentNodeId;
                }
            }
        }

        var path = new List<int>();
        int? current = endNodeId;
        while (current.HasValue)
        {
            path.Insert(0, current.Value);
            previous.TryGetValue(current.Value, out current);
        }

        return path;
    }

    public static int GetTotalCost(Grid grid, List<int> path)
    {
        int totalCost = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalCost += GridHelper.GetEdgeCost(grid, path[i], path[i + 1]);
        }
        return totalCost;
    }
    
    public static TimeSpan GetTotalTime(Grid grid, List<int> path)
    {
        TimeSpan totalTime = TimeSpan.Zero;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var timeString = GridHelper.GetEdgeTime(grid, path[i], path[i + 1]);
            totalTime += timeString;
        }
        return totalTime;
    }
}
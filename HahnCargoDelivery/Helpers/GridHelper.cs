using HahnCargoDelivery.Models;

namespace HahnCargoDelivery.Helpers;

public class GridHelper
{
    public static List<int> GetNeighbors(Grid grid, int nodeId)
    {
        return grid.Connections
            .Where(c => c.FirstNodeId == nodeId || c.SecondNodeId == nodeId)
            .Select(c => c.FirstNodeId == nodeId ? c.SecondNodeId : c.FirstNodeId)
            .ToList();
    }

    public static int GetEdgeCost(Grid grid, int nodeId1, int nodeId2)
    {
        var connection = grid.Connections
            .FirstOrDefault(c => (c.FirstNodeId == nodeId1 && c.SecondNodeId == nodeId2) ||
                                 (c.FirstNodeId == nodeId2 && c.SecondNodeId == nodeId1));
        if (connection == null)
            throw new Exception("Connection not found!");

        var edge = grid.Edges.FirstOrDefault(e => e.Id == connection.EdgeId);
        if (edge == null)
            throw new Exception("Edge not found!");

        return edge.Cost;
    }
    
    public static TimeSpan GetEdgeTime(Grid grid, int nodeId1, int nodeId2)
    {
        var connection = grid.Connections
            .FirstOrDefault(c => (c.FirstNodeId == nodeId1 && c.SecondNodeId == nodeId2) ||
                                 (c.FirstNodeId == nodeId2 && c.SecondNodeId == nodeId1));
        if (connection == null)
            throw new Exception("Connection not found!");

        var edge = grid.Edges.FirstOrDefault(e => e.Id == connection.EdgeId);
        if (edge == null)
            throw new Exception("Edge not found!");

        return edge.Time;
    }
}
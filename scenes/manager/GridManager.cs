using System.Collections.Generic;
using System.Linq;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	public const int GRID_SIZE = 64;
	
	private readonly HashSet<Vector2I> _occupiedCells = new();
	
	[Export] private TileMapLayer _highlightTileMapLayer;
	[Export] private TileMapLayer _baseTerrainTileMapLayer; 
	
	public override void _Ready()
	{
	}

	public bool IsTilePositionValid(Vector2I tilePostion)
	{
		var customData = _baseTerrainTileMapLayer.GetCellTileData(tilePostion);

		if (customData == null) return false;
		if (!(bool)customData.GetCustomData("buildable")) return false;
		
		return !_occupiedCells.Contains(tilePostion);
	}

	public void MarkTileAsOccupied(Vector2I position) => _occupiedCells.Add(position);

	public void HighlightBuildableTiles()
	{
		ClearHighlightedTiles();
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

		foreach (var buildingComponent in buildingComponents)
		{
			HighlightValidTilesRadius(buildingComponent.GetGridCellComponent(), buildingComponent.BuildableRadius);
		}
	}

	public void ClearHighlightedTiles()
	{
		_highlightTileMapLayer.Clear();
	}
	
	public Vector2I GetMouseGridCellPosition()
	{
		var mousePos = _highlightTileMapLayer.GetGlobalMousePosition();
		var gridPosition = mousePos / GRID_SIZE;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}
	
	private void HighlightValidTilesRadius(Vector2I rootCell, int radius)
	{
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var titlePostion = new Vector2I(x, y);
				if (!IsTilePositionValid(titlePostion)) continue;
				
				_highlightTileMapLayer.SetCell(titlePostion, 1, Vector2I.Zero);
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	public const int GRID_SIZE = 64;
	
	private readonly HashSet<Vector2I> _validBuildableTiles = new();
	
	[Export] private TileMapLayer _highlightTileMapLayer;
	[Export] private TileMapLayer _baseTerrainTileMapLayer; 
	
	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
	}

	// Cái này để check xem cái tile này có build được hay không: Sand, Ground
	public bool IsTilePositionValid(Vector2I tilePostion)
	{
		var customData = _baseTerrainTileMapLayer.GetCellTileData(tilePostion);

		if (customData == null) return false;
		return (bool)customData.GetCustomData("buildable");
	}

	// Cái này thì kiểm tra xem trong cái tilePosition nó đang ở trong state có thể build được
	public bool IsTilePositionBuildable(Vector2I tilePosition) => _validBuildableTiles.Contains(tilePosition);

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in _validBuildableTiles)
		{
			_highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		ClearHighlightedTiles();
		HighlightBuildableTiles();
		
		var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(_validBuildableTiles).Except(GetOccupiedTiles());
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in expandedTiles)
		{
			_highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
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

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellComponent();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildableRadius);
		_validBuildableTiles.UnionWith(validTiles);
		_validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponentsInGroup = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		var occupiedTiles = buildingComponentsInGroup.Select(x => x.GetGridCellComponent());
		return occupiedTiles;
	}
	
	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		var result = new List<Vector2I>();
		
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var titlePostion = new Vector2I(x, y);
				if (!IsTilePositionValid(titlePostion)) continue;
				result.Add(titlePostion);
			}
		}

		return result;
	}
	
	private void OnBuildingPlaced(BuildingComponent buildingcomponent)
	{
		UpdateValidBuildableTiles(buildingcomponent);
	}
}

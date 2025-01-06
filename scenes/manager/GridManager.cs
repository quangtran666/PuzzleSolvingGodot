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
			_highlightTileMapLayer.SetCell(tilePosition, 1, Vector2I.Zero);
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
		
		for (var x = rootCell.X - buildingComponent.BuildableRadius; x <= rootCell.X + buildingComponent.BuildableRadius; x++)
		{
			for (var y = rootCell.Y - buildingComponent.BuildableRadius; y <= rootCell.Y + buildingComponent.BuildableRadius; y++)
			{
				var titlePostion = new Vector2I(x, y);
				if (!IsTilePositionValid(titlePostion)) continue;
				_validBuildableTiles.Add(titlePostion);
			}
		}

		// Prevent to add the same tile twice in the placed building
		_validBuildableTiles.Remove(rootCell);
	}
	
	private void OnBuildingPlaced(BuildingComponent buildingcomponent)
	{
		UpdateValidBuildableTiles(buildingcomponent);
	}
}

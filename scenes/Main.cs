using System.Collections.Generic;
using Godot;

namespace Game;

public partial class Main : Node
{
    private const int GRID_SIZE = 64;
    
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;
    private TileMapLayer _highlightTileMapLayer;

    private Vector2? _hoveredGridCell;
    private readonly HashSet<Vector2> _occupiedCells = new();
    
    public override void _Ready()                           
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("Place Building Button");
        _highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

        _cursor.Visible = false;
        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (_hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && !_occupiedCells.Contains(_hoveredGridCell.Value))
        {
            PlaceBuildingAtHoverCellPosition();
            _cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        _cursor.GlobalPosition = gridPosition * GRID_SIZE;

        if (_cursor.IsVisible() && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition))
        {
            _hoveredGridCell = gridPosition;
            UpdateHighlightTileMapLayer();
        }
    }

    private Vector2 GetMouseGridCellPosition()
    {
        var mousePos = _highlightTileMapLayer.GetGlobalMousePosition();
        var gridPosition = mousePos / GRID_SIZE;
        gridPosition = gridPosition.Floor();
        return gridPosition;
    }
    
    private void PlaceBuildingAtHoverCellPosition()
    {
        if (!_hoveredGridCell.HasValue) return;
        
        var building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);
        
        building.GlobalPosition = _hoveredGridCell.Value * GRID_SIZE;
        _occupiedCells.Add(_hoveredGridCell.Value);

        _hoveredGridCell = null;
        UpdateHighlightTileMapLayer();
    }
    
    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    } 

    private void UpdateHighlightTileMapLayer()
    {
        _highlightTileMapLayer.Clear();
        
        if (!_hoveredGridCell.HasValue) return;

        for (var x = _hoveredGridCell.Value.X - 3; x <= _hoveredGridCell.Value.X + 3; x++)
        {
            for (var y = _hoveredGridCell.Value.Y - 3; y <= _hoveredGridCell.Value.Y + 3; y++)
            {
                _highlightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 1, Vector2I.Zero);
            }
        }
    }
}

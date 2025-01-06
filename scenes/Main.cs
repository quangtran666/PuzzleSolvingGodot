using System.Collections.Generic;
using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    private GridManager _gridManager;
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;

    private Vector2I? _hoveredGridCell;
    
    public override void _Ready()                           
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _gridManager = GetNode<GridManager>("GridManager");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("Place Building Button");

        _cursor.Visible = false;
        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (_hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && _gridManager.IsTilePositionBuildable(_hoveredGridCell.Value))
        {
            PlaceBuildingAtHoverCellPosition();
            _cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = _gridManager.GetMouseGridCellPosition();
        _cursor.GlobalPosition = gridPosition * 64;

        if (_cursor.IsVisible() && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition))
        {
            _hoveredGridCell = gridPosition;
            _gridManager.HighlightBuildableTiles();
        }
    }
    
    private void PlaceBuildingAtHoverCellPosition()
    {
        if (!_hoveredGridCell.HasValue) return;
        
        var building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);
        building.GlobalPosition = _hoveredGridCell.Value * 64;
        
        _hoveredGridCell = null;
        _gridManager.ClearHighlightedTiles();
    }
    
    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    } 
}

using Game.Autoload;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
    [Export] public int BuildableRadius { get; private set; }
    
    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
        // Cái này đơn giản sẽ dừng lại lời gọi hàm cho tới cái frame tiếp theo
        Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
    }

    public Vector2I GetGridCellComponent()
    {
        var gridPosition = GlobalPosition / 64;
        gridPosition = gridPosition.Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}

using Godot;
using System;

public partial class Character : Node2D
{
	[Export]
	public float Speed { get; set; } = 200.0f;
	
	private Vector2 _velocity = Vector2.Zero;
	public Vector2 _target;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_target = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_target != Position)
		{
			Rotation = _target.AngleToPoint(Position) + Mathf.Pi;
			Position = Position.MoveToward(_target, (float)delta * Speed);
		}	
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				_target = GetGlobalMousePosition();
			}
		}
	}
}

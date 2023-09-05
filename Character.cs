using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Character : Node2D
{
	[Export]
	public float Speed { get; set; } = 200.0f;

	[Signal]
	public delegate void TurnEndedEventHandler();
	 
	private const float Mass = 10;
	private const float DistanceToArrival = 10;
	
	private Vector2 _velocity = Vector2.Zero;
	public Vector2 _target;
	private Vector2 _nextPoint = Vector2.Zero;
	private List<Vector2> _path;
	private PathFinder _pathFinder = new PathFinder();
	private bool _isActive = false;
	
	public enum CharacterState { Idle, Searching, Moving };
	public CharacterState State { get; set; } = CharacterState.Idle;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_target = Position;
		_pathFinder = GetTree().Root.GetChild(0).GetNode<PathFinder>("TileMap");
		ChangeState(CharacterState.Idle);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (State != CharacterState.Moving)
			return;

		var arrivedToNextPoint = MoveTo(_nextPoint);
		if (arrivedToNextPoint)
		{
			_path.RemoveAt(0);
			if (!_path.Any())
			{
				ChangeState(CharacterState.Idle);
				EmitSignal(SignalName.TurnEnded);
				return;
			}
			_nextPoint = _path[0];
		}
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_isActive)
			return;
			
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			_target = GetGlobalMousePosition();
			if (_pathFinder.IsPointWalkable(_target) && State != CharacterState.Moving)
			{
				ChangeState(CharacterState.Searching);
				QueueRedraw();
			}
		}
		if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				_target = GetGlobalMousePosition();
				var label = GetTree().Root.GetChild(0).GetNode<Label>("Label");
				label.Text = _target.ToString();
				GD.Print(_target.ToString());
				if (_pathFinder.IsPointWalkable(_target))
					ChangeState(CharacterState.Moving);
			}
		}
	}

	private void ChangeState(CharacterState state)
	{
		if (state == CharacterState.Idle)
		{
			_pathFinder.ClearPath();
		}
		else if (state == CharacterState.Searching || state == CharacterState.Moving)
		{
			_path = _pathFinder.FindPath((Vector2I)Position, (Vector2I)_target).ToList();
			if (_path.Count < 2)
			{
				ChangeState(CharacterState.Idle);
				return;
			}
			_nextPoint = _path[0];
		}
		State = state;
	}

	private bool MoveTo(Vector2 position)
	{
		var desiredVelocity = (position - Position).Normalized() * Speed;
		var steering = desiredVelocity - _velocity;
		_velocity += steering / Mass;
		Position += new Vector2(_velocity.X * (float)GetProcessDeltaTime(), _velocity.Y * (float)GetProcessDeltaTime());
		Rotation = _velocity.Angle();
		return Position.DistanceTo(position) < DistanceToArrival;
	}

	public Task PlayTurn()
	{
		return Task.CompletedTask;
	}
	
	public void IsActiveCharacter(Node2D activeCharacter)
	{
		_isActive = false;
		if (activeCharacter.Name == Name)
			_isActive = true;
	}
}

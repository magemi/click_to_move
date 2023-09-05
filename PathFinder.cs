using Godot;
using System;
public partial class PathFinder : TileMap
{
	private enum Tile { Obstacle, StartPoint, EndPoint };
	private readonly Vector2 CellSize = new Vector2(64, 64);

	private AStarGrid2D _aStar = new AStarGrid2D();
	private Vector2I _startPoint = Vector2I.Zero;
	private Vector2I _endPoint = Vector2I.Zero;
	private Vector2[] _path;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_aStar.Region = GetUsedRect();
		_aStar.CellSize = CellSize;
		_aStar.Offset = new Vector2(CellSize.X / 2, CellSize.Y / 2);
		_aStar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
		_aStar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
		_aStar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		_aStar.Update();

		foreach (var i in GD.Range(_aStar.Region.Position.X, _aStar.Region.End.X))
		{
			foreach (var j in GD.Range(_aStar.Region.Position.Y, _aStar.Region.End.Y))
			{
				var position = new Vector2I(i, j);
				if (GetCellSourceId(0, position) == (int)Tile.Obstacle)
					_aStar.SetPointSolid(position);
			}
		}
	}

	public override void _Draw()
	{
		if (_path.IsEmpty())
			return;

		var lastPoint = _path[0];
		foreach (var i in GD.Range(0, _path.Length))
		{
			var currentPoint = _path[i];
			DrawLine(lastPoint, currentPoint, Colors.White, 3.0f, true);
			DrawCircle(currentPoint, 6.0f, Colors.White);
			lastPoint = currentPoint;
		}
	}
	
	public Vector2 RoundLocalPosition(Vector2 position)
	{
		return MapToLocal(LocalToMap(position));
	}

	public bool IsPointWalkable(Vector2 position)
	{
		var mapPosition = LocalToMap((Vector2I)ToLocal(position));

		if (_aStar.IsInBoundsv(mapPosition))
			return !_aStar.IsPointSolid(mapPosition);

		return false;
	}

	public void ClearPath()
	{
		if (!_path.IsEmpty())
		{
			Array.Clear(_path);
			EraseCell(0, _startPoint);
			EraseCell(0, _endPoint);
		}
		QueueRedraw();
	}

	public Vector2[] FindPath(Vector2I startPoint, Vector2I endPoint)
	{
		ClearPath();

		_startPoint = LocalToMap(startPoint);
		_endPoint = LocalToMap(endPoint);
		_path = _aStar.GetPointPath(_startPoint, _endPoint);

		if (!_path.IsEmpty())
		{
			SetCell(0, _startPoint, (int)Tile.StartPoint, Vector2I.Zero);
			SetCell(0, _endPoint, (int)Tile.EndPoint, Vector2I.Zero);
		}

		return (Vector2[])_path.Clone();
	}
}

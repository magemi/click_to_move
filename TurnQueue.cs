using Godot;
using System;
using System.Collections.Generic;

public partial class TurnQueue : Node2D
{
	[Signal]
	public delegate void ActiveCharacterChangedEventHandler(Character activeCharacter);
	
	public Character ActiveCharacter { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ActiveCharacter = GetChild<Character>(0);
		EmitSignal(SignalName.ActiveCharacterChanged, ActiveCharacter);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void PlayTurn()
	{
		int nextIndex = (ActiveCharacter.GetIndex() + 1) % GetChildCount();
		ActiveCharacter = GetChild<Character>(nextIndex);
		EmitSignal(SignalName.ActiveCharacterChanged, ActiveCharacter);
	}
}

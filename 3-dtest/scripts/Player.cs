using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement Settings")]
	[Export] public float WalkSpeed = 140.0f;
	[Export] public float RunSpeed = 240.0f;
	
	// Higher Acceleration (2500) = Snappy, light start
	[Export] public float Acceleration = 2500.0f; 
	// Higher Friction (1800) = Stops fast, but with a tiny "weighted" micro-slide
	[Export] public float Friction = 1800.0f;    

	private AnimationPlayer _anim;
	private string _lastFacing = "right";
	private bool _isDancing = false;

	public override void _Ready()
	{
		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
		_anim.Play("player_idle_right");
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		bool isRunning = Input.IsActionPressed("ui_shift");
		bool dancePressed = Input.IsActionJustPressed("ui_accept");

		float targetSpeed = isRunning ? RunSpeed : WalkSpeed;
		Vector2 targetVelocity = inputDir * targetSpeed;

		if (inputDir.Length() > 0)
		{
			// Reaches max speed almost instantly
			Velocity = Velocity.MoveToward(targetVelocity, Acceleration * d);
			_isDancing = false;
		}
		else
		{
			// Stops much faster now, but isn't a "brick wall" 0 speed.
			Velocity = Velocity.MoveToward(Vector2.Zero, Friction * d);
		}

		MoveAndSlide();

		if (inputDir.Length() == 0 && dancePressed) _isDancing = true;

		UpdateAnimations(inputDir, isRunning);
	}

	private void UpdateAnimations(Vector2 inputDir, bool isRunning)
	{
		string currentAnim = _anim.CurrentAnimation;
		string targetAnim;

		if (inputDir.X > 0) _lastFacing = "right";
		else if (inputDir.X < 0) _lastFacing = "left";

		// Reduced threshold to 5.0f so the legs stop exactly when the micro-slide ends
		if (Velocity.Length() > 5.0f) 
		{
			targetAnim = $"player_move_{_lastFacing}";
			_anim.SpeedScale = isRunning ? 1.6f : 1.0f;
		}
		else if (_isDancing)
		{
			targetAnim = "player_idle_dance";
			_anim.SpeedScale = 1.0f;
			if (!_anim.IsPlaying()) _isDancing = false;
		}
		else
		{
			targetAnim = $"player_idle_{_lastFacing}";
			_anim.SpeedScale = 1.0f;
		}

		if (currentAnim != targetAnim)
		{
			_anim.Play(targetAnim);
		}
	}
}

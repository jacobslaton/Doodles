using Godot;
using System;

public class Cell : Node2D
{
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private Sprite sprite = null;
	public CellState[] states = null;

	public Rect2 rect
	{
		get { return new Rect2(sprite.GetRect().Position, sprite.GetRect().Size * Scale); }
	}

	public bool Init(int buffers)
	{
		rng.Randomize();
		sprite = GetNode<Sprite>("Sprite");

		states = new CellState[buffers];
		states[0].isAlive = rng.Randi() % 2 == 0;
		Update(0);
		return true;
	}

	public void Update(int bufferIndex)
	{
		if (states[bufferIndex].isAlive)
		{
			sprite.Show();
		}
		else
		{
			sprite.Hide();
		}
	}
}

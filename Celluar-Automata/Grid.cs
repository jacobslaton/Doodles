using Godot;
using System;
using System.Collections.Generic;

// Pointy side up hex grid
public class Grid : Node2D
{
	[Export]
	private int width = 5;
	[Export]
	private int height = 5;
	[Export]
	private float waitTime = 1.0f;

	private Vector2 cellSize;

	private Timer timer = null;
	private int currentBuffer = 0;
	private int buffersCount = 2;

	private PackedScene sceneCell = (PackedScene)GD.Load("res://CellHex/CellHex.tscn");
	private Dictionary<Tuple<int, int>, Cell> grid = new Dictionary<Tuple<int, int>, Cell>();
	private CellState outOfBoundsState = new CellState();
	private Tuple<int, int>[] neighborhoodOffsets = {
		Tuple.Create( 1, -1),
		Tuple.Create( 1,  0),
		Tuple.Create( 0,  1),
		Tuple.Create(-1,  1),
		Tuple.Create(-1,  0),
		Tuple.Create( 0, -1)
	};

	public override void _Ready()
	{
		// Setup timer
		timer = new Timer();
		timer.Connect("timeout", this, "Step");
		timer.WaitTime = waitTime;
		timer.OneShot = false;
		AddChild(timer);

		// Setup buffersCount
		if (buffersCount < 2)
			buffersCount = 2;

		// Setup grid
		for (int ci = 0; ci < width; ++ci)
		{
			for (int ri = 0; ri < height; ++ri)
			{
				Cell cell = (Cell)sceneCell.Instance();
				cell.Init(buffersCount);
				cell.Scale /= 8;
				cellSize = new Vector2(cell.rect.Size.x * (float)Math.Sqrt(3) / 2.0f, cell.rect.Size.y);
				cell.Position = getCartesianFromOffset(ci, ri);

				AddChild(cell);
				grid.Add(Tuple.Create(ci, ri), cell);
			}
		}

		// // Setup grid
		// for (int ri = 0; ri < width; ++ri)
		// {
		// 	for (int ci = 0; ci < height; ++ci)
		// 	{
		// 		Cell cell = (Cell)sceneCell.Instance();
		// 		cell.Init(buffersCount);
		// 		cell.Scale /= 8;
		// 		cell.Position = getCellPosition(ci, ri);
		// 		cellSize = new Vector2(cell.rect.Size.x * (float)Math.Sqrt(3) / 2.0f, cell.rect.Size.y);
		//
		// 		AddChild(cell);
		// 		grid.Add(Tuple.Create(ci, ri), cell);
		// 	}
		// }


		timer.Start();
	}

	private int getNextBuffer()
	{
		return currentBuffer + 1 >= buffersCount ? 0 : currentBuffer + 1;
	}

	private Tuple<int, int> offsetToAxial(Tuple<int, int> coords)
	{
		return Tuple.Create(
			(int)(coords.Item1 - Math.Floor((coords.Item2 + (coords.Item2 & 1)) / 2.0f)),
			coords.Item2
		);
	}

	private Tuple<int, int> axialToOffset(Tuple<int, int> coords)
	{
		return Tuple.Create(
			(int)(coords.Item1 + Math.Floor((coords.Item2 + (coords.Item2 & 1)) / 2.0f)),
			coords.Item2
		);
	}

	private Vector2 getCartesianFromOffset(int ci, int ri)
	{
		return new Vector2(
			(float)(cellSize.x * (0.5f + ci + 0.5f - 0.5f * (ri % 2))),
			(float)(cellSize.y * (0.5f + 0.75f * ri))
		);
	}

	private CellState[] getNeighborStates(Tuple<int, int> coords)
	{
		Tuple<int, int> coordsAxial = offsetToAxial(coords);
		CellState[] neighborhood = new CellState[neighborhoodOffsets.Length];
		for (int ni = 0; ni < neighborhood.Length; ++ni)
		{
			Tuple<int, int> offset = neighborhoodOffsets[ni];
			Tuple<int, int> key = axialToOffset(Tuple.Create(
				coordsAxial.Item1 + offset.Item1,
				coordsAxial.Item2 + offset.Item2
			));
			if (grid.ContainsKey(key))
				neighborhood[ni] = grid[key].states[currentBuffer];
			else
				neighborhood[ni] = outOfBoundsState;
		}
		return neighborhood;
	}

	public void Step()
	{
		int nextBuffer = getNextBuffer();
		for (int ci = 0; ci < width; ++ci)
		{
			for (int ri = 0; ri < height; ++ri)
			{
				Tuple<int, int> key = Tuple.Create(ci, ri);
				CellState[] neighborhood = getNeighborStates(key);

				// Replacement for proper rule
				int count = 0;
				for (int ni = 0; ni < neighborhood.Length; ++ni)
					if (neighborhood[ni].isAlive)
						++count;
				if (grid[key].states[currentBuffer].isAlive)
					grid[key].states[nextBuffer].isAlive = (count >= 3 && count <= 4);
				else
					grid[key].states[nextBuffer].isAlive = (count == 2);

				grid[key].Update(nextBuffer);
			}
		}
		currentBuffer = nextBuffer;
	}

	private int sfci, sfri = 0;
	public void SlowFill()
	{
		grid[Tuple.Create(sfci, sfri)].states[0].isAlive = true;
		grid[Tuple.Create(sfci, sfri)].Update(0);

		++sfci;
		if (sfci >= width)
		{
			sfci = 0;
			++sfri;
			if (sfri >= height)
			{
				sfri = 0;
			}
		}
	}

	private int ni = 0;
	Tuple<int, int> coordsDebugNeighborhoods = Tuple.Create(3, 10);
	public void DebugNeighborhoods()
	{
		// Debug neighborhoods
		grid[coordsDebugNeighborhoods].states[currentBuffer].isAlive = true;
		grid[coordsDebugNeighborhoods].Update(currentBuffer);

		Tuple<int, int> coordsAxial = offsetToAxial(coordsDebugNeighborhoods);
		Tuple<int, int> offset = neighborhoodOffsets[ni];

		Tuple<int, int> key = Tuple.Create(
			coordsAxial.Item1 + offset.Item1,
			coordsAxial.Item2 + offset.Item2
		);
		key = axialToOffset(key);

		grid[key].states[currentBuffer].isAlive = !grid[key].states[currentBuffer].isAlive;
		grid[key].Update(currentBuffer);

		++ni;
		if (ni >= neighborhoodOffsets.Length)
		{
			ni = 0;
		}
	}
}

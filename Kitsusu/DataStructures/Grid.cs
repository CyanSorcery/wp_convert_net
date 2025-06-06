namespace Kitsusu.DataStructures;

public class Grid<T>
{
	public int Width { get; private set; } = 0;
	public int Height { get; private set; } = 0;
	private T[,] Data = { };

	public Grid(int w, int h, T fill) => Resize(Math.Max(1, w), Math.Max(1, h), fill);

	public T Get(int x, int y) => Data[x, y];
	public T Set(int x, int y, T val) => Data[x, y] = val;

	public string Pack(bool addNewlines = false)
	{
		//Create a string for this grid, optionally with newlines at the end of each row
		//NOTE: This function is only meant to handle ints
		string packed = "";
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				var lookupVal = Get(x, y);
				if (lookupVal is null) return "null";

				if (!int.TryParse(lookupVal.ToString(), out int val))
					throw new Exception("Tried to use pack function with value that wasn't int!");

				packed += val.ToString("x").PadLeft(2, '0');
			}
			if (addNewlines)
				packed += "\r\n";
		}
		return packed;
	}

	public void Resize(int w, int h, T fill)
	{
		//Get the current size of the grid
		int oldW = Width;
		int oldH = Height;
		T[,] oldGrid = Data;

		//Create a new grid of the specified size, and then copy the old grid into it
		Data = new T[w, h];
		Width = w;
		Height = h;
		Fill(fill);

		for (int x = 0; x < oldW; x++)
			for (int y = 0; y < oldH; y++)
				Data[x, y] = oldGrid[x, y];
	}

	//Fill the grid with the given value
	public void Fill(T _fill)
	{
		for (int x = 0; x < Width; x++)
			for (int y = 0; y < Height; y++)
				Data[x, y] = _fill;
	}

	//Copy the given grid into this one
	public void CopyFrom(Grid<T> sourceGrid, int srcX, int srcY, int srcW, int srcH, int dstX, int dstY)
	{
		//Source rectangle boundary checks
		srcX = Math.Clamp(srcX, 0, Math.Max(0, sourceGrid.Width - 1));
		srcY = Math.Clamp(srcY, 0, Math.Max(0, sourceGrid.Height - 1));
		srcW = Math.Clamp(srcW, 0, Math.Max(0, sourceGrid.Width - 1 - srcX));
		srcH = Math.Clamp(srcH, 0, Math.Max(0, sourceGrid.Height - 1 - srcY));
		//Dest rectangle boundary checks
		dstX = Math.Clamp(dstX, 0, Math.Max(0, Width - 1));
		dstY = Math.Clamp(dstY, 0, Math.Max(0, Height - 1));

		//Start copying
		int finSrcX, finDstX;

		for (int x = 0; x < srcW; x++)
		{
			finSrcX = Math.Min(srcX + x, sourceGrid.Width - 1);
			finDstX = Math.Min(dstX + x, Width - 1);
			for (int y = 0; y < srcH; y++)
				Data[finDstX, Math.Min(dstY + y, Height - 1)] = sourceGrid.Data[finSrcX, Math.Min(srcY + y, sourceGrid.Height - 1)];
		}
	}
}
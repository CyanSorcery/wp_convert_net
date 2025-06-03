namespace Kitsusu.DataStructures;

public class Grid
{
	public int Width = 0;
	public int Height = 0;
	private int[,] Data = { };

	public Grid(int _w, int _h, int _fill = 0)
	{
		//Resize the grid to the given size
		Resize(_w, _h, _fill);
	}

	public int Get(int _x, int _y) => Data[_x, _y];
	public int Set(int _x, int _y, int _val) => Data[_x, _y] = _val;

	public string Pack(bool _add_newlines = false)
	{
		//Create a string for this grid, optionally with newlines at the end of each row
		string _packed = "";
		for (int _y = 0; _y < Height; _y++)
		{
			for (int _x = 0; _x < Width; _x++)
				_packed += Data[_x, _y].ToString("x").PadLeft(2, '0');
			if (_add_newlines)
				_packed += "\r\n";
		}
		return _packed;
	}

	public void Resize(int _w, int _h, int _fill = 0)
	{
		//Get the current size of the grid
		int _old_w = Width;
		int _old_h = Height;
		int[,] _old_grid = Data;

		//Create a new grid of the specified size, and then copy the old grid into it
		Data = new int[_w, _h];
		Width = _w;
		Height = _h;
		Fill(_fill);

		for (int _x = 0; _x < _old_w; _x++)
			for (int _y = 0; _y < _old_h; _y++)
				Data[_x, _y] = _old_grid[_x, _y];
	}

	//Fill the grid with the given value
	public void Fill(int _fill)
	{
		for (int _x = 0; _x < Width; _x++)
			for (int _y = 0; _y < Height; _y++)
				Data[_x, _y] = _fill;
	}

	//Copy the given grid into this one
	public void CopyFrom(Grid _src, int _src_x, int _src_y, int _src_w, int _src_h, int _dst_x, int _dst_y)
	{
		//Source rectangle boundary checks
		_src_x = Math.Clamp(_src_x, 0, Math.Max(0, _src.Width - 1));
		_src_y = Math.Clamp(_src_y, 0, Math.Max(0, _src.Height - 1));
		_src_w = Math.Clamp(_src_w, 0, Math.Max(0, _src.Width - 1 - _src_x));
		_src_h = Math.Clamp(_src_h, 0, Math.Max(0, _src.Height - 1 - _src_y));
		//Dest rectangle boundary checks
		_dst_x = Math.Clamp(_dst_x, 0, Math.Max(0, Width - 1));
		_dst_y = Math.Clamp(_dst_y, 0, Math.Max(0, Height - 1));

		//Start copying
		int _fin_src_x, _fin_dst_x;

		for (int _x = 0; _x < _src_w; _x++)
		{
			_fin_src_x = Math.Min(_src_x + _x, _src.Width - 1);
			_fin_dst_x = Math.Min(_dst_x + _x, Width - 1);
			for (int _y = 0; _y < _src_h; _y++)
				Data[_fin_dst_x, Math.Min(_dst_y + _y, Height - 1)] = _src.Data[_fin_src_x, Math.Min(_src_y + _y, _src.Height - 1)];
		}
	}
}
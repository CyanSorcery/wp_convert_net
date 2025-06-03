using System.IO.Compression;
using Kitsusu.DataStructures;

class MiniPak
{
	const float min_version = 13f;
	public string pak_name { get; set; } = "No Name";
	public float file_version { get; set; } = min_version;
	public string? pak_id { get; set; }
	public MiniWorld[] pak_worlds { get; set; } = [new MiniWorld()];

	//These are the save slots that are currently in use (prevents overlaps)
	private bool[] save_slots = Enumerable.Repeat(false, 60).ToArray();


	public void ToPico8()
	{
		try
		{
			//Don't load this if it's too old
			if (file_version < min_version)
				throw new Exception($"Worldpak is too old. Must be version {min_version} or newer, but worldpak is version {file_version}.");

			//Count up how many stages there are
			int stage_count = 0;

			//Check all stages for conflicting save IDs
			foreach (MiniWorld world in pak_worlds)
			{
				foreach (MiniStage stage in world.world_stages)
				{
					stage_count++;

					//Too many stages?
					if (stage_count >= 60)
						throw new Exception("Too many stages in the given worldpak! The limit is 60.");
					
					//Prevent the save ID from being out of range
					stage.save_slot = Math.Clamp((int)stage.stage_id, 0, 59);

					if (save_slots[stage.save_slot])
						throw new Exception($"Stage {stage.stage_name} uses save slot {stage.save_slot} which is already in use!");

					save_slots[stage.save_slot] = true;

					//Convert this into a string for pico8
					stage.ToPico8();
				}
			}
		}
		catch (Exception e)
		{
			Console.WriteLine("Error while converting worldpak.");
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
		}
	}
}

public class MiniWorld
{
	public MiniStage[] world_stages { get; set; } = [new MiniStage()];
}

public class MiniStage
{
	//The maximum amount of time
	const float max_time = 599.9999f;

	//Various things from the full worldpak
	public string stage_name { get; set; } = "No Name";
	public string stage_author { get; set; } = "No Author";
	public double stage_width { get; set; } = 1;
	public double stage_height { get; set; } = 1;
	public double stage_id { get; set; } = 0;
	public double stage_target_time { get; set; } = max_time;
	public double stage_dev_time { get; set; } = max_time;
	public string stage_replay_data { get; set; } = "";
	public double stage_hint_count { get; set; } = 0;
	public string stage_data { get; set; } = "";

	public int save_slot { get; set; } = 0;

	//This is the string that pico8 will read
	public string ministage { get; set; } = "";

	public void ToPico8()
	{
		//Make sure the stage isn't too big
		if (stage_width > 16 || stage_height > 15)
			throw new Exception($"Stage {stage_name} is too large! Must be 16x15 or less.");

		//Start creating the ministage data

		//Stage name
		ministage += PicoLabel(stage_name);
		//Stage author
		ministage += PicoLabel(stage_author);
		//Stage width (minus 1)
		ministage += Math.Clamp((int)stage_width - 1, 0, 15).ToString("x");
		//Stage save slot
		ministage += save_slot.ToString("x").PadLeft(2, '0');
		//The stage target time
		ministage += PicoTimeFormat(stage_target_time);
		//The stage dev time
		ministage += PicoTimeFormat(stage_dev_time);

		//Now, we need to start decoding the stage data
		byte[] _stage_data_bytes = DecompressFromBase64(stage_data);
		string _stage_data_string = Convert.ToHexStringLower(_stage_data_bytes);

		//Copy the puzzle width and height
		int _puzz_w = (int)stage_width;
		int _puzz_h = (int)stage_height;

		//Create an array of strings for each object
		//The first string is for Stephanie, and will be overwritten later
		List<string> _objects = ["00000"];

		//The player start coordinates (used to generate hint arrows)
		int _player_start_x = 0;
		int _player_start_y = 0;

		//This array holds the position of floor portals we found previously, so we
		//can reference back to them. If the string is empty, that means it hasn't been found yet
		string[] _floor_portals = ["", "", "", ""];

		//Create a grid to hold the puzzle elements, with padding to hold the borders
		Grid _ele_grid = new(_puzz_w + 2, _puzz_h + 2);

		//How many tiles the player must touch in this puzzle
		int _tile_count = 0;
		int _tile_id;

		//Start reading elements into the grid
		for (int i = 0; i < _stage_data_bytes.Length; i++)
		{
			_tile_id = _stage_data_bytes[i];
			int _dst_x = i % _puzz_w;
			int _dst_y = i / _puzz_w;
			string _poskey = _dst_x.ToString("x") + _dst_y.ToString("x");
			int _ele_id = _tile_id & 0x1F;
			int _sub_id = (_tile_id >> 5) & 0x7;

			//Stephanie (overwrite the first entry if so)
			if (_tile_id == Tiles.ElementToBitmask(1, 1))
			{
				_objects[0] = $"0{_poskey}00";
				_player_start_x = _dst_x;
				_player_start_y = _dst_y;
			}

			//Heart
			if (_tile_id == Tiles.ElementToBitmask(2, 0)) _objects.Add($"1{_poskey}53");
			//Diamond
			if (_tile_id == Tiles.ElementToBitmask(2, 1)) _objects.Add($"2{_poskey}54");
			//Triangle
			if (_tile_id == Tiles.ElementToBitmask(2, 2)) _objects.Add($"3{_poskey}55");
			//Coin
			if (_tile_id == Tiles.ElementToBitmask(2, 2)) _objects.Add($"4{_poskey}56");
			//Octogem (encode index in sprite)
			if (_ele_id == 15) _objects.Add($"5{_poskey}0" + _sub_id.ToString("x"));
			//Normal state
			if (_tile_id == Tiles.ElementToBitmask(8, 0)) _objects.Add($"6{_poskey}e6");
			//Fire state
			if (_tile_id == Tiles.ElementToBitmask(8, 1)) _objects.Add($"7{_poskey}e7");
			//Ice state
			if (_tile_id == Tiles.ElementToBitmask(8, 2)) _objects.Add($"8{_poskey}e8");
			//Generic key
			if (_tile_id == Tiles.ElementToBitmask(12, 1)) _objects.Add($"9{_poskey}9f");

			//Floor portal
			if (_ele_id == 5)
			{
				//Do we have a matching floor portal? If not, store this for later
				//If so, create a pair of objects
				if (_floor_portals[_sub_id] == "")
					_floor_portals[_sub_id] = _poskey;
				else
				{
					string _dstkey = _floor_portals[_sub_id];
					_floor_portals[_sub_id] = "";
					_objects.Add($"a{_poskey}{_dstkey}");
					_objects.Add($"a{_dstkey}{_poskey}");
				}
			}

			//Arrows
			if (_ele_id == 17)
			{
				//Encode the arrow direction, then clear the tile for the stage generator
				_objects.Add($"b{_poskey}0" + _sub_id.ToString("x"));
				_tile_id = 1;
			}

			//Offset the destination
			_dst_x += 1;
			_dst_y += 1;

			//Apply the checkerboard?
			if (_tile_id == 1 && (_dst_x + _dst_y) % 2 == 1) _tile_id = 255;
			//Apply checkerboard to cracked floors?
			if (_tile_id == 9 && (_dst_x + _dst_y) % 2 == 0) _tile_id = 254;

			//If this is a valid floor, add to the tile count
			if (_tile_id > 0) _tile_count++;

			//Place the tile into the grid
			_ele_grid.Set(_dst_x, _dst_y, _tile_id);
		}

		//Increment these for the future grids
		_puzz_w += 2;
		_puzz_h += 2;

		//Create an autotile grid to use as reference for wall generation
		Grid _wall_ele_grid = new(_puzz_w, _puzz_h, 0);
		_wall_ele_grid.CopyFrom(_ele_grid, 0, 0, _puzz_w, _puzz_h, 0, 0);

		//For each element of the grid, prep it for autotiling
		for (int _x = 0; _x < _puzz_w; _x++)
			for (int _y = 0; _y < _puzz_h; _y++)
				_wall_ele_grid.Set(_x, _y, _wall_ele_grid.Get(_x, _y) >= 1 ? 0 : 1);

		//Now, perform the blob wang autotile on this
		//Ashe and Roxy note: this is a fair bit slower than what's in the game,
		//but since this isn't meant to run in real time, we don't have to be as efficient
		int _xl, _xr, _yt, _yb;
		for (int _x = 0; _x < _puzz_w; _x++)
		{
			_xl = Math.Max(_x - 1, 0);
			_xr = Math.Min(_x + 1, _puzz_w - 1);

			for (int _y = 0; _y < _puzz_h; _y++)
			{
				_yt = Math.Max(_y - 1, 0);
				_yb = Math.Min(_y + 1, _puzz_h - 1);

				//If the center tile is 0, skip
				if (_wall_ele_grid.Get(_x, _y) != 0)
				{
					_tile_id = Tiles.blob_wang_indices[
						_wall_ele_grid.Get(_xl, _yt) |
						(_wall_ele_grid.Get(_x, _yt) << 1) |
						(_wall_ele_grid.Get(_xr, _yt) << 2) |
						(_wall_ele_grid.Get(_xl, _y) << 3) |
						(_wall_ele_grid.Get(_xr, _y) << 4) |
						(_wall_ele_grid.Get(_xl, _yb) << 5) |
						(_wall_ele_grid.Get(_x, _yb) << 6) |
						(_wall_ele_grid.Get(_xr, _yb) << 7)
					];

					if (_tile_id < 255)
						_ele_grid.Set(_x, _y, Tiles.metaremap_walls[_tile_id]);
				}
			}
		}

		//Now do the autotiling for lava and water tiles
		Dictionary<int, int[]> _metaremap = new() { { 41, Tiles.metaremap_lava }, { 73, Tiles.metaremap_water } };
		foreach (var (_id, _remap) in _metaremap)
		{
			_wall_ele_grid.Fill(0);

			_wall_ele_grid.CopyFrom(_ele_grid, 0, 0, _puzz_w, _puzz_h, 0, 0);

			//For each element of the grid, prep it for autotiling
			for (int _x = 0; _x < _puzz_w; _x++)
				for (int _y = 0; _y < _puzz_h; _y++)
					_wall_ele_grid.Set(_x, _y, _wall_ele_grid.Get(_x, _y) == _id ? 1 : 0);

			for (int _x = 0; _x < _puzz_w; _x++)
			{
				_xl = Math.Max(_x - 1, 0);
				_xr = Math.Min(_x + 1, _puzz_w - 1);

				for (int _y = 0; _y < _puzz_h; _y++)
				{
					_yt = Math.Max(_y - 1, 0);
					_yb = Math.Min(_y + 1, _puzz_h - 1);

					//If the center tile is 0, skip
					if (_wall_ele_grid.Get(_x, _y) != 0)
					{
						_tile_id = Tiles.blob_wang_indices[
							_wall_ele_grid.Get(_xl, _yt) |
							(_wall_ele_grid.Get(_x, _yt) << 1) |
							(_wall_ele_grid.Get(_xr, _yt) << 2) |
							(_wall_ele_grid.Get(_xl, _y) << 3) |
							(_wall_ele_grid.Get(_xr, _y) << 4) |
							(_wall_ele_grid.Get(_xl, _yb) << 5) |
							(_wall_ele_grid.Get(_x, _yb) << 6) |
							(_wall_ele_grid.Get(_xr, _yb) << 7)
						];

						if (_tile_id < 255)
							_ele_grid.Set(_x, _y, _remap[_tile_id]);
					}
				}
			}
		}

		//Create the object string. First, encode how many objects we have
		string _obj_str = _objects.Count.ToString("x").PadLeft(2, '0');
		//Now, add all the objects to the string
		foreach (string _str in _objects)
			_obj_str += _str;


		//add the hint arrows, object list, number of tiles, and the stage data
		ministage += PicoHintArrows(_player_start_x, _player_start_y)
			+ _obj_str
			+ _tile_count.ToString("x").PadLeft(2, '0')
			+ _ele_grid.Pack();

		Console.WriteLine(ministage);
	}

	private string PicoHintArrows(int _start_x, int _start_y)
	{
		//If there's no hints, just return 0
		if (stage_replay_data == "") return "0";

		//Get the replay bytes
		byte[] _replay_data = DecompressFromBase64(stage_replay_data);

		//Parse the hint arrows. Each byte contains two hint arrows, with one potential extra that we ignore
		List<byte> _parsed_hints = [];
		int _arrow;

		for (int i = 2; i < _replay_data.Length; i++)
		{
			_arrow = _replay_data[i];
			//for (int j = 0; j < 8; j += 2)
			//	_parsed_hints.Add((byte)((_arrow >> j) & 0x3));
			_parsed_hints.Add((byte)((_arrow >> 2) & 0x3));
			_parsed_hints.Add((byte)((_arrow >> 0) & 0x3));
			_parsed_hints.Add((byte)((_arrow >> 6) & 0x3));
			_parsed_hints.Add((byte)((_arrow >> 4) & 0x3));
		}

		//Remove first hint, since the player wont see it
		//_parsed_hints.RemoveAt(0);

		//Find out how many moves the are in the replay array
		//Gamemaker stores the bytes flipped, so we have to flip them back
		int _move_count = (_replay_data[1] << 8) | _replay_data[0];
		int _hint_count = Math.Min(Math.Min(15, Math.Min(_move_count, (int)stage_hint_count)), _parsed_hints.Count);

		//Now that we have our hints, go ahead and make a string of them
		string _fin = _hint_count.ToString("x");
		int _dir;
		double _angle;
		for (int i = 0; i < _hint_count; i++)
		{
			_dir = _parsed_hints[i];
			_fin += _dir.ToString() + _start_x.ToString("x") + _start_y.ToString("x");
			_angle = MathF.PI / 2 * _dir;
			_start_x += (int)Math.Round(Math.Cos(_angle));
			_start_y -= (int)Math.Round(Math.Sin(_angle));
		}
		return _fin;
	}
	
	private static byte[] DecompressFromBase64(string _b64)
	{
		using MemoryStream _o = new(Convert.FromBase64String(_b64));
		using ZLibStream _z = new(_o, CompressionMode.Decompress);
		using MemoryStream _m = new();
		_z.CopyTo(_m);
		return _m.ToArray();
	}

	private static string PicoLabel(string _str)
	{
		//Note: this currently doesn't convert out of bounds characters to ascii
		//first, limit this to 16 characters
		_str = _str.Substring(0, Math.Min(16, _str.Length)).ToLower();

		if (_str == "") _str = "unset";

		//Return how many bytes this is (minus 1, as it should always have at least one character)
		//and then append the string
		return (_str.Length - 1).ToString("x").ToLower() + _str;
	}

	private static string PicoTimeFormat(double _t)
	{
		//To correct for precision issues, add a little bit of time
		_t += 0.0011;

		return Math.Floor(_t).ToString().PadLeft(3, '0') + "." + Math.Floor((_t % 1) * 10000).ToString().PadRight(4, '0');
	}
}

/*
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class WorldStage
    {
        public bool stage_finalized { get; set; }
        public bool stage_dark { get; set; }
        public double stage_move_count { get; set; }
        public double stage_actual_moves { get; set; }
        public bool stage_special { get; set; }
        public double stage_hint_count { get; set; }
        public List<object> stage_cameras { get; set; }
        public bool stage_unlock_zoom { get; set; }
        public string stage_start_state { get; set; }
        public string stage_theme { get; set; }
        public double stage_tile_count { get; set; }
    }
    public class PakWorld
    {
        public List<WorldStage> world_stages { get; set; }
        public double world_required_stars { get; set; }
        public string world_name { get; set; }
        public string world_id { get; set; }
        public string world_theme { get; set; }
    }

    public class Root
    {
        
        public double pak_error_code { get; set; }
        public double pak_version { get; set; }
        public double pak_save_time { get; set; }
        public string pak_desc { get; set; }
        public string pak_author { get; set; }
        public double pak_modio_id { get; set; }
        public double pak_cache_worldcount { get; set; }
        public double pak_cache_stagecount { get; set; }
    }


*/
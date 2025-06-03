class Tiles
{
	//First entry is the element ID, second is the tiles that'll go there
	//The tiles are in the order of TL, TR, BL, BR
	//If unset, all tiles are 0. These are "void tiles" and can be overwritten,
	//with the exception of entry 0
	private int[][] lut = new int[256][];

	public static readonly int[] blob_wang_indices = [
		0,0,1,1,0,0,1,1,
		2,2,3,4,2,2,3,4,
		5,5,6,6,5,5,7,7,
		8,8,9,10,8,8,11,12,
		0,0,1,1,0,0,1,1,
		2,2,3,4,2,2,3,4,
		5,5,6,6,5,5,7,7,
		8,8,9,10,8,8,11,12,
		13,13,14,14,13,13,14,14,
		15,15,16,17,15,15,16,17,
		18,18,19,19,18,18,20,20,
		21,21,22,23,21,21,24,25,
		13,13,14,14,13,13,14,14,
		26,26,27,28,26,26,27,28,
		18,18,19,19,18,18,20,20,
		29,29,30,31,29,29,32,33,
		0,0,1,1,0,0,1,1,
		2,2,3,4,2,2,3,4,
		5,5,6,6,5,5,7,7,
		8,8,9,10,8,8,11,12,
		0,0,1,1,0,0,1,1,
		2,2,3,4,2,2,3,4,
		5,5,6,6,5,5,7,7,
		8,8,9,10,8,8,11,12,
		13,13,14,14,13,13,14,14,
		15,15,16,17,15,15,16,17,
		34,34,35,35,34,34,36,36,
		37,37,38,39,37,37,40,41,
		13,13,14,14,13,13,14,14,
		26,26,27,28,26,26,27,28,
		34,34,35,35,34,34,36,36,
		42,42,43,44,42,42,45,46
	];
	
	//This remaps the tiles from the lookup table to generate walls
	public static readonly int[] metaremap_walls = [11,12,13,14,16,17,18,19,
		20,21,22,23,24,25,26,27,
		28,29,30,31,32,42,43,45,
		46,48,49,50,51,52,53,54,
		55,56,57,58,59,60,61,62,
		63,64,65,75,76,77,78];

	//This remaps to generate lava tiles
	public static readonly int[] metaremap_lava = [80,81,82,83,84,85,86,87,
		88,89,90,91,92,93,94,95,
		96,97,103,104,107,108,109,110,
		112,113,114,115,116,117,118,119,
		120,121,122,123,124,125,126,127,
		128,129,130,131,132,133,134];

	//This remaps to generate water tiles
	public static readonly int[] metaremap_water = [135,136,139,140,141,142,144,145,
		146,147,148,149,150,151,152,153,
		154,155,156,157,158,159,160,161,
		162,163,164,165,166,167,168,169,
		170,171,172,173,174,176,177,178,
		179,180,181,182,183,184,185];

	private Tiles()
	{
		//Blank tile player can step on
		lut[ElementToBitmask(1, 0)] = GetMirroredTile(16);
		//Second blank tile
		lut[255] = GetMirroredTile(17);

		//Player tile
		lut[ElementToBitmask(1, 1)] = GetFullTile(218);

		//Heart (on)
		lut[ElementToBitmask(3, 0)] = GetMirroredTile(19);
		//Heart (off)
		lut[ElementToBitmask(4, 0)] = GetMirroredTile(20);

		//Diamond (on)
		lut[ElementToBitmask(3, 1)] = GetMirroredTile(21);
		//Diamond (off)
		lut[ElementToBitmask(4, 1)] = GetMirroredTile(22);

		//Triangle (on)
		lut[ElementToBitmask(3, 2)] = GetMirroredTile(23);
		//Triangle (off)
		lut[ElementToBitmask(4, 2)] = GetMirroredTile(24);

		//Coin (on)
		lut[ElementToBitmask(3, 3)] = GetMirroredTile(25);
		//Coin (off)
		lut[ElementToBitmask(4, 3)] = GetMirroredTile(26);

		//Octoblock (on)
		lut[ElementToBitmask(10, 2)] = GetMirroredTile(27);
		//Octoblock (off)
		lut[ElementToBitmask(10, 3)] = GetMirroredTile(28);

		//Zapper (cyan)
		lut[ElementToBitmask(7, 1)] = GetMirroredTile(29);
		//Zapper (magenta)
		lut[ElementToBitmask(7, 0)] = GetMirroredTile(30);
		//Zapper (yellow)
		lut[ElementToBitmask(7, 2)] = GetMirroredTile(31);

		//Slime trap
		lut[ElementToBitmask(9, 4)] = GetMirroredTile(48);

		//Generic lock block
		lut[ElementToBitmask(10, 4)] = GetMirroredTile(51);
		//Generic key (floor)
		lut[ElementToBitmask(12, 1)] = GetMirroredTile(18);

		//Heart key (floor)
		lut[ElementToBitmask(2, 0)] = GetMirroredTile(52);
		//Diamond key (floor)
		lut[ElementToBitmask(2, 1)] = GetMirroredTile(53);
		//Triangle key (floor)
		lut[ElementToBitmask(2, 2)] = GetMirroredTile(54);
		//Coin key (floor)
		lut[ElementToBitmask(2, 3)] = GetMirroredTile(55);

		//Octogems
		for (int i = 0; i < 8; i++)
			lut[ElementToBitmask(15, i)] = GetMirroredTile(56 + i);

		//Normal state
		lut[ElementToBitmask(8, 0)] = GetMirroredTile(80);
		//Fire state
		lut[ElementToBitmask(8, 1)] = GetMirroredTile(81);
		//Ice state
		lut[ElementToBitmask(8, 2)] = GetMirroredTile(82);

		//Red portal
		lut[ElementToBitmask(5, 0)] = GetFullTile(88);
		//Green portal
		lut[ElementToBitmask(5, 1)] = GetFullTile(90);
		//Blue portal
		lut[ElementToBitmask(5, 2)] = GetFullTile(92);
		//Yellow portal
		lut[ElementToBitmask(5, 3)] = GetFullTile(94);

		//Conveyer east
		lut[ElementToBitmask(6, 0)] = GetFullTile(112);
		//Conveyer north
		lut[ElementToBitmask(6, 1)] = GetFullTile(114);
		//Conveyer west
		lut[ElementToBitmask(6, 2)] = GetFullTile(116);
		//Conveyer south
		lut[ElementToBitmask(6, 3)] = GetFullTile(118);

		//Ice block
		lut[ElementToBitmask(10, 0)] = GetFullTile(120);

		//Ice floor
		lut[ElementToBitmask(9, 3)] = GetFullTile(122);

		//Cracked floor
		lut[ElementToBitmask(9, 0)] = GetFullTile(124);

		//Lava floor
		lut[ElementToBitmask(9, 1)] = GetSingleTile(191);
		//Water tile
		lut[ElementToBitmask(9, 2)] = GetSingleTile(207);

		//Slime tile (normal)
		lut[240] = GetFullTile(218);
		//Slime tile (fire)
		lut[241] = GetFullTile(220);
		//Slime tile (ice)
		lut[242] = GetFullTile(222);

		//Cracked floor (alternate)
		lut[254] = GetFullTile(126);

		//Closed slime trap
		lut[253] = GetMirroredTile(49);

		//pit
		lut[252] = GetMirroredTile(50);

		//Add the metaremaps to the lookup table
		AddMetaremap(metaremap_walls, true);
		AddMetaremap(metaremap_lava, false, 176, true);
		AddMetaremap(metaremap_water, false, 192, true);

		//Fill out any remaining tiles with a blank set
		int[] _dummy_set = GetSingleTile(15);
		for (int i = 1; i < 256; i++)
			if (lut[i][0] == 0)
				lut[i] = _dummy_set;
	}

	private void AddMetaremap(int[] _metaremap, bool _clear_15th, int _add_val = 0, bool _do_water_lava = false)
	{
		//Tile lookups we'll need for blob wang
		int[][] _blob_wang_tiles = [[2, 1, 4, 8],[6, 9, 4, 8],[3, 1, 12, 8],[7, 9, 12, 8],[15, 9, 12, 8],[2, 3, 4, 12],[6, 11, 4, 12],[6, 15, 4, 12],
		[3, 3, 12, 12],[7, 11, 12, 12],[15, 11, 12, 12],[7, 15, 12, 12],[15, 15, 12, 12],[2, 1, 6, 9],[6, 9, 6, 9],[3, 1, 14, 9],
		[7, 9, 14, 9],[15, 9, 14, 9],[2, 3, 6, 13],[6, 11, 6, 13],[6, 15, 6, 13],[3, 3, 14, 13],[7, 11, 14, 13],[15, 11, 14, 13],
		[7, 15, 14, 13],[15, 15, 14, 13],[3, 1, 15, 9],[7, 9, 15, 9],[15, 9, 15, 9],[3, 3, 15, 13],[7, 11, 15, 13],[15, 11, 15, 13],
		[7, 15, 15, 13],[15, 15, 15, 13],[2, 3, 6, 15],[6, 11, 6, 15],[6, 15, 6, 15],[3, 3, 14, 15],[7, 11, 14, 15],[15, 11, 14, 15],
		[7, 15, 14, 15],[15, 15, 14, 15],[3, 3, 15, 15],[7, 11, 15, 15],[15, 11, 15, 15],[7, 15, 15, 15],[15, 15, 15, 15]];

		//First, do any needed modification to the given values
		for (int i = 0; i < 47; i++)
			for (int j = 0; j < 4; j++)
			{
				//If we're to clear the 15th tile, or are applying the lava/water grid, do this
				if (_clear_15th || (_do_water_lava && j == 3))
					if (_blob_wang_tiles[i][j] == 15)
						_blob_wang_tiles[i][j] = 0;

				_blob_wang_tiles[i][j] += _add_val;
			}

		//Now, add them to the lookup table
		for (int i = 0; i < 47; i++)
			lut[_metaremap[i]] = _blob_wang_tiles[i];
	}
	public static int ElementToBitmask(int _ele_id, int _sub_id)
	{
		return _ele_id | (_sub_id << 5);
	}

	private static int[] GetMirroredTile(int _tile_id)
	{
		return [253, _tile_id, 253, _tile_id + 16];
	}
	private static int[] GetFullTile(int _tile_id)
	{
		return [_tile_id, _tile_id + 1, _tile_id + 16, _tile_id + 17];
	}
	private static int[] GetSingleTile(int _tile_id)
	{
		return [_tile_id, _tile_id, _tile_id, _tile_id];
	}
}
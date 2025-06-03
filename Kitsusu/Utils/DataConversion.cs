namespace Kitsusu.Utils;

using System.IO.Compression;

public class DataConversion
{
	/// <summary>
	/// Decompresses a binary encoded Base64 string to a byte array.
	/// </summary>
	/// <param name="b64">Base64 string</param>
	/// <returns>Decompressed bytes.</returns>
	public static byte[] DecompressFromBase64(string b64)
	{
		using MemoryStream o = new(Convert.FromBase64String(b64));
		using ZLibStream z = new(o, CompressionMode.Decompress);
		using MemoryStream m = new();
		z.CopyTo(m);
		return m.ToArray();
	}
}
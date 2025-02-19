using System;
using System.IO;
using System.IO.Compression;

namespace Client
{
	public static class GZip
	{
		public static byte[] Compress(byte[] buff)
		{
			using MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(BitConverter.GetBytes(buff.Length), 0, 4);
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
			{
				gZipStream.Write(buff, 0, buff.Length);
				gZipStream.Flush();
			}
			return memoryStream.ToArray();
		}

		public static byte[] Decompress(byte[] buff)
		{
			using MemoryStream memoryStream = new MemoryStream(buff);
			byte[] array = new byte[4];
			memoryStream.Read(array, 0, 4);
			int num = BitConverter.ToInt32(array, 0);
			using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
			byte[] array2 = new byte[num];
			gZipStream.Read(array2, 0, num);
			return array2;
		}
	}
}

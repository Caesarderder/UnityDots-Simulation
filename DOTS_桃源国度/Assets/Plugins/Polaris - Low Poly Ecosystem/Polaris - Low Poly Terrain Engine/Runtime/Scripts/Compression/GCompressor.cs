#if GRIFFIN
using Lzf;
using UnityEngine;


namespace Pinwheel.Griffin.Compression
{
    public static class GCompressor
    {
        private static byte[] defaultOutputBuffer;
        private static byte[] DefaultOutputBuffer
        {
            get
            {
                int bufferSizeMB = 100;
                if (defaultOutputBuffer == null ||
                    defaultOutputBuffer.Length != bufferSizeMB * 1000000)
                {
                    defaultOutputBuffer = new byte[bufferSizeMB * 1000000];
                }
                return defaultOutputBuffer;
            }
        }

        public static byte[] Compress(byte[] data)
        {
            if (data.Length == 0)
                return data;

            byte[] outputData = new byte[data.Length * 2];

            LZF compressor = new LZF();
            int compressedLength = compressor.Compress(data, data.Length, outputData, outputData.Length);

            byte[] result = new byte[compressedLength];
            System.Array.Copy(outputData, result, compressedLength);
            return result;
        }

        public static byte[] Decompress(byte[] data, int outputSizeHint = -1)
        {
            if (data.Length == 0)
                return data;
            byte[] outputData;
            if (outputSizeHint > 0)
            {
                outputData = new byte[outputSizeHint];
            }
            else
            {
                outputData = DefaultOutputBuffer;
            }

            LZF decompressor = new LZF();
            int decompressedLength = decompressor.Decompress(data, data.Length, outputData, outputData.Length);

            byte[] result = new byte[decompressedLength];
            System.Array.Copy(outputData, result, decompressedLength);
            return result;
        }

        public static void CleanUp()
        {
            defaultOutputBuffer = null;
            LZF.CleanUp();
        }
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList
{
    class ImageManager
    {
        public static string ZipStream(MemoryStream input)
        {
            input.Seek(0, SeekOrigin.Begin);

            using (var gzipOutput = new MemoryStream())
            {
                using (var gzip = new GZipStream(gzipOutput, CompressionMode.Compress))
                {
                    input.CopyTo(gzip);
                }

                return Convert.ToBase64String(gzipOutput.ToArray());
            }
        }

        public static void UnzipStream(string str, MemoryStream output)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (MemoryStream gzipInput = new MemoryStream(bytes))
            {
                using (GZipStream gzip = new GZipStream(gzipInput, CompressionMode.Decompress))
                {
                    gzip.CopyTo(output);
                }
            }
        }

        public static string ImageToString(Bitmap image)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream()) // result from bitmap to stream
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ZipStream(ms);
                }
            }
            catch (Exception e)
            {
                Log.write(e.Message);
                return "";
            }
        }


        public static Bitmap StringToImage(string bitmap)
        {
            try
            {
                MemoryStream ms = new MemoryStream(); // input stream for gzip
                UnzipStream(bitmap, ms);
                return new Bitmap(ms);
            }
            catch (Exception e)
            {
                Log.write(e.Message);
                return null;
            }
        }
    }
}

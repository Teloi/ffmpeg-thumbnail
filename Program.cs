using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.NET;

namespace FFmpeg_Thumbnail
{
    internal class Program
    {
        private static readonly JsonConfigHelper JsonReader = new JsonConfigHelper("config.json");
        private static readonly int GROUND_WIDTH = Convert.ToInt32(JsonReader.GetValue("GroundWidth"));
        private static readonly int GROUND_HEIGHT = Convert.ToInt32(JsonReader.GetValue("GroundHeight"));
        private static readonly int ROW_COUNT = Convert.ToInt32(JsonReader.GetValue("RowCount"));
        private static readonly int COLUMN_COUNT = Convert.ToInt32(JsonReader.GetValue("ColumnCount"));
        private static readonly string FFMPEG_PATH = JsonReader.GetValue("FFmpegPath");
        private static readonly string INPUT_VIDEO = JsonReader.GetValue("InputVideo");
        private static readonly string OUTPUT_PATH = JsonReader.GetValue("OutputPath");
        private static readonly string OUTPUT_NAME = "-thumb";
        private static readonly string OUTPUT_TEMP_NAME = "-thumb-temp";
        private static readonly string OUTPUT_EXT = ".jpg";

        public static void Main(string[] args)
        {
            TransformVideo().GetAwaiter().GetResult();
        }

        public static async Task<bool> TransformVideo()
        {
            Engine ffmpeg = new Engine(FFMPEG_PATH);
            MediaFile inputFile = new MediaFile(INPUT_VIDEO);
            MetaData metadata = await ffmpeg.GetMetaDataAsync(inputFile);
            Bitmap bitmap = new Bitmap(GROUND_WIDTH, GROUND_HEIGHT);
            string inputFileName = Path.GetFileNameWithoutExtension(INPUT_VIDEO);

            double intervalTime = Math.Floor(metadata.Duration.TotalMilliseconds / (ROW_COUNT * COLUMN_COUNT + 1));
            for (int i = 1; i < (ROW_COUNT * COLUMN_COUNT + 1); i++)
            {
                ConversionOptions options = new ConversionOptions { Seek = TimeSpan.FromMilliseconds(intervalTime * i) };
                FileInfo fileInfo = new FileInfo(Path.Combine(OUTPUT_PATH, inputFileName + OUTPUT_TEMP_NAME + OUTPUT_EXT));
                MediaFile file = await ffmpeg.GetThumbnailAsync(inputFile, new MediaFile(fileInfo), options);
                Image tempThumbImage = Image.FromFile(file.FileInfo.FullName);
                Image smallThumbImage = ImageHelper.ResizeImage(tempThumbImage, (GROUND_WIDTH / ROW_COUNT), (GROUND_HEIGHT / COLUMN_COUNT), true);
                using (Bitmap bmp = new Bitmap(smallThumbImage))
                {
                    double col = Math.Floor(((double)i - 1) / ROW_COUNT) * (GROUND_HEIGHT / COLUMN_COUNT);
                    double row = (i - 1) % ROW_COUNT * (GROUND_WIDTH / ROW_COUNT);
                    using (Graphics draw = Graphics.FromImage(bitmap))
                    {
                        draw.DrawImage(bmp, (int)row, (int)col, bmp.Width, bmp.Height);
                        draw.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        Font font = new Font("Arial", 20, FontStyle.Regular);
                        draw.DrawString(TimeSpan.FromMilliseconds(intervalTime * i).ToString(), font, new SolidBrush(Color.White), (int)row, (int)col);
                    }
                }

                smallThumbImage.Dispose();
                tempThumbImage.Dispose();
                fileInfo.Delete();
                Console.WriteLine(i);
            }

            bitmap.Save(Path.Combine(OUTPUT_PATH, inputFileName + OUTPUT_NAME + OUTPUT_EXT), System.Drawing.Imaging.ImageFormat.Jpeg);
            bitmap.Dispose();

            return true;
        }
    }
}

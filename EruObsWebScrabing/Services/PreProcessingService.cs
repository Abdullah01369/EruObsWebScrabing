using EruObsWebScrabing.IServices;
using Humanizer;
using System.Drawing;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace EruObsWebScrabing.Services
{
    public class PreProcessingService : IPreProcessingOCR
    {


        public Bitmap PreprocessImage(Bitmap originalImage)
        {
            Bitmap processedImage = originalImage.Clone() as Bitmap;
            processedImage = ConvertToGrayscale(processedImage);
            processedImage = EnhanceContrast(processedImage);
            processedImage = ApplyThreshold(processedImage);
            processedImage = Dilate(processedImage);
            processedImage = Erode(processedImage);
            return processedImage;
        }
        #region
        public Bitmap RemoveNoise(Bitmap image)
        {
            Bitmap cleanedImage = new Bitmap(image.Width, image.Height);

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    // Piksel etrafındaki komşu pikselleri kontrol et
                    int neighborBlackPixels = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (image.GetPixel(x + i, y + j).GetBrightness() < 0.5)
                            {
                                neighborBlackPixels++;
                            }
                        }
                    }

                    // Eğer çok az siyah komşu varsa, beyaz olarak işaretle
                    if (neighborBlackPixels <= 2)
                    {
                        cleanedImage.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        cleanedImage.SetPixel(x, y, image.GetPixel(x, y));
                    }
                }
            }

            return cleanedImage;
        }

        public Bitmap ConvertToGrayscale(Bitmap original)
        {
            Bitmap grayscale = new Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    Color c = original.GetPixel(i, j);
                    int grayScale = (int)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayscale.SetPixel(i, j, newColor);
                }
            }

            return grayscale;
        }

        public Bitmap EnhanceContrast(Bitmap original)
        {
            Bitmap contrastImage = new Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    Color pixel = original.GetPixel(i, j);
                    int grayScale = pixel.R;

                    // Kontrast artırma (eşik değerini ayarlayabilirsiniz)
                    if (grayScale > 180)
                        contrastImage.SetPixel(i, j, Color.White);
                    else
                        contrastImage.SetPixel(i, j, Color.Black);
                }
            }

            return contrastImage;
        }

        public Bitmap ApplyThreshold(Bitmap original, int threshold = 200)
        {
            Bitmap thresholdImage = new Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    Color pixel = original.GetPixel(i, j);

                    // Siyah-beyaz dönüşüm
                    if (pixel.R > threshold)
                        thresholdImage.SetPixel(i, j, Color.White);
                    else
                        thresholdImage.SetPixel(i, j, Color.Black);
                }
            }

            return thresholdImage;
        }

        public Bitmap Dilate(Bitmap image)
        {
            Bitmap dilatedImage = new Bitmap(image.Width, image.Height);

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    bool shouldBeFilled = false;

                    // Komşu piksellerde siyah varsa, bu pikseli siyah yap
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (image.GetPixel(x + i, y + j).GetBrightness() < 0.5)
                            {
                                shouldBeFilled = true;
                                break;
                            }
                        }
                        if (shouldBeFilled) break;
                    }

                    dilatedImage.SetPixel(x, y, shouldBeFilled ? Color.Black : Color.White);
                }
            }

            return dilatedImage;
        }

        public Bitmap Erode(Bitmap image)
        {
            Bitmap erodedImage = new Bitmap(image.Width, image.Height);

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    bool shouldBeWhite = false;

                    // Komşu piksellerde beyaz varsa, bu pikseli beyaz yap
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (image.GetPixel(x + i, y + j).GetBrightness() > 0.5)
                            {
                                shouldBeWhite = true;
                                break;
                            }
                        }
                        if (shouldBeWhite) break;
                    }

                    erodedImage.SetPixel(x, y, shouldBeWhite ? Color.White : Color.Black);
                }
            }

            return erodedImage;
        }
        #endregion 
    }

}

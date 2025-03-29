using System.Drawing;

namespace EruObsWebScrabing.IServices
{
    public interface IPreProcessingOCR
    {
 

        Bitmap PreprocessImage(Bitmap originalImage);


        Bitmap RemoveNoise(Bitmap image);



        Bitmap ConvertToGrayscale(Bitmap original);


        Bitmap EnhanceContrast(Bitmap original);

        Bitmap ApplyThreshold(Bitmap original, int threshold = 200);


        Bitmap Dilate(Bitmap image);


        Bitmap Erode(Bitmap image);

    }
}

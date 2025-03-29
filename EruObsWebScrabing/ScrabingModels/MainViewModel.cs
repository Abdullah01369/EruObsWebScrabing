using EruObsWebScrabing.Services;

namespace EruObsWebScrabing.ScrabingModels
{
    public class MainViewModel
    {
        public string ogrenciNo  { get; set; }
        public string adiSoyadi { get; set; }
        public string fakulteAdi { get; set; }
        public string bolumAdi { get; set; }
        public string sinifSeneGano { get; set; }
        public string danisman { get; set; }

        public List<LessonList> GetLessonLists = new List<LessonList>();

    }
}

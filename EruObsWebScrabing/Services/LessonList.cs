using System.Data.SqlTypes;

namespace EruObsWebScrabing.Services
{
    public class LessonList
    {

        public static List<LessonList> lessonLists = new List<LessonList>();

        public string dersAdi { get; set; }
        public string finalNotu { get; set; }
        public string vizeNotu { get; set; }
        public string but { get; set; }
        public string ort { get; set; }
        public string code { get; set; }


        public static List<LessonList> GetLessons()
        {
            return lessonLists;
        }

        public static void AddLesson(string dersadi, string final, string vize, string but, string code, string ort)
        {
            LessonList lessonList = new LessonList()
            {
                ort = ort,
                but = but,
                code = code,
                dersAdi = dersadi,
                finalNotu = final,
                vizeNotu = vize
            };
            lessonLists.Add(lessonList);

        }



    }
}

using EruObsWebScrabing.ScrabingModels;
using EruObsWebScrabing.Services;
using System.Drawing;
using Tesseract;

namespace EruObsWebScrabing.IServices
{
    public interface IObsLoginService
    {

        Task<StudentLoginModel> GetLoginPageData();
        Task<string> Login(StudentLoginModel loginModel);
     
        Task<List<LessonList>> GetScores();








    }
}

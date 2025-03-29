using EruObsWebScrabing.IServices;
using EruObsWebScrabing.Models;
using EruObsWebScrabing.ScrabingModels;
using EruObsWebScrabing.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EruObsWebScrabing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IObsLoginService _obsLoginService;
        private static string MainPageHtml;


        public HomeController(ILogger<HomeController> logger, IObsLoginService obsLoginService)
        {
            _obsLoginService = obsLoginService;
            _logger = logger;

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            bool status = true;
            StudentLoginModel loginvm;
            string res = "";

            while (status)
            {
                loginvm = await _obsLoginService.GetLoginPageData();
                if (loginvm.CaptchaTextBox != "" && !loginvm.CaptchaTextBox.Any(c => char.IsLetter(c)) && loginvm.CaptchaTextBox.Length == 5)
                {

                    loginvm.txtBoxSifre = model.Password;
                    loginvm.txtboxOgrenciNo = model.StudentNo;
                    res = await _obsLoginService.Login(loginvm);
                    if (res.Contains("Güvenli Çýkýþ"))
                    {
                        status = false;
                    }
                }
            }

            MainPageHtml = res;

            var scores = await _obsLoginService.GetScores();


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(MainPageHtml);

          var val=  LessonList.GetLessons();



            string ogrenciNo = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtOgrenciNo']").InnerText.Trim();
            string adiSoyadi = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtAdiSoyadi']").InnerText.Trim();
            string fakulteAdi = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtFakulteAdi']").InnerText.Trim();
            string bolumAdi = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtBolumAdi']").InnerText.Trim();
            string sinifSeneGano = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtSinifSeneGano']").InnerText.Trim();
            string danisman = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_labelBirinciDanisman']").InnerText.Trim();

            MainViewModel mainViewModel = new MainViewModel()
            {
                GetLessonLists = LessonList.lessonLists,
                adiSoyadi = adiSoyadi,
                bolumAdi = bolumAdi,
                danisman = danisman,
                fakulteAdi = fakulteAdi,
                ogrenciNo = ogrenciNo,
                sinifSeneGano = sinifSeneGano
            };


            return View(mainViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

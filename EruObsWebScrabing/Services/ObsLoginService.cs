using EruObsWebScrabing.IServices;
using EruObsWebScrabing.ScrabingModels;
using HtmlAgilityPack;
using System.Drawing;
using Tesseract;

namespace EruObsWebScrabing.Services
{
    public class ObsLoginService : IObsLoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static HttpClient _httpClient;
        private readonly string _baseUrl = "https://obisis2.erciyes.edu.tr/";
        private readonly string _LessonUrl = "https://obisis2.erciyes.edu.tr/Default.aspx?tabInd=3&tabNo=3";
        private IPreProcessingOCR _preprocessingOCR;

        public ObsLoginService(IHttpClientFactory httpClientFactory, IPreProcessingOCR preProcessingOCR)
        {
            _preprocessingOCR = preProcessingOCR;
            _httpClientFactory = httpClientFactory;
        }


        string ReadCaptcha(string imagePath)
        {
            Bitmap originalImage = new Bitmap(imagePath);
            Bitmap preprocessedImage = _preprocessingOCR.PreprocessImage(originalImage);

            using var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);


            return page.GetText().Trim();
        }

        public async Task<List<LessonList>> GetScores()
        {


            var response = await _httpClient.GetAsync(_LessonUrl);
            var html = await response.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var dersler = doc.DocumentNode.SelectNodes("//tr[@class='NormalBlack']");


            foreach (var ders in dersler)
            {
                var cells = ders.SelectNodes("td");
                if (cells != null && cells.Count > 5)
                {
                    string derscode = cells[0].InnerText.Trim();
                    string dersAdi = cells[1].InnerText.Trim();
                    string vize = cells[6].InnerText.Trim();
                    string final = cells[9].InnerText.Trim();
                    string but = cells[10].InnerText.Trim();
                    string ort = cells[11].InnerText.Trim();

                    LessonList.AddLesson(dersAdi, final, vize, but, derscode, ort);


                }

            }


            return LessonList.GetLessons();

        }

        public async Task<StudentLoginModel> GetLoginPageData()
        {
            var client = _httpClientFactory.CreateClient();
            _httpClient = client;
            var response = await client.GetAsync(_baseUrl);
            if (!response.IsSuccessStatusCode) return null;

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var model = new StudentLoginModel
            {
                __VIEWSTATE = GetInputValue(doc, "__VIEWSTATE"),
                __VIEWSTATEGENERATOR = GetInputValue(doc, "__VIEWSTATEGENERATOR"),
                __EVENTVALIDATION = GetInputValue(doc, "__EVENTVALIDATION"),
                ctl02_RadCaptcha1_ClientState = GetInputValue(doc, "ctl02_RadCaptcha1_ClientState"),
                btnLogin = "Giriş" // Buton değeri sabit
            };

            var captchaImgNode = doc.DocumentNode.SelectSingleNode("//img[contains(@id, 'RadCaptcha1_CaptchaImageUP')]");
            if (captchaImgNode != null)
            {
                model.CaptchaImageUrl = _baseUrl + captchaImgNode.GetAttributeValue("src", "");
            }
            model.CaptchaImageUrl = model.CaptchaImageUrl.Replace("&amp;", "&");

            HttpResponseMessage resp = await client.GetAsync(model.CaptchaImageUrl);
            var html2 = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {


                string imgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "img");
                if (!Directory.Exists(imgFolderPath))
                {

                    Directory.CreateDirectory(imgFolderPath);
                }
                var outp = Guid.NewGuid();


                string imagePath = Path.Combine(imgFolderPath, $"{outp}.jpg");



                using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] imageData = await resp.Content.ReadAsByteArrayAsync();
                    await fileStream.WriteAsync(imageData, 0, imageData.Length);
                    fileStream.Dispose();
                    fileStream.Close();
                }


                Console.WriteLine($" CAPTCHA resmi kaydedildi: {imagePath}");
                string imagePathForPrep = Path.Combine(imgFolderPath, "output.jpg");


                string captchaText = ReadCaptcha(imagePath);
                model.CaptchaTextBox = captchaText;
            }
            else
            {
                Console.WriteLine(" CAPTCHA resmi alınamadı.");
            }

            return model;
        }

        private string GetInputValue(HtmlDocument doc, string inputName)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//input[@name='{inputName}']");
            return node?.GetAttributeValue("value", "") ?? "";
        }

        public async Task<string> Login(StudentLoginModel loginModel)
        {

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("__VIEWSTATE", loginModel.__VIEWSTATE),
            new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", loginModel.__VIEWSTATEGENERATOR),
            new KeyValuePair<string, string>("__EVENTVALIDATION", loginModel.__EVENTVALIDATION),
            new KeyValuePair<string, string>("ctl02$txtboxOgrenciNo", loginModel.txtboxOgrenciNo),
            new KeyValuePair<string, string>("ctl02$txtBoxSifre", loginModel.txtBoxSifre),
            new KeyValuePair<string, string>("ctl02$RadCaptcha1$CaptchaTextBox", loginModel.CaptchaTextBox),
            new KeyValuePair<string, string>("ctl02_RadCaptcha1_ClientState", loginModel.ctl02_RadCaptcha1_ClientState),
            new KeyValuePair<string, string>("ctl02$btnLogin", loginModel.btnLogin)
        });

            var response = await _httpClient.PostAsync(_baseUrl, content);
            var html = await response.Content.ReadAsStringAsync();

            await GetInfo(html);

            return html;
        }

        public async Task GetInfo(string txt)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(txt);
            string ogrenciNo = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtOgrenciNo']")?.InnerText.Trim();
            string adSoyad = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtAdiSoyadi']")?.InnerText.Trim();
            string fakulte = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtFakulteAdi']")?.InnerText.Trim();
            string bolum = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtBolumAdi']")?.InnerText.Trim();
            string sinifGano = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_txtSinifSeneGano']")?.InnerText.Trim();
            string danisman = doc.DocumentNode.SelectSingleNode("//span[@id='Banner1_Kullanici1_labelBirinciDanisman']")?.InnerText.Trim();


        }

        public void RemoveImageFolder()
        {
            string imgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "img");

            if (Directory.Exists(imgFolderPath))
            {

                foreach (string file in Directory.GetFiles(imgFolderPath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        continue;
                        
                    }

                   
                }


            }
        }
    }
}







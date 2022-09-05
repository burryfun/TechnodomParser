using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Net;

string[] brands = { "apple", "samsung", "huawei", "sony" };
string URL = $"https://www.technodom.kz/catalog/smartfony-i-gadzhety/smartfony-i-telefony/smartfony/f/brands";
int DELAY = 500; // msec

List<DownoloadUrl> downloadUrls = new List<DownoloadUrl>();

/*
 * Finding images
 */
using (WebDriver driver = new ChromeDriver())
{

    foreach (var brand in brands)
    {
        string brandURL = $"{URL}/{brand}";
        driver.Navigate().GoToUrl(brandURL);

        var pages = driver.FindElements(By.CssSelector("div.Paginator__List-Item"));

        if (pages.Count() == 0)
        {
            var totalOnPage = SaveImagesUrlsByBrand(driver, brand);
            Console.WriteLine($"Images for {brand} from 1 page: {totalOnPage}");
        }
        else
        {
            for (int i = 0; i < pages.Count; i++)
            {

                driver.Navigate().GoToUrl($"{brandURL}?page={i + 1}");
                Thread.Sleep(DELAY);

                var totalOnPage = SaveImagesUrlsByBrand(driver, brand);

                Console.WriteLine($"Images for {brand} from {i + 1} page: {totalOnPage}");

            }
        }
    }
}

int SaveImagesUrlsByBrand(WebDriver driver, string brand)
{
    int total = 0;
    ScrollToEndPage(driver);

    var images = driver.FindElements(By.CssSelector("img.LazyImage__Source"));

    foreach (var image in images)
    {
        string title = image.GetAttribute("title").ToLower();
        string imageUrl = image.GetAttribute("src");

        if (title.IndexOf(brand) == -1)
        {
            continue;
        }

        downloadUrls.Add(new DownoloadUrl { Brand = brand, Title = title, Url = imageUrl });
        total++;
    }
    return total;
}

/*
  * Downloading images
  */
using (WebClient client = new WebClient())
{
    int i = 1;
    foreach (var url in downloadUrls)
    {
        string directoryPath = $"{System.IO.Directory.GetCurrentDirectory()}/images/{url.Brand}";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string imagePath = $"{directoryPath}/{url.Title}.jpg";
        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"Downloading {url.Title} ...");
            client.DownloadFile(url.Url, imagePath);
            Console.WriteLine($"Successfully Downloaded File {url.Title}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"File {imagePath} already exists");
            Console.ResetColor();
        }
    }
}


void ScrollToEndPage(WebDriver driver)
{
    var lastPageHeight = driver.ExecuteScript("return document.documentElement.scrollHeight/2;");
    while (true)
    {
        driver.ExecuteScript($"window.scrollTo(0,{lastPageHeight});");
        var newPageHeight = driver.ExecuteScript("return document.documentElement.scrollHeight;");
        Thread.Sleep(DELAY);

        if (newPageHeight.Equals(lastPageHeight))
        {
            break;
        }
        lastPageHeight = newPageHeight;
    }
}
struct DownoloadUrl
{
    public string Brand;
    public string Title;
    public string Url;
}
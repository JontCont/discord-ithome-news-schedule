// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Net;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;

// 建立 Browser 的配置
var config = AngleSharp.Configuration.Default
    .WithDefaultLoader()
    .WithDefaultCookies();

// 根據配置建立出我們的 Browser 
var browser = BrowsingContext.New(config);

// 這邊用的型別是 AngleSharp 提供的 AngleSharp.Dom.Url
var url = new Url("https://www.ithome.com.tw/news");

// 使用 OpenAsync 來打開網頁抓回內容
var doc = await browser.OpenAsync(url);
var htmls = doc.QuerySelector("div.view-display-id-news");

//dc : 變數
string webhook = "https://discord.com/api/webhooks/1175284326926135316/psoddy44BeuMPJzNNmrlMTPT_I0xcPKgrA4m9ra33x_8UsfnVeXE8_j-OQVLsykEk-U_";
var contentEmbed = new List<Embed>();

//標題
var header = htmls.QuerySelector("div.view-header");
var headerlink = header?.QuerySelector("a")?.GetAttribute("href");
var headerphoto = header?.QuerySelector("img")?.GetAttribute("src");
var headertitle = header?.QuerySelector("div.title a")?.TextContent;
var headertext = header?.QuerySelector("div.summary p")?.TextContent;
contentEmbed.Add(new Embed
{
    title = "【快訊】" + headertitle ?? "",
    description = headertext ?? "",
    color = 0x00ff00,
    url = $"https://www.ithome.com.tw{headerlink}",
    image = new Image
    {
        url = headerphoto ?? ""
    },
    footer = new Footer
    {
        text = "Powered by ITHome",
        icon_url = "https://s4.itho.me/sites/default/files/ithome_logo_0.png"
    }
});

//明細處理
var contents = htmls.QuerySelectorAll("div.view-content div.span4.channel-item");
var detailstr = string.Empty;
foreach (var row in contents.Select((x, index) => new { data = x, index }))
{
    DateTime dateTime;
    var content = row.data;
    var items = content.QuerySelector("div.item");
    var link = items?.QuerySelector("p.title a")?.GetAttribute("href");
    var photo = items?.QuerySelector("p.photo a img")?.GetAttribute("src");
    var title = items?.QuerySelector("p.title")?.TextContent;
    string format = "yyyy-MM-dd";
    detailstr += $"☕ - {row.index + 1} [{title?.Trim()}](https://www.ithome.com.tw{link})\n";
    if ((row.index + 1) == 5)
    {
        contentEmbed.Add(new Embed
        {
            title = $"其他資訊- {(row.index + 1) / 5}",
            description = detailstr,
            color = 0x00ff00,
            footer = new Footer
            {
                text = "Powered by ITHome",
                icon_url = "https://s4.itho.me/sites/default/files/ithome_logo_0.png"
            }
        });
        detailstr = "";
        break;
    }
}


if (contentEmbed.Count() > 1 || !string.IsNullOrEmpty(detailstr))
{
    if (contentEmbed.Count() <= 1)
    {
        contentEmbed.Add(new Embed
        {
            title = $"其他資訊- 1",
            description = detailstr,
            color = 0x00ff00,
            footer = new Footer
            {
                text = "Powered by ITHome",
                icon_url = "https://s4.itho.me/sites/default/files/ithome_logo_0.png"
            }
        });
        detailstr = "";
    }

    SendMessage(new messages
    {
        content = $"【系統】IThome 新聞快訊\n",
        embeds = contentEmbed
    }, webhook);
}

static void SendMessage(messages json, string webhook)
{
    WebClient client = new WebClient();
    client.Headers.Add("Content-Type", "application/json");
    string payload = JsonConvert.SerializeObject(json);
    client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
}

static DateTime GetDateTimeNow()
{
    DateTime dateTime = DateTime.Now;
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
    return TimeZoneInfo.ConvertTime(dateTime, timeZone);
}
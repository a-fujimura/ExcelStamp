using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ExcelStamp.Controllers
{
    public class StampController : Controller
    {
        string pnginkan_red = string.Empty;
        string pnginkan_brack = string.Empty;
        private readonly IWebHostEnvironment environment;
        public StampController(IWebHostEnvironment environment)
        {
            this.environment = environment;
            pnginkan_red = Path.Combine(environment.WebRootPath, "stamp/inkan-red.png");
            pnginkan_brack = Path.Combine(environment.WebRootPath, "stamp/inkan-black.png");
        }
        [Route("api/stamp/getstamp/{compName}/{userName}/{authority}/{date}")]
        public IActionResult GetStamp(string compName, string userName, int authority, string date)
        {
            try
            {
                byte[] img;
                var userColor = authority > 0 ? pnginkan_red : pnginkan_brack;
                var userFontColor = authority > 0 ? Brushes.Red : Brushes.Black;
                using (var userStamp = CreateStamp(userColor, userFontColor, compName, userName, date))
                using (var ms = new MemoryStream())
                {
                    userStamp.Save(ms, ImageFormat.Png);
                    img = ms.GetBuffer();
                }
                return File(img, "image/png", $"{compName}_{userName}_{authority}_{date}.png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        Bitmap CreateStamp(string path, Brush fontColor, string compName, string name, string dateTime)
        {
            var rslt = new Bitmap(path);
            using (var g = Graphics.FromImage(rslt))
            using (var stringFormat = new StringFormat(StringFormat.GenericTypographic))
            using (var font = new Font("Arial", 15, FontStyle.Bold))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                var compSize = g.MeasureString(compName, font, 1000, stringFormat);
                var dateSize = g.MeasureString(dateTime, font, 1000, stringFormat);
                var nameSize = g.MeasureString(name, font, 1000, stringFormat);
                g.DrawString(compName, font, fontColor, rslt.Width / 2 - compSize.Width / 2, rslt.Height / 6 * 1 - compSize.Height / 2 - -5, stringFormat);
                g.DrawString(dateTime, font, fontColor, rslt.Width / 2 - dateSize.Width / 2, rslt.Height / 6 * 3 - dateSize.Height / 2 - -2, stringFormat); ;
                g.DrawString(name, font, fontColor, rslt.Width / 2 - nameSize.Width / 2, rslt.Height / 6 * 5 - nameSize.Height / 2 - 0, stringFormat);
                rslt.MakeTransparent(Color.White);
            }
            return rslt;
        }
    }
}
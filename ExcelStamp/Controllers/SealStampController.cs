using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
namespace ExcelStamp.Controllers
{
    public class SealStampController : Controller
    {
        private readonly IWebHostEnvironment environment;
        public SealStampController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }
        [HttpPost]
        [Route("api/sealstamp")]
        public async Task<IActionResult> SealStamp(IFormFile[] file)
        {
            var guid = Guid.NewGuid();
            try
            {
                var orgFileName = Path.Combine(environment.WebRootPath + @"\excel", "test.xlsx");
                var newFileName = Path.Combine(environment.WebRootPath + @"\delete", $"{guid}.xlsx"); ;
                System.IO.File.Copy(orgFileName, newFileName);
                using (var stampMs = new MemoryStream())
                {
                    await file[0].CopyToAsync(stampMs);
                    var stamp = stampMs.ToArray();
                    var anchor = new XSSFClientAnchor()
                    {
                        AnchorType = AnchorType.DontMoveAndResize,
                        Row1 = 0,
                        Row2 = 4,
                        Col1 = 0,
                        Col2 = 2,
                        Dx1 = XSSFShape.EMU_PER_PIXEL * 5,
                        Dx2 = XSSFShape.EMU_PER_PIXEL * -5,
                        Dy1 = XSSFShape.EMU_PER_PIXEL * 5,
                        Dy2 = XSSFShape.EMU_PER_PIXEL * -5,
                    };
                    SealStamp(newFileName, newFileName, "Sheet1", anchor, stamp);
                }
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        protected void SealStamp(string inpath, string outpath, string sheetName, XSSFClientAnchor anchor, byte[] data)
        {
            var book = WorkbookFactory.Create(inpath);
            var sheet = book.GetSheet(sheetName); // ←既存のエクセルシートを取得する場合
            var pictureidx = book.AddPicture(data, (PictureType)XSSFWorkbook.PICTURE_TYPE_PNG);
            var patriarch = sheet.CreateDrawingPatriarch();
            patriarch.CreatePicture(anchor, pictureidx);
            //ブックを保存
            using (var fs = new FileStream(outpath, FileMode.OpenOrCreate))
            {
                book.Write(fs, true);
            }
            book.Close(); // クローズ
        }
    }
}
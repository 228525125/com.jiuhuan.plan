using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.tools
{
    public class QRCodeHelper
    {
        public static string FILE_NAME = "qrcode.png";
        public static string FILE_NAME2 = "qrcode2.png";
        public static string FILE_NAME3 = "qrcode3.png";
        public static string GenerateQRCode(string text, string filePath, string fileName)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrImage = qrCode.GetGraphic(20, Color.Black, Color.White, false);
            qrImage.Save(filePath+ fileName, ImageFormat.Png);
            return filePath + fileName;
        }
    }
}

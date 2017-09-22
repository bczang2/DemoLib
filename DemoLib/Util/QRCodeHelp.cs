using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace DemoLib.Util
{
    public class QRCodeHelp
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="qrString">二维码内容</param>
        /// <param name="character">编码</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="margin">设置二维码的边距,单位不是固定像素</param>
        /// <param name="level">容错级别</param>
        /// <returns></returns>
        public static Bitmap CreateQRCode(string qrString, string character, int width, int height, int margin = 0, string level = "H")
        {
            try
            {
                BarcodeWriter writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                //可选参数
                QrCodeEncodingOptions options = new QrCodeEncodingOptions();
                options.DisableECI = true;
                options.CharacterSet = character;
                options.Width = width;
                options.Height = height;
                options.Margin = margin;
                options.ErrorCorrection = InitialErrorCorrection(level);
                writer.Options = options;

                Bitmap map = writer.Write(qrString);
                return map;
            }
            catch (Exception ex) { }

            return null;
        }

        private static ErrorCorrectionLevel InitialErrorCorrection(string level)
        {
            ErrorCorrectionLevel ret = ErrorCorrectionLevel.H;
            if (!string.IsNullOrWhiteSpace(level))
            {
                switch (level)
                {
                    case "H":
                        ret = ErrorCorrectionLevel.H;
                        break;
                    case "M":
                        ret = ErrorCorrectionLevel.M;
                        break;
                    case "L":
                        ret = ErrorCorrectionLevel.L;
                        break;
                    case "Q":
                        ret = ErrorCorrectionLevel.Q;
                        break;
                }
            }

            return ret;
        }
    }
}

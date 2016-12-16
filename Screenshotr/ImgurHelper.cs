using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Screenshotr
{
    internal static class ImgurHelper
    {
        private const string clientID = "232e814b1d6d717";

        public static void UploadToImgur(Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            var bitmapBytes = ms.ToArray();
            var base64Image = Convert.ToBase64String(bitmapBytes);

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", $"Client-ID {clientID}");

                var values = new NameValueCollection
                {
                    {"image", base64Image}
                };

                var responseBytes = webClient.UploadValues("https://api.imgur.com/3/upload.json", values);
                var response = Encoding.UTF8.GetString(responseBytes);
                dynamic jsonData = JObject.Parse(response);
                if (jsonData.success.Value)
                    Process.Start(jsonData.data.link.Value);
            }
        }
    }
}
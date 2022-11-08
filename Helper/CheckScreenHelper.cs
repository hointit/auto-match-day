using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KAutoHelper;
using System.Threading;
using System.Drawing;

namespace MatchDay.Helper
{
    public static class CheckScreenHelper
    {
        private static string tempPath = "E:/temp";

        private static string cmdSreenCap = "adb -s {0} shell \"screencap -p '/sdcard/screenshot.png'\"";

        private static string cmdPullFile = "adb -s {0} pull /sdcard/screenshot.png E:\\temp";

        private static string cmdGetCurrentWindow = "adb -s {0} shell \"dumpsys window windows | grep -E 'mCurrentFocus'\"";

        private static string gamePackageName = "com.playsportgames.football";

        private static bool isGameActivity (string deviceId)
        {
            string cmd = string.Format(cmdGetCurrentWindow, deviceId);
            string result = KAutoHelper.ADBHelper.ExecuteCMD(deviceId);
            return result.Contains(gamePackageName);
        }


        public static bool isMainScreenGame (string deviceId)
        {
            ADBHelper.SetADBFolderPath("D:\\Program Files\\Nox\\bin");
            string a  = ADBHelper.ExecuteCMD(string.Format(cmdSreenCap, deviceId));
            string b = ADBHelper.ExecuteCMD(string.Format(cmdPullFile, deviceId));
            var screen = new Bitmap(tempPath + "/screenshot.png");

            var ad = ADBHelper.ScreenShoot(deviceId);

            var homeCheck = new Bitmap("Images/home-check.png");

            var hasPostion = KAutoHelper.ImageScanOpenCV.FindOutPoint(ad, homeCheck);

            return hasPostion != null;
        }

    }
}

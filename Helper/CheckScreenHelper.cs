using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KAutoHelper;
using System.Threading;
using System.Drawing;
using System.Security.Permissions;

namespace MatchDay.Helper
{
    public static class CheckScreenHelper
    {
        private static string tempPath = "E:/temp";

        private static string cmdSreenCap = "adb -s {0} shell \"screencap -p '/sdcard/screenshot.png'\"";

        private static string cmdPullFile = "adb -s {0} pull /sdcard/screenshot.png E:\\temp";

        private static string cmdGetCurrentWindow = "adb -s {0} shell \"dumpsys window windows | grep -E 'mCurrentFocus'\"";

        private static string gamePackageName = "com.playsportgames.football";

        public static bool isGameActivity (string deviceId)
        {
            string result = KAutoHelper.ADBHelper.ExecuteCMD(string.Format(cmdGetCurrentWindow, deviceId));
            return result.Contains(gamePackageName);
        }


        public static bool isMainScreenGame (string deviceId)
        {
            ADBHelper.SetADBFolderPath(MainWindow.ADB_FOLDER_PATH);
            string fileName = deviceId.Substring(deviceId.Length -4, 4);
            var screen = ADBHelper.ScreenShoot(deviceId, true, $"homeCheck{deviceId}.png");

            var homeCheck = new Bitmap("Images/home-check.png");

            var hasPostion = ImageScanOpenCV.FindOutPoint(screen, homeCheck);

            return hasPostion != null;
        }

        public static Point? GetPostionStartNewGame (string deviceId)
        {
            ADBHelper.SetADBFolderPath(MainWindow.ADB_FOLDER_PATH);
            string fileName = deviceId.Substring(deviceId.Length - 4, 4);
            var screen = ADBHelper.ScreenShoot(deviceId, true, $"startNewGame{fileName}.png");

            var startNewGame = new Bitmap("Images/start-new-game.png");

            return ImageScanOpenCV.FindOutPoint(screen, startNewGame);
        }

        public static bool CheckStep (string deviceId, string step)
        {
            ADBHelper.SetADBFolderPath(MainWindow.ADB_FOLDER_PATH);
            string fileName = deviceId.Substring(deviceId.Length - 4, 4);
            var screen = ADBHelper.ScreenShoot(deviceId, true, $"step{step}-{fileName}.png");

            var stepImage = new Bitmap($"Images/{step}.png");
            var hasPostion = ImageScanOpenCV.FindOutPoint(screen, stepImage);

            return hasPostion != null;
        }
    }
}

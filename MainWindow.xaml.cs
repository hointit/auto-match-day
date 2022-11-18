using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ADBHelper = KAutoHelper.ADBHelper;
using KAutoHelper;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using System.Timers;
using MatchDay.Helper;

namespace MatchDay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread thread1;
        private Thread thread2;
        private Thread thread3;

        private int countDevice2 = 0;
        private int countTotalDevice2 = 0;
        private bool readyToRestartDevice2 = false;
        private int countDevice3 = 0;
        private int countTotalDevice3 = 0;
        private bool readyToRestartDevice3 = false;

        private string deviceId2 = "";
        private string deviceId3 = "";

        private string urlInvite = "";

        private string cmdOpenBrowser = "adb -s {0} shell \"am start -a android.intent.action.VIEW -d {1}\"";


        private string cmdOpenGame =
                    "adb -s {0} shell \"am start -n com.playsportgames.football/com.google.firebase.MessagingUnityPlayerActivity\"";
        private string cmdKillGame = "adb -s {0} shell \"am force-stop com.playsportgames.football\"";
        private string cmdKillBrowser = "adb -s {0} shell \"am force-stop com.android.browser\"";

        private string cmdGetCurrentWindow = "adb -s {0} shell \"dumpsys window windows | grep -E 'mCurrentFocus'\"";

        private int TIME_TO_RESET = 30;

        private bool hasComingCoin = false;

        private DateTime lastComingCoin = DateTime.Now;

        private int totalCoinEarn = 0;

        public static string ADB_FOLDER_PATH = "";

        private int MAX_TRY = 10;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ADB_FOLDER_PATH = this.txtPathToADB.Text;
            ADBHelper.SetADBFolderPath(ADB_FOLDER_PATH);
            var devices = KAutoHelper.ADBHelper.GetDevices();

            if (devices.Count > 0)
            {
                this.device1.Text = devices[0];
            }

            if (devices.Count > 1)
            {
                this.device2.Text = devices[1];
            }

            if (devices.Count > 2)
            {
                this.device3.Text = devices[2];
            }
        }

        private void AutoResetGame (string deviceId)
        {
            if (!CheckScreenHelper.isGameActivity(deviceId))
            {
                ADBHelper.ExecuteCMD(String.Format(this.cmdOpenGame,deviceId));
                Thread.Sleep(2000);
            }

            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.isMainScreenGame(deviceId))
                {
                    ADBHelper.TapByPercent(deviceId, 88.0, 4.0);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}]Waiting for load main screen game: count = {i + 1}");
                    Thread.Sleep(5000);
                }
            }

            // Click Setting
            ADBHelper.TapByPercent(deviceId, 37.7, 16.8);

            Thread.Sleep(1500);

            // scroll down and find the but ton start new game
            ADBHelper.SwipeByPercent(deviceId, 50, 80, 50, 10);
            Thread.Sleep(1000);
            // stop scroll

            for (int i = 0; i < MAX_TRY; i ++)
            {
                var startNewGamePositon = CheckScreenHelper.GetPostionStartNewGame(deviceId);
                if (startNewGamePositon != null)
                {
                    ADBHelper.Tap(deviceId, startNewGamePositon.Value.X, startNewGamePositon.Value.Y);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}]Can't find the button start new game: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            Thread.Sleep(1000);
            // Click confirm clear data
            ADBHelper.TapByPercent(deviceId, 70, 65);

            // waiting for reset data
            Thread.Sleep(10000);

            // check step1. screen load new game
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step1"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 95);
                    Thread.Sleep(2000);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for load new game: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(4000);
                }
                
            }

            // step 2: Tab to continute. confirm kit
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step2"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 95);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for confirm kit 1st: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            // step 3:  Tab to continute. confirm kit again
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step3"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 95);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for confirm kit 2nd: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            // step 4: Confirm kit (final confirm kit and club's name)
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step4"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 92);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for final confirm kit and club's name: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            // step 5: Tab to continute. confirm stadium plans
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step5"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 95);
                    Thread.Sleep(1500);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for confirm stadium plans: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            // step 6: confirm location
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step6"))
                {
                    // Tab to continute
                    ADBHelper.TapByPercent(deviceId, 50, 92);

                    // waiting for load screen NEWS
                    Thread.Sleep(10000);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for confirm stadium plans: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(1000);
                }
            }

            // step 7: Continute screen NEWS, go to main screen
            for (int i = 0; i < MAX_TRY; i++)
            {
                if (CheckScreenHelper.CheckStep(deviceId, "step7"))
                {
                    // Tab to continute (xác nhận màn hình tin tức)
                    ADBHelper.TapByPercent(deviceId, 50, 95);
                    Thread.Sleep(2000);
                    break;
                }
                else
                {
                    WriteLog($"[{deviceId}] Waiting for confirm stadium plans: count = {i + 1}");
                    if (i == MAX_TRY - 1)
                    {
                        SetStopThread(deviceId);
                    }
                    Thread.Sleep(3000);
                }
            }

            while(true)
            {
                if (hasComingCoin)
                {

                    var remainingTime = (DateTime.Now - this.lastComingCoin).TotalSeconds;
                    var seconds = Convert.ToInt32(remainingTime);
                    var timeToSleep = 15000 - (seconds * 1000);
                    if (timeToSleep <= 0)
                    {
                        timeToSleep = 15000;
                    }
                    WriteLog($"[{deviceId}] There are some coins that have not been received, please wait: {timeToSleep/1000}s");
                    Thread.Sleep(timeToSleep);
                }
                else
                {
                    ADBHelper.ExecuteCMD(string.Format(cmdOpenBrowser, deviceId, this.urlInvite));
                    Thread.Sleep(3000);
                    this.lastComingCoin = DateTime.Now;
                    this.hasComingCoin = true;
                    break;
                }
            }
            DispatcherAction(() => updateCount(deviceId)) ;
        }

        private void SetStopThread(string deviceId)
        {
            for (int i  = 0; i < MAX_TRY; i ++)
            {
                ADBHelper.TapByPercent(deviceId, 50, 95);
                Thread.Sleep(2000);
            }
            
            if (deviceId == this.deviceId2)
            {
                this.readyToRestartDevice2 = true;
                DispatcherAction(StopAutoDevice2);
                return;
            }

            if (deviceId == this.deviceId3)
            {
                this.readyToRestartDevice3 = true;
                DispatcherAction(StopAutoDevice3);
                return;
            }
        }

        private void AutoReciveCoin (string deviceId)
        {
            ADBHelper.TapByPercent(deviceId, 50, 86);

            Thread.Sleep(3000);
            
            var isChooserActivity = ADBHelper.ExecuteCMD(string.Format(this.cmdGetCurrentWindow, deviceId));

            if(isChooserActivity.Contains("com.android.internal.app.ChooserActivity"))
            {
                // Click bluetooth
                ADBHelper.TapByPercent(deviceId, 24, 90);
                Thread.Sleep(2000);
            }

            ADBHelper.ExecuteCMD(String.Format(this.cmdOpenGame, deviceId));

            // Click nhận tiền
            Thread.Sleep(4000);
            ADBHelper.TapByPercent(deviceId, 50, 80);

            totalCoinEarn += 15;
            DispatcherAction(UpdateTotalCoinToUI);
            Thread.Sleep(1000);
            this.hasComingCoin = false;
        }

        private void UpdateTotalCoinToUI ()
        {
            this.lbTotalCoint.Content = totalCoinEarn;
        }

        private void btnDevice1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.urlInvite))
            {
                MessageBox.Show("Please confirm URL invite!!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string device1 = this.device1.Text;
            this.btnDeviceStop1.IsEnabled = true;
            this.btnDevice1.IsEnabled = false;
            this.txtTimeToReset.IsEnabled = false;
            this.device1.IsEnabled = false;
            this.urlInvite = this.txtUrlInvite.Text;

            this.TIME_TO_RESET = Convert.ToInt32(this.txtTimeToReset.Text);
            this.thread1 = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    if (hasComingCoin)
                    {
                        AutoReciveCoin(device1);
                        this.hasComingCoin = false;
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }

                }
            });
            thread1.Start();
        }

        private void btnDevice2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.urlInvite))
            {
                MessageBox.Show("Please confirm URL invite!!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetStatusButtonDevice2ToRunning();
            this.deviceId2 = this.device2.Text;
            StartThreadDevice2(this.deviceId2);
            WaitingForRestartGame(this.deviceId2);
        }

        private void SetStatusButtonDevice2ToRunning()
        {
            this.btnDevice2.IsEnabled = false;
            this.btnDeviceStop2.IsEnabled = true;
            this.device2.IsEnabled = false;
        }


        private void StartThreadDevice2(string deviceId)
        {
            thread2 = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    AutoResetGame(deviceId);
                    Thread.Sleep(3000);
                    if (this.countDevice2 >= TIME_TO_RESET)
                    {
                        this.readyToRestartDevice2 = true;
                        DispatcherAction(StopAutoDevice2);
                        break;
                    }
                }
            });
            thread2.Start();
        }

        private void btnDevice3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.urlInvite))
            {
                MessageBox.Show("Please confirm URL invite!!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetStatusButtonDevice3ToRunning();
            this.deviceId3 = this.device3.Text;
            StartThreadDevice3(this.deviceId3);
            WaitingForRestartGame(this.deviceId3);
        }

        private void SetStatusButtonDevice3ToRunning()
        {
            this.btnDevice3.IsEnabled = false;
            this.btnDeviceStop3.IsEnabled = true;
            this.device3.IsEnabled = false;
        }

        private void StartThreadDevice3(string deviceId)
        {
            
            this.thread3 = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    AutoResetGame(deviceId);
                    Thread.Sleep(3000);
                    if (this.countDevice3 >= TIME_TO_RESET)
                    {
                        this.readyToRestartDevice3 = true;
                        DispatcherAction(StopAutoDevice3);
                    }
                }
            });
            thread3.Start();
        }

        private void btnDeviceStop1_Click(object sender, RoutedEventArgs e)
        {
            this.btnDevice1.IsEnabled = true;
            this.btnDeviceStop1.IsEnabled = false;
            this.txtTimeToReset.IsEnabled = true;
            this.device1.IsEnabled = true;
            thread1.Abort();
        }

        private void btnDeviceStop2_Click(object sender, RoutedEventArgs e)
        {
            StopAutoDevice2();
        }

        private void StopAutoDevice2()
        {
            this.btnDevice2.IsEnabled = true;
            this.btnDeviceStop2.IsEnabled = false;
            this.device2.IsEnabled = true;
            if (thread2 != null)
            {
                thread2.Abort();
                thread2.Interrupt();
                thread2 = null;
            }
            
        }

        private void btnDeviceStop3_Click(object sender, RoutedEventArgs e)
        {
            StopAutoDevice3();
        }

        private void StopAutoDevice3()
        {
            this.btnDevice3.IsEnabled = true;
            this.btnDeviceStop3.IsEnabled = false;
            this.device3.IsEnabled = true;
            thread3.Abort();
            thread3 = null;
        }

        private void btnDeviceForceOpenLink2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.urlInvite))
            {
                MessageBox.Show("Please confirm URL invite!!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string deviceId = this.device2.Text;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ADBHelper.ExecuteCMD(string.Format(cmdOpenBrowser, deviceId, this.urlInvite));
                Thread.Sleep(3000);
            }).Start();

        }

        private void btnDeviceForceOpenLink3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.urlInvite))
            {
                MessageBox.Show("Please confirm URL invite!!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string deviceId = this.device3.Text;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ADBHelper.ExecuteCMD(string.Format(cmdOpenBrowser, deviceId, this.urlInvite));
                Thread.Sleep(3000);
            }).Start();
        }

        #region Reset game
        
        
        private void WaitingForRestartGame(string deviceId)
        {
            new Thread(() =>
            {
                while (true)
                {
                    if ((deviceId == this.deviceId2 && this.readyToRestartDevice2) ||
                    (deviceId == this.deviceId3 && this.readyToRestartDevice3))
                    {
                        RestartGame(deviceId);

                        if (deviceId == this.deviceId2)
                        {
                            this.readyToRestartDevice2 = false;
                            this.countDevice2 = 0;
                            StartThreadDevice2(deviceId);
                            DispatcherAction(SetStatusButtonDevice2ToRunning);
                        }
                        if (deviceId == this.deviceId3)
                        {
                            this.readyToRestartDevice3 = false;
                            this.countDevice3 = 0;
                            StartThreadDevice3(deviceId);
                            DispatcherAction(SetStatusButtonDevice3ToRunning);
                        }

                        var action = new Action(() =>
                        {
                            WriteLog("Start Auto device " + deviceId);
                            this.lbCountDevice2.Content = this.countDevice2;
                            this.lbCountDevice3.Content = this.countDevice3;
                        });

                        DispatcherAction(action);
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }).Start();
        }


        #endregion

        private void RestartGame (string deviceId)
        {
            WriteLog("Stop game device: " + deviceId);
            ADBHelper.ExecuteCMD(string.Format(cmdKillGame, deviceId));
            
            // nghỉ 2s sau khi tắt game
            Thread.Sleep(2000);

            WriteLog("Start game device: " + deviceId);
            ADBHelper.ExecuteCMD(string.Format(cmdOpenGame, deviceId));

            WriteLog(deviceId + "  Waiting to ready (about 1 min)....");

            // nghỉ 1 phút sau khi mở app
            Thread.Sleep(60000);

            WriteLog("Ready Device: " + deviceId);
        }

        private void WriteLog(string mesage)
        {
            Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            this.txtLog.AppendText(DateTime.Now.ToString("[dd/MM/yyyy HH:mm:ss]:  ") + mesage + "\n");
                            this.txtLog.ScrollToEnd();
                        }));
        }

        private void updateCount (string deviceId)
        {
            if (deviceId == this.deviceId2)
            {
                this.countDevice2++;
                this.countTotalDevice2++;
                this.lbCountDevice2.Content = $"{this.countDevice2}/{this.countTotalDevice2}";
                return;
            }

            if (deviceId == this.deviceId3)
            {
                this.countDevice3++;
                this.countTotalDevice3++;
                this.lbCountDevice3.Content = $"{this.countDevice3}/{this.countTotalDevice3}";
                return;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.lbCountDevice2.Content = "0/0";
            this.lbCountDevice3.Content = "0/0";
        }

        private void DispatcherAction(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
        }

        private void btnConfirmUrl_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtUrlInvite.IsEnabled)
            {
                this.urlInvite = this.txtUrlInvite.Text;
                this.txtUrlInvite.IsEnabled = false;
                this.btnConfirmUrl.Content = "Edit Url";
            } else
            {
                this.txtUrlInvite.IsEnabled = true;
                this.txtUrlInvite.Focus();
                this.txtUrlInvite.SelectAll();
                this.btnConfirmUrl.Content = "Confirm Url";
            }
           
        }

        private void btnTestFeature_Click(object sender, RoutedEventArgs e)
        {
            var a = CheckScreenHelper.GetPostionStartNewGame("emulator-5554");
        }
    }
}

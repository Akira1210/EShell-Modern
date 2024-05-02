using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Timers;
using Windows.Devices.Midi;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Xml.Linq;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Composition;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using Windows.System;
using System.Xml;
using System.Linq.Expressions;
using System.Drawing;
using Windows.UI;
using Windows.Storage;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Size = Windows.Foundation.Size;
using Windows.Security.Cryptography.Core;
using Windows.UI.Core;
using System.Reflection;
using Windows.ApplicationModel.Core;
using System.Collections;
using Windows.UI.Popups;
using Windows.Devices.Power;
using Windows.System.Power;
using Windows.Media;
using Windows.Media.Audio;
using Rectangle = Windows.UI.Xaml.Shapes.Rectangle;
using System.Data.SqlTypes;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using Windows.Devices.Lights.Effects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security.Cryptography;
using Windows.Foundation.Metadata;

namespace EShell_Modern
{
    /// <summary>
    /// Add Settings File for Pinned Apps - InPro
    /// Add Run Dialog
    /// Fix PackageFind: Many Stub Apps Present(IsFramework Check?)
    /// Add StartMenu Win32 Shortcuts to All Apps (Users/Appdata/Microsoft/Windows/StartMenu) - InPro
    /// Add StatusBar - InPro
    /// Add Taskbar?
    /// Prevent Minimizing
    /// Force Maximize?
    /// Sort AllAppList by Name
    /// Fix App Tiles changing color when mouse over
    /// Create own Background
    /// Fix Battery Percentage invisible when DarkMode active
    /// 
    /// Replace Int Cases 
    /// </summary>
    public sealed partial class MainPage : Page

    {
        ArrayList appPath = new ArrayList();
        ArrayList w32Apps = new ArrayList(); //WIP
        ArrayList allApps = new ArrayList();
        int ActivePage = 0; // 0 = Pinned Apps | 1 = All Apps
        bool bg = false; // Use Background Image?
        int batterystatus = 0;
        int batterypercentage = 0;
        bool statuschange = false;
        bool tip = true;
        int AppCount = 0;
        bool allAppsLoaded = false;

        ImageBrush battery = new ImageBrush();
        MenuFlyout foBattery = new MenuFlyout();
        ContentDialog cdConfig = new ContentDialog();
        Brush DefaultMainBg;
        CheckBox useBackground = new CheckBox();


        public MainPage()
        {
            this.InitializeComponent();
            calFlyout.Visibility = Visibility.Collapsed;
            tbVer.Text = SetVersionLabel();
            DispatcherTimer tClock = new DispatcherTimer();
            tClock.Tick += tClock_Tick;
            tClock.Interval = TimeSpan.FromSeconds(1);
            tClock.Start();
            Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;
            RequestAggregateBatteryReport();
            DefaultMainBg = MainGrid.Background;
            createCdConfig();
            getAllApps();
            //LoadCfg();
            LoadStatusBar();
            if (bg) { changeBackground("ms-appx:///Assets/img0.jpg"); }
            LoadApps(0);
            InitAnim(0);
            setTip();

        }

        private String SetVersionLabel()
        {
            var ProcessArch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            var BuildDate = GetLinkerTimestampUtc(Assembly.GetExecutingAssembly()).ToString();
            BuildDate = BuildDate.Replace(":", "");
            BuildDate = BuildDate.Replace(".", "");
            BuildDate = BuildDate.Replace(" ", "_");

            return Assembly.GetExecutingAssembly().GetName().Name + "\nInternal pre1" + " \nBuild 21." + ProcessArch + ".pre1." + BuildDate + "\n" + System.Environment.OSVersion;
        }

        private void tClock_Tick(object sender, object e)
        {
            tbTimeDate.Text = System.DateTime.Now.ToString("HH:mm") + System.Environment.NewLine +
                System.DateTime.Now.Date.ToString("d");
            if (statuschange)
            {
                spStatus.Children.Clear();
                LoadStatusBar();
                statuschange = false;

            }
        }

        private void tbVer_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void cmdMainStart_Click(object sender, RoutedEventArgs e)
        {
            if (ActivePage == 1) InitAnim(1);
            if (ActivePage == 0) InitAnim(0);
            ActivePage = SwitchPage(ActivePage);
            LoadApps(ActivePage);
        }

        private void InitAnim(int direct)
        {
            TranslateTransform moveTransform = new TranslateTransform();

            CompositeTransform3D rotateProject = new CompositeTransform3D();

            ScaleTransform scaleTransformI = new ScaleTransform();

            if (direct == 0)
            {
                rotateProject.RotationX = -10;
                rotateProject.RotationY = 95;
                moveTransform.X = -100;
                moveTransform.Y = -20;
                scaleTransformI.ScaleX = 1.5;
            }
            if (direct == 1)
            {
                rotateProject.RotationX = -10;
                rotateProject.RotationY = -95;
                moveTransform.X = 100;
                moveTransform.Y = 20;
                scaleTransformI.ScaleX = 0.5;
            }


            // Objects for Anim.
            tbFrontPageLabel.RenderTransform = moveTransform;
            tbFrontPageLabel.Transform3D = rotateProject;
            tbFrontPageLabel.RenderTransform = scaleTransformI;
            lvMain.RenderTransform = moveTransform;
            lvMain.Transform3D = rotateProject;
            lvMain.RenderTransform = scaleTransformI;
            tbAppCount.RenderTransform = scaleTransformI;
            tbAppCount.Transform3D = rotateProject;
            tbAppCount.RenderTransform = moveTransform;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.7));
            DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
            DoubleAnimation myDoubleAnimationY = new DoubleAnimation();
            DoubleAnimation my3dRoationX = new DoubleAnimation();
            DoubleAnimation my3dRoationY = new DoubleAnimation();
            DoubleAnimation scaleTransformA = new DoubleAnimation();
            myDoubleAnimationX.Duration = duration;
            myDoubleAnimationY.Duration = duration;
            my3dRoationX.Duration = duration;
            my3dRoationY.Duration = duration;
            scaleTransformA.Duration = duration;
            Storyboard justintimeStoryboard = new Storyboard();
            justintimeStoryboard.Duration = duration;
            justintimeStoryboard.Children.Add(myDoubleAnimationX);
            justintimeStoryboard.Children.Add(myDoubleAnimationY);
            justintimeStoryboard.Children.Add(my3dRoationX);
            justintimeStoryboard.Children.Add(my3dRoationY);
            justintimeStoryboard.Children.Add(scaleTransformA);
            Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
            Storyboard.SetTarget(myDoubleAnimationY, moveTransform);
            Storyboard.SetTarget(my3dRoationX, rotateProject);
            Storyboard.SetTarget(my3dRoationY, rotateProject);
            Storyboard.SetTarget(scaleTransformA, scaleTransformI);
            Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
            Storyboard.SetTargetProperty(myDoubleAnimationY, "Y");
            Storyboard.SetTargetProperty(my3dRoationX, "rotateProject.RotationX");
            Storyboard.SetTargetProperty(my3dRoationY, "rotateProject.RotationY");
            Storyboard.SetTargetProperty(scaleTransformA, "scaleTransformI.ScaleX");

            myDoubleAnimationX.To = 0;
            myDoubleAnimationY.To = 0;
            my3dRoationX.To = 0;
            my3dRoationY.To = 0;
            scaleTransformA.To = 1;


            justintimeStoryboard.Begin();
        }


        private async void CreateAppTileAsync(AppListEntry package, int Type) //Type 0 = UWP App   Type 1 = w32 App
        {

            var cmdApp = new Button();
            var ImgLogo = new ImageBrush();
            try
            {
                var appLogo = package.DisplayInfo.GetLogo(new Size(100, 100));
                var appLogo2 = await appLogo.OpenReadAsync();
                BitmapImage b = new BitmapImage();
                b.SetSource(appLogo2.CloneStream());
                ImgLogo.ImageSource = b;
                ImgLogo.Stretch = Stretch.Uniform;
                cmdApp.Background = ImgLogo;
            }
            catch (Exception)
            { }
            cmdApp.Width = 150;
            cmdApp.Height = 140;
            cmdApp.Content = package.AppInfo.DisplayInfo.DisplayName.ToString();
            cmdApp.Tag = package.AppInfo.Package.Id.FullName;

            cmdApp.VerticalAlignment = VerticalAlignment.Top;
            cmdApp.VerticalContentAlignment = VerticalAlignment.Bottom;
            cmdApp.BorderBrush = new SolidColorBrush(Colors.Gray);
            if (Type == 0)
            {
                cmdApp.Click += AppClick;
                cmdApp.RightTapped += AppRightClick;
            }
            if (Type == 1)
            {
                cmdApp.Click += w32AppClick;
                cmdApp.RightTapped += w32AppRightClick;
            }
            allApps.Add(cmdApp);
            if (ActivePage == 1)
            {
                AppCount++;
                var tbApps = new TextBlock();
                tbAppCount.Foreground = new SolidColorBrush(Colors.Gray);
                tbAppCount.Text = AppCount + " Apps installed";
            }


        }

        private void getAllApps()
        {
            AppCount = 0;
            try
            {
                var packageManager = new PackageManager();
                IEnumerable<Package> packages = packageManager.FindPackagesForUser("");
                //var TTileCreate = new Task(() =>
                //{
                foreach (Package packageUp in packages)
                {
                    var package = packageUp.GetAppListEntries().ToList();
                    foreach (AppListEntry packageDown in package)
                    //if (!package.IsFramework || !package.IsResourcePackage || !package.IsStub || !package.IsOptional)
                    {
                        CreateAppTileAsync(packageDown, 0);

                    }
                }



                //});
                //TTileCreate.Start();

                //TTileCreate.Wait();


                //w32 Apps //allApps

                //add win32 shortcut paths to List

                foreach (String path in w32Apps)
                {
                    //WIP

                    CreateAppTileAsync(null, 1);


                }

                allAppsLoaded = true;
            }

            catch (Exception) { }

        }

        private async void LoadApps(int Start)
        {
            lvMain.Items.Clear();
            lvMain.IsEnabled = true;

            if (Start == 1) //All Apps
                            //UWP APPS
            {
                if (!allAppsLoaded)
                {
                    getAllApps();
                        if (lvMain.Items.Count == 0)
                        {
                            var noApps = new TextBlock();
                            noApps.Text = "Failure, can't get Apps from System";
                            lvMain.Items.Add(noApps);
                            lvMain.IsEnabled = false;
                        }
                    
                    foreach (Array app in lvMain.Items)
                    {
                        allApps.Add(app);
                    }
                }
                else if (allAppsLoaded)
                {
                    foreach (Button app in allApps)
                    {
                        lvMain.Items.Add(app);
                    }
                }
                lvMain.CanReorderItems = false;
                tbFrontPageLabel.Text = "All Apps";



                //var items = lvMain.Items.Cast<Button>().OrderBy(x => x.Content).ToList();
                //lvMain.Items.Clear();
                //lvMain.Items.Add(items);
                //lvMain.Items.RemoveAt(0);

            }

            if (Start == 0)
            {
                //SaveCfg();
                if (appPath.Count != 0)
                {

                    var packageManager = new PackageManager();

                    foreach (String appPin in appPath)
                    {
                        Package package = packageManager.FindPackageForUser("", appPin);
                        CreateAppTileAsync(package.GetAppListEntries().First(), 0);
                    }

                    //w32 Apps //allApps //WIP

                    foreach (String path in w32Apps)
                    {
                        //WIP
                        CreateAppTileAsync(null, 1);
                    }
                }

                else
                {
                    var noApps = new TextBlock();
                    lvMain.IsEnabled = false;
                    noApps.Text = "No pinned apps...";
                    noApps.FontSize = 18;
                    lvMain.Items.Add(noApps);
                }
                lvMain.CanReorderItems = true;
                tbFrontPageLabel.Text = "Pinned Apps";
                tbAppCount.Text = String.Empty;



                //XmlDocument CfgFile = new XmlDocument();

                //try {
                //    CfgFile.Load("CfgFile.xml"); }
                //catch {
                //    CreateCfg();
                //}
                //lvMain.CanReorderItems = true;


            }
        }


        private void LoadCfg()
        {

            var doc = XDocument.Load("PinnedApps.xml");

            var items = from i in doc.Root.Element("app").Elements("info")
                        select (string)i.Element("path");
            foreach (String item in items)
            {
                appPath.Add(item);
            }
        }

        private void SaveCfg()
        {
            XDocument doc = new XDocument();
            foreach (String item in appPath)
            {
                XElement Apps = new XElement("app",
                                                           new XElement("info",
                                                               new XElement("path", item)));
                doc.Add(Apps);
            }




            //doc.WriteStartDocument();
            //foreach (String item in appPath)
            //{
            //    doc.WriteStartElement("app");
            //    doc.WriteStartAttribute("info");
            //    doc.WriteAttributeString("path", item);
            //    doc.WriteEndAttribute();
            //    doc.WriteEndElement();

            //}
            //doc.WriteEndDocument();
        }

        private void cmdCfg_Click(object sender, RoutedEventArgs e)
        {

        }

        private int SwitchPage(int CurrentPage)
        {
            if (CurrentPage == 0)
            {
                return 1;
            }
            else { return 0; }
        }

        private void LoadStatusBar()
        {
            try { spStatus.Children.Clear(); }
            catch { }
            //Battery
            if (batterystatus != 0)
            {
                var cmdBattery = new Button();
                if (batterystatus == 1) { battery.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/BootstrapIcons-BatteryCharging.png")); }
                if (batterystatus == 2) { battery.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/BootstrapIcons-Battery.png")); }
                if (batterystatus == 3) { battery.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/BootstrapIcons-BatteryHalf.png")); }
                if (batterystatus == 4) { battery.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/BootstrapIcons-BatteryFull.png")); }
                ScaleTransform batteryScale = new ScaleTransform();
                batteryScale.ScaleX = 1.2;
                batteryScale.ScaleY = 1.2;
                battery.Transform = batteryScale;
                battery.Stretch = Stretch.Uniform;
                cmdBattery.Background = battery;
                Button tbBatteryPercent = new Button();
                cmdBattery.Tag = batterypercentage.ToString() + "%";
                cmdBattery.Click += CmdBattery_Click;
                spStatus.Children.Add(cmdBattery);
            }



            //Volume
            //Networking
            //User Control
            //var cmdUser = new Button();
            //var usrPicSrc = new PersonPicture();
            //var usrPic = new ImageBrush();
            //usrPicSrc = null;
            ////usrPic.ImageSource = usrPicSrc.BadgeImageSource;
            //usrPic.Stretch = Stretch.Uniform;
            ////cmdUser.Background = usrPic;
            //cmdUser.Content = KnownUserProperties.DisplayName;
            //gvStatus.Items.Add(cmdUser);

        }

        private void CmdBattery_Click(object sender, RoutedEventArgs e)
        {
            Button cmdBattery = sender as Button;
            cmdBattery.Background = battery;
            MenuFlyoutItem cmdfoBattery = new MenuFlyoutItem();
            cmdfoBattery.Text = batterypercentage.ToString() + " % remaining";
            foBattery.Items.Clear();
            foBattery.Items.Add(cmdfoBattery);
            foBattery.ShowAt(cmdBattery);
        }

        private Boolean checkStatusAvail(String status)
        {
            return true;
        }

        private async void AppClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            //AppLaunchAsync(clickedButton.Content.ToString());
            //{
            //    try
            //    {
            //        //Uri appUri = new Uri(clickedButton.Tag.ToString());//(clickedButton.Tag.ToString());
            //        //Console.Write(appUri.ToString());
            //        //Launcher.LaunchUriAsync(appUri);
            //        using (Process launchApp = new Process())
            //        {
            //            launchApp.StartInfo.UseShellExecute = false;
            //            launchApp.StartInfo.FileName = "start "+clickedButton.Tag.ToString();
            //            launchApp.StartInfo.CreateNoWindow = false;
            //            launchApp.Start();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);

            //    }
            //}
            PackageManager packageManager = new PackageManager();
            Package package = packageManager.FindPackageForUser("", clickedButton.Tag.ToString());
            var entrylist = await package.GetAppListEntriesAsync();
            if (entrylist != null)
            {
                try
                {
                    var entry = entrylist.First();
                    await (entry as AppListEntry).LaunchAsync();
                }
                catch (Exception) { };


            }
        }

        private void AppRightClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MenuFlyout appright = new MenuFlyout();
            MenuFlyoutItem cmdpin = new MenuFlyoutItem();
            MenuFlyoutItem cmdcopypath = new MenuFlyoutItem();
            cmdpin.Tag = clickedButton.Tag;
            cmdcopypath.Tag = clickedButton.Tag;
            cmdcopypath.Text = "Copy Path";
            if (ActivePage == 0) { cmdpin.Text = "Unpin App"; }
            if (ActivePage == 1) { cmdpin.Text = "Pin App"; }
            cmdpin.Click += cmdpinClick;
            cmdcopypath.Click += cmdcopyPathClick;
            appright.Items.Add(cmdpin);
            appright.Items.Add(cmdcopypath);
            appright.ShowAt(clickedButton);
        }

        private void cmdpinClick(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem cmdpin = sender as MenuFlyoutItem;
            if (ActivePage == 0)
            {
                appPath.Remove(cmdpin.Tag);
                LoadApps(0);
            }
            if (ActivePage == 1)
            {
                if (!appPath.Contains(cmdpin.Tag))
                {
                    appPath.Add(cmdpin.Tag);
                }
            }
        }

        private void cmdcopyPathClick(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem cmdcopypath = sender as MenuFlyoutItem;
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(cmdcopypath.Tag.ToString());
            Clipboard.SetContent(dataPackage);
        }

        private void w32AppClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

        }

        private void w32AppRightClick(object sender, RoutedEventArgs e)
        {

        }

        private void tbTimeDate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (calFlyout.Visibility == Visibility.Collapsed)
            {
                calFlyout.Visibility = Visibility.Visible;
                tbTimeDate.FontStyle = Windows.UI.Text.FontStyle.Italic;
            }
        }

        private void tbTimeDate_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void tbTimeDate_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            tbTimeDate.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
        }

        private void tbTimeDate_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            tbTimeDate.TextDecorations = Windows.UI.Text.TextDecorations.None;
        }

        private void MainGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (calFlyout.Visibility.Equals(Visibility.Visible) && tbTimeDate.TextDecorations.Equals(Windows.UI.Text.TextDecorations.None))
            {
                calFlyout.Visibility = Visibility.Collapsed;
                tbTimeDate.FontStyle = Windows.UI.Text.FontStyle.Normal;
            }

        }

        private void changeBackground(String Path)
        {

            if (Path != null)
            {
                var bg = new ImageBrush();
                bg.ImageSource = new BitmapImage(new Uri(Path));
                bg.Stretch = Stretch.UniformToFill;
                bg.Opacity = 0.4;
                MainGrid.Background = bg;
            }
            else
            {
                MainGrid.Background = DefaultMainBg;
            }
        }

        private void AggregateBattery_ReportUpdated(Battery sender, object args)
        {
            var batteryReport = Battery.AggregateBattery.GetReport();

            var percentage = (batteryReport.RemainingCapacityInMilliwattHours.Value / (double)batteryReport.FullChargeCapacityInMilliwattHours.Value);

            if ((percentage <= 0.25) && batteryReport.Status == Windows.System.Power.BatteryStatus.Discharging)
            {
                batterystatus = 2;
            }

            if ((percentage < 0.75 && percentage > 0.25) && batteryReport.Status == Windows.System.Power.BatteryStatus.Discharging)
            {
                batterystatus = 3;
            }

            if ((percentage >= 0.75) && batteryReport.Status == Windows.System.Power.BatteryStatus.Discharging)
            {
                batterystatus = 4;
            }

            if (batteryReport.Status == Windows.System.Power.BatteryStatus.Charging || batteryReport.Status != Windows.System.Power.BatteryStatus.Discharging && batteryReport.Status != Windows.System.Power.BatteryStatus.NotPresent)
            {
                batterystatus = 1;
            }

            if (batteryReport.Status == Windows.System.Power.BatteryStatus.NotPresent)
            {
                batterystatus = 0;
            }
            batterypercentage = (int)(percentage * 100);
            statuschange = true;
        }

        private void RequestAggregateBatteryReport()
        {
            // Create aggregate battery object
            var aggBattery = Battery.AggregateBattery;

            // Get report
            var report = aggBattery.GetReport();
            AggregateBattery_ReportUpdated(aggBattery, report);
        }

        private async void cmdMainStart_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            await cdConfig.ShowAsync();

        }

        private void createCdConfig()
        {
            cdConfig.Title = "Settings";
            cdConfig.PrimaryButtonText = "Save and apply";
            cdConfig.SecondaryButtonText = "Cancel";
            cdConfig.SecondaryButtonCommand = cdConfig.CloseButtonCommand;
            var cfgText = new TextBlock();
            cfgText.Text = "Choose Settings for the Shell";
            var sp = new StackPanel();



            //Śettings Objects

            useBackground.Content = "Show Background Image?";
            if (bg == true) { useBackground.IsChecked = true; } else { useBackground.IsChecked = false; }
            GridView ConfigContent = new GridView();
            ConfigContent.Items.Add(useBackground);

            //Add Content
            sp.Children.Add(cfgText);
            sp.Children.Add(ConfigContent);
            sp.Spacing = 10;

            cdConfig.Content = sp;
            cdConfig.PrimaryButtonClick += cdConfigPrimaryButton_Click;
        }

        private void cdConfigPrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs e)
        {
            if (useBackground.IsChecked.Equals(true)) { bg = true; }
            if (useBackground.IsChecked.Equals(false)) { bg = false; }
            if (bg) { changeBackground("ms-appx:///Assets/img0.jpg"); }
            if (!bg) { changeBackground(null); }
        }

        private void setTip()
        {
            if (tip)
            {
                spTip.Children.Clear();
                ArrayList tipList = new ArrayList();

                var cfgTip = "Right click the windows button, to open Settings";
                var wipsaveTip = "Saving the app layout is not yet supported";
                var w32Tip = "As of now, only UWP Apps are supported";
                var manAppTip = "You can manually add Apps to your Pinned Apps page, \nby clicking the plus button on the right \nand entering the FullPackageName in the text field";
                tipList.Add(cfgTip);
                tipList.Add(wipsaveTip);
                tipList.Add(w32Tip);
                tipList.Add(manAppTip);
                var rdnTip = new Random();
                var thisTip = new TextBlock();
                var ranTip = (rdnTip.Next(1, tipList.Count + 1) - 1);
                thisTip.Text = "Tip:\n" + tipList[ranTip].ToString();
                spTip.Children.Add(thisTip);
            }
        }

        private async void cmdManualAddApp_Click(object sender, RoutedEventArgs e)
        {
            if (txtManualAddApp.Visibility.Equals(Visibility.Collapsed))
            {
                txtManualAddApp.Visibility = Visibility.Visible;
                cmdManualAddApp.Content = ">";
            }

            else if (txtManualAddApp.Visibility.Equals(Visibility.Visible))
            {
                var packageManager = new PackageManager();
                if (!txtManualAddApp.Text.Equals(String.Empty))
                {
                    try
                    {
                        Package package = packageManager.FindPackageForUser("", txtManualAddApp.Text);
                        if (package != null && !appPath.Contains(txtManualAddApp.Text))
                        {
                            appPath.Add(txtManualAddApp.Text);
                            if (ActivePage == 0) { LoadApps(0); }
                        }
                        else if (package != null && appPath.Contains(txtManualAddApp.Text))
                        {
                            var errPath = new ContentDialog();
                            errPath.Title = "Package already pinned";
                            errPath.Content = "The specified package is already pinned";
                            errPath.IsSecondaryButtonEnabled = false;
                            errPath.IsPrimaryButtonEnabled = true;
                            errPath.PrimaryButtonText = "OK";
                            await errPath.ShowAsync();
                        }
                        else //(package == null)
                        {
                            var errPath = new ContentDialog();
                            errPath.Title = "Cannot find package";
                            errPath.Content = "The specified package could not be resolved";
                            errPath.IsSecondaryButtonEnabled = false;
                            errPath.IsPrimaryButtonEnabled = true;
                            errPath.PrimaryButtonText = "OK";
                            await errPath.ShowAsync();
                        }

                    }
                    catch { }
                }
                txtManualAddApp.Visibility = Visibility.Collapsed;
                txtManualAddApp.Text = String.Empty;
                cmdManualAddApp.Content = "+";
            }

        }

        private void txtManualAddApp_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampUtc(location);
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970 + 7200);
        }


    }

}

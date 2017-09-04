using DirectShowLib;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using WPFMediaKit.DirectShow.Controls;
using System.Drawing.Imaging;

namespace photomaton.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static string ImgFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Photomaton");

        public MainWindowViewModel()
        {
            Title = "Photomaton";
            IsFullScreen = true;

            CaptureCommand = new DelegateCommand(Capture, CanCapture);
            ToggleFullScreenCommand = new DelegateCommand(ToggleFullScreen, CanToggleFullScreen);
            ToggleStretchCommand = new DelegateCommand<StretchModes>(ToggleStretch, CanToggleStretch);
            OpenStretchOptionsCommand = new DelegateCommand(OpenStretchOptions, CanOpenStretchOptions);
            OpenSettingsCommand = new DelegateCommand(OpenSettings, CanOpenSettings);
            ChangeDeviceCommand = new DelegateCommand<DsDevice>(ChangeDevice, CanChangeDevice);

            if (!Directory.Exists(ImgFolder))
                Directory.CreateDirectory(ImgFolder);

            RefreshDevicesList();
            CaptureDevice = CaptureDevices.FirstOrDefault();
        }

        #region fields & properties

        private AutoResetEvent _capturingLock = new AutoResetEvent(false);

        #region settings and options

        private string _title = null;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private List<DsDevice> _captureDevices = null;

        public List<DsDevice> CaptureDevices
        {
            get { return _captureDevices; }
            set { SetProperty(ref _captureDevices, value); }
        }

        private DsDevice _captureDevice = null;

        public DsDevice CaptureDevice
        {
            get { return _captureDevice; }
            set { SetProperty(ref _captureDevice, value); }
        }

        public List<StretchModes> AvailableStretchModes { get; } = new List<StretchModes>
        {
            StretchModes.None,
            StretchModes.Fill,
            StretchModes.Uniform,
            StretchModes.UniformToFill
        };

        private Stretch _stretch = Stretch.UniformToFill;

        public Stretch Stretch
        {
            get { return _stretch; }
            set { SetProperty(ref _stretch, value); }
        }

        private bool _isFullScreen;

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set { SetProperty(ref _isFullScreen, value); }
        }

        private bool _areSettingsOpen;

        public bool AreSettingsOpen
        {
            get { return _areSettingsOpen; }
            set { SetProperty(ref _areSettingsOpen, value); }
        }

        private bool _isStretchMenuOpen;

        public bool IsStretchMenuOpen
        {
            get { return _isStretchMenuOpen; }
            set { SetProperty(ref _isStretchMenuOpen, value); }
        }

        #endregion

        private byte[] _cameraShot;

        public byte[] CameraShot
        {
            get { return _cameraShot; }
            set
            {
                SetProperty(ref _cameraShot, value);
                _capturingLock.Set();
            }
        }

        private bool _isWaitingCameraShot;

        public bool IsWaitingCameraShot
        {
            get { return _isWaitingCameraShot; }
            set { SetProperty(ref _isWaitingCameraShot, value); }
        }

        private bool _isRunningCapture;

        public bool IsRunningCapture
        {
            get { return _isRunningCapture; }
            set { SetProperty(ref _isRunningCapture, value); }
        }

        private string _countdown = null;

        public string Countdown
        {
            get { return _countdown; }
            set { SetProperty(ref _countdown, value); }
        }

        private int _delayBeforeShot = 0;

        public int DelayBeforeShot
        {
            get { return _delayBeforeShot; }
            set { SetProperty(ref _delayBeforeShot, value); }
        }

        #region final image merge

        private string _lastMerge = "";

        public string LastMerge
        {
            get { return _lastMerge; }
            set { SetProperty(ref _lastMerge, value); }
        }

        private bool _isShowingMerge;

        public bool IsShowingMerge
        {
            get { return _isShowingMerge; }
            set { SetProperty(ref _isShowingMerge, value); }
        }

        #endregion

        #endregion fields & properties

        #region methods

        public void RefreshDevicesList()
        {
            CaptureDevices = MultimediaUtil.VideoInputDevices.ToList();
        }

        public void Capture()
        {
            Task.Run(() => DoCapture());
        }

        private void DoCapture()
        {
            IsRunningCapture = true;
            CommandManager.InvalidateRequerySuggested();

            for(var i = 3; i > 0; --i)
            {
                Countdown = i.ToString();
                Thread.Sleep(1000);
            }

            Countdown = "Smile !";
            Thread.Sleep(1000);
            Countdown = null;

            var folder = DateTime.Now.ToString(@"yyyy-MM-dd HH\hmm ssfff", CultureInfo.InvariantCulture);
            folder = Path.Combine(ImgFolder, folder);
            Directory.CreateDirectory(folder);

            List<byte[]> images = new List<byte[]>();
            var nbShot = 4;
            for (var i = 0; i < nbShot; ++i)
            {
                Thread.Sleep(500);

                IsWaitingCameraShot = true;

                _capturingLock.WaitOne();

                Thread.Sleep(200);
                IsWaitingCameraShot = false;

                using (var fs = new FileStream(Path.Combine(folder, string.Format(@"{0}.jpg", i + 1)), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(CameraShot, 0, CameraShot.Length);
                }

                images.Add(CameraShot);

                if (i < nbShot - 1) {
                    int msToSleep = 1500 / 100;
                    for (var delayPercent = 100; delayPercent >= 0; --delayPercent)
                    {
                        DelayBeforeShot = delayPercent;
                        Thread.Sleep(msToSleep);
                    }
                }
                else
                    Thread.Sleep(500);
            }

            MergeImages(images, folder);

            IsShowingMerge = true;
            Thread.Sleep(6000);
            IsShowingMerge = false;

            IsRunningCapture = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void MergeImages(List<byte[]> shots, string folderName)
        {
            List<Image> images = new List<Image>();
            foreach (var shot in shots)
            {
                using(var ms = new MemoryStream(shot))
                    images.Add(Image.FromStream(ms));
            }

            var imgWidth = images.First().Width;
            var imgHeight = images.First().Height;
            Bitmap bitmap1 = new Bitmap(imgWidth + 80, 40 + (imgHeight + 40) * images.Count);
            using (Graphics g = Graphics.FromImage(bitmap1))
            {
                for (var i = 0; i < images.Count; ++i)
                {
                    g.DrawImage(images[i], 40, 40 + i * (imgHeight + 40));
                }
                bitmap1.Save(Path.Combine(folderName, "merge_col.jpg"), ImageFormat.Jpeg);
            }

            var nbCol = (int)Math.Ceiling(Math.Sqrt(images.Count));
            var nbRow = nbCol;
            Bitmap bitmap2 = new Bitmap(40 + (imgWidth + 40) * nbCol , 40 + (imgHeight + 40) * nbRow);
            using (Graphics g = Graphics.FromImage(bitmap2))
            {
                for (var i = 0; i < images.Count; ++i)
                {
                    var col = i % nbCol;
                    var row = i / nbCol;
                    g.DrawImage(images[i], 40 + (imgWidth + 40) * col, 40 + row * (imgHeight + 40));
                }
                var path = Path.Combine(folderName, "merge_square.jpg");
                bitmap2.Save(path, ImageFormat.Jpeg);
                LastMerge = path;
            }
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        public void ToggleStretch(StretchModes stretchMode)
        {
            Stretch = stretchMode.Stretch;
        }

        public void OpenStretchOptions()
        {
            IsStretchMenuOpen = !IsStretchMenuOpen;
        }

        public void OpenSettings()
        {
            RefreshDevicesList();
            AreSettingsOpen = !AreSettingsOpen;
        }

        public void ChangeDevice(DsDevice deviceName)
        {
            CaptureDevice = deviceName;
        }

        #endregion methods

        #region commands

        public DelegateCommand CaptureCommand { get; private set; }

        public bool CanCapture()
        {
            return !IsRunningCapture;
        }

        public DelegateCommand ToggleFullScreenCommand { get; private set; }

        public bool CanToggleFullScreen()
        {
            return !IsRunningCapture;
        }

        public DelegateCommand<StretchModes> ToggleStretchCommand { get; private set; }

        public bool CanToggleStretch(StretchModes stretchMode)
        {
            return !IsRunningCapture;
        }

        public DelegateCommand OpenStretchOptionsCommand { get; private set; }

        public bool CanOpenStretchOptions()
        {
            return !IsRunningCapture;
        }

        public DelegateCommand OpenSettingsCommand { get; private set; }

        public bool CanOpenSettings()
        {
            return !IsRunningCapture;
        }

        public DelegateCommand<DsDevice> ChangeDeviceCommand { get; private set; }

        public bool CanChangeDevice(DsDevice deviceName)
        {
            return !IsRunningCapture;
        }

        #endregion commands
    }

    public class StretchModes
    {
        public Stretch Stretch { get; set; }

        public string Name { get; set; }

        public StretchModes(string name, Stretch stretch)
        {
            Name = name;
            Stretch = stretch;
        }

        public static StretchModes None = new StretchModes("None", Stretch.None);
        public static StretchModes Fill = new StretchModes("Fill", Stretch.Fill);
        public static StretchModes Uniform = new StretchModes("Uniform", Stretch.Uniform);
        public static StretchModes UniformToFill = new StretchModes("UniformToFill", Stretch.UniformToFill);
    }
}
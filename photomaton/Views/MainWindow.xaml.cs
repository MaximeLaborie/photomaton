using photomaton.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFMediaKit.DirectShow.MediaPlayers;

namespace photomaton.Views
{
    public partial class MainWindow : Window
    {
        public bool IsSampleRequired { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            videoCapElement.NewVideoSample += OnNewVideoSample;
        }

        private void OnNewVideoSample(object sender, VideoSampleArgs e)
        {
            if (IsSampleRequired)
            {
                var bmp = e.VideoFrame;

                Task.Run(() => {
                    var vm = DataContext as MainWindowViewModel;
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        videoCapElement.Play();
                        vm.CameraShot = ms.ToArray();
                    }
                });

            }
        }

        public void SetCameraShotToViewModel()
        {
            IsSampleRequired = true;
            //var vm = DataContext as MainWindowViewModel;
            //vm.CameraShot = GetCameraShot();
        }

        //private byte[] GetCameraShot()
        //{
        //    videoCapElement.Pause();
        //    RenderTargetBitmap bmp = new RenderTargetBitmap((int)videoCapElement.ActualWidth, (int)videoCapElement.ActualHeight, 96, 96, PixelFormats.Default);
        //    bmp.Render(videoCapElement);
        //    BitmapEncoder encoder = new JpegBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(bmp));
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        encoder.Save(ms);
        //        videoCapElement.Play();
        //        return ms.ToArray();
        //    }
        //}
    }
}
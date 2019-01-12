using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using RabbitMQ.Client;

namespace VideoRecorder
{
    internal class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Private fields

        private FilterInfo _currentDevice;

        private BitmapImage _image;
        private bool _isWebcamSource;

        private IVideoSource _videoSource;
        private VideoFileWriter _writer;
        private bool _recording;
        private DateTime? _firstFrameTime;
        private string _fullPath;
        private int _lastFile;
        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            GetVideoDevices();
            IsWebcamSource = true;
            //StartSourceCommand = new RelayCommand(StartCamera);
            //StopSourceCommand = new RelayCommand(StopCamera);
            StartRecordingCommand = new RelayCommand(StartRecording);
            StopRecordingCommand = new RelayCommand(StopRecording);
            StartCamera();
        }

        #endregion

        #region Properties

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public BitmapImage Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        

        public bool IsWebcamSource
        {
            get { return _isWebcamSource; }
            set { Set(ref _isWebcamSource, value); }
        }

       

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { Set(ref _currentDevice, value); }
        }

        public ICommand StartRecordingCommand { get; private set; }

        public ICommand StopRecordingCommand { get; private set; }

        public ICommand StartSourceCommand { get; private set; }

        public ICommand StopSourceCommand { get; private set; }

        public ICommand SaveSnapshotCommand { get; private set; }

        #endregion

        private void GetVideoDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                VideoDevices.Add(device);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No camera found");
            }
        }

        private void StartCamera()
        {
            if (IsWebcamSource)
            {
                if (CurrentDevice != null)
                {
                    _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                    _videoSource.NewFrame += video_NewFrame;
                    _videoSource.Start();
                }
                else
                {
                    MessageBox.Show("Current device can't be null");
                }
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime != null)
                        {
                            _writer.WriteVideoFrame(bitmap);
                        }
                        else
                        {
                            _writer.WriteVideoFrame(bitmap);
                        }
                    }
                }
                using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Image = null;
        }

        private void StopRecording()
        {
            if (_recording)
            {
                _recording = false;
                _writer.Close();
                _writer.Dispose();
                AskForRecognition(_fullPath);
            }
            else
            {
                MessageBox.Show("No recording has started");
            }
        }

        private void StartRecording()
        {
            /*
            var dialog = new SaveFileDialog();
            dialog.FileName = "Video1";
            dialog.DefaultExt = ".mp4";
            dialog.AddExtension = true;
            var dialogresult = dialog.ShowDialog();
            if (dialogresult != true)
            {
                return;
            }
            */
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Yo\Desktop\Studies\Sistemas Distribuidos\Proyecto Final\WebCamera\VideoRecorder\videos");
            FileInfo[] Files = d.GetFiles("*.mp4");
            _lastFile = Files.Length + 1;
            _fullPath = @"C:\Users\Yo\Desktop\Studies\Sistemas Distribuidos\Proyecto Final\WebCamera\VideoRecorder\videos\"+ _lastFile + ".mp4";
            _firstFrameTime = null;
            _writer = new VideoFileWriter();
            _writer.Open(_fullPath, (int)Math.Round(Image.Width, 0), (int)Math.Round(Image.Height, 0));
            _recording = true;
        }



        private void SaveSnapshot()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Snapshot1";
            dialog.DefaultExt = ".png";
            var dialogresult = dialog.ShowDialog();
            if (dialogresult != true)
            {
                return;
            }
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Image));
            using (var filestream = new FileStream(dialog.FileName, FileMode.Create))
            {
                encoder.Save(filestream);
            }
        }

        private void AskForRecognition(string path)
        {
            var rpcClient = new RpcClient();
            Console.WriteLine(" [x] Requesting video processing of " + path);
            var response = rpcClient.Call(path);
            Console.WriteLine(response);
            rpcClient.Close();
            if (response == "No face")
                MessageBox.Show("No face identified");
            else
                Process.Start(response);
        }


        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
            }
            _writer?.Dispose();
        }
    }
}

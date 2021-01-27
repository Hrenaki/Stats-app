using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using System.Management;
using OpenHardwareMonitor.Hardware;

namespace StatsApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private Computer myComputer;
        private Point prevMousePos;
        private bool IsMovementEnabled { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            //Configuring timer
            timer = new DispatcherTimer();
            timer.Tick += Timer_Elapsed;
            timer.Interval = new TimeSpan(0, 0, 1);

            //Configuring window
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 5;
            Left = 5;
            IsMovementEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();

            cpuTextBlock.Text = "";
            myComputer = new Computer();
            myComputer.Open();

            myComputer.CPUEnabled = true;            
            myComputer.GPUEnabled = true;
            myComputer.RAMEnabled = true;
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            cpuTextBlock.Text = null;
            gpuTextBlock.Text = null;
            ramTextBlock.Text = null;

            foreach (var hardwareItem in myComputer.Hardware)
            {
                hardwareItem.Update();
                foreach (var subHardware in hardwareItem.SubHardware)
                    subHardware.Update();
                switch (hardwareItem.HardwareType)
                {
                    case HardwareType.CPU:
                        foreach (var sensor in hardwareItem.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                                cpuTextBlock.Text += sensor.Name.Remove(0, 3) + " " + 
                                    Convert.ToInt32(sensor.Value) + "%\n";
                            if (sensor.SensorType == SensorType.Temperature)
                                cpuTextBlock.Text += sensor.Name.Remove(0, 3) + " " + 
                                    Convert.ToInt32(sensor.Value) + "°С\n";
                        }
                        break;
                    case HardwareType.GpuNvidia:
                        int loadPercentage = 0;
                        foreach (var sensor in hardwareItem.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                            {
                                if (sensor.Name == "GPU Core")
                                    loadPercentage = Convert.ToInt32(sensor.Value);
                                gpuTextBlock.Text += sensor.Name.Remove(0, 3) + " " + 
                                    Convert.ToInt32(sensor.Value) + "%\n";
                            }

                            if (sensor.SensorType == SensorType.Temperature)
                                gpuTextBlock.Text += sensor.Name.Remove(0, 3) + " " + 
                                    Convert.ToInt32(sensor.Value) + "°С\n";
                        }
                        break;
                    case HardwareType.GpuAti:
                            goto case HardwareType.GpuNvidia;
                    case HardwareType.RAM:
                        foreach (var sensor in hardwareItem.Sensors)
                            if (sensor.SensorType == SensorType.Load)
                                ramTextBlock.Text += " " + 
                                    Convert.ToInt32(sensor.Value) + "%\n";
                        break;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            myComputer.Close();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (lockButton.IsMouseDirectlyOver)
                lockButton_Click(this, e);
            else
            {
                if (IsMouseOver && IsMovementEnabled && e.MouseDevice.LeftButton == MouseButtonState.Pressed)
                {
                    Point position = e.MouseDevice.GetPosition(this);
                    Point screenPoint = PointToScreen(position);
                    Top += screenPoint.Y - prevMousePos.Y;
                    Left += screenPoint.X - prevMousePos.X;

                    prevMousePos = screenPoint;
                }
                else
                {
                    if (!IsMouseOver)
                        Mouse.Capture(null);
                }
            }
        }

        private void lockButton_Click(object sender, RoutedEventArgs e)
        {
            IsMovementEnabled = !IsMovementEnabled;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevMousePos = PointToScreen(e.GetPosition(this));
        }
    }
}

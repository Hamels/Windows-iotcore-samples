// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 5;
        private GpioPin pin;
        private GpioPinValue pinValue;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private IAsyncAction workItemThread;

        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
            InitGPIO();
            if (pin != null)
            {
                //timer.Start();
            }        
        }



        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }


        public void PWM_R(int pinNumber)
        {
            var stopwatch = Stopwatch.StartNew();
            

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                    // ManualResetEvent mre = new ManualResetEvent(false);
                    // mre.WaitOne(10);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 100) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 1)
                     {
                         pin.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks) break;
                         }
                         pin.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }

        public void PWM_L(int pinNumber)
        {
            var stopwatch = Stopwatch.StartNew();

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                     ManualResetEvent mre = new ManualResetEvent(false);
                     mre.WaitOne(1500);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 300)
                     {
                         pin.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = starttick - (ulong)(stopwatch.ElapsedTicks);
                             if (delta > pulseTicks) break;
                         }
                         pin.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }



        private void Timer_Tick(object sender, object e)
        {
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
                LED.Fill = redBrush;
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PWM_L(LED_PIN);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                PWM_R(LED_PIN);
                Task.Delay(6).Wait();
            }
           
        }
    }
}

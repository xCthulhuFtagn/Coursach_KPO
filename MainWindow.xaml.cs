using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Globalization;



namespace Swing
{

    public partial class Clock : Window
    {
        public IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-En");
        bool paused = true;
        bool start = true;
        private double Mass = 1;
        private double NominalLength = 40;
        private double Length = 1;
        private double DampCoef = 0.5;
        private double Angle = 45, AngVel = 0;
        private double step = 0.1;
        private double HourAngle = 0, MinuteAngle = 0, SecondAngle = 0;
        private double dH, dM, dS, TimeCoef;
        Polyline pl = new Polyline();
        const double g = 9.81;
        public Clock()
        {
            InitializeComponent();
        }
        private void SettingUp(ref TextBox input, ref double param)
        {
            var tmp = input.Text.Split('.', ',');
            param = Math.Abs(Double.Parse(String.Join('.', tmp), provider));
            input.Text = param.ToString();
        }
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (start)
            {
                HourAngle = 0; MinuteAngle = 0; SecondAngle = 0;
                //impossible to enter mass/length/dampcoef/timecoef < 0
                SettingUp(ref tbMass, ref Mass);
                SettingUp(ref tbLength, ref Length);
                SettingUp(ref tbDamping, ref DampCoef);
                SettingUp(ref tbTimeCoef, ref TimeCoef);
                TimeCoef = Math.Abs(Double.Parse(tbTimeCoef.Text));
                tbTimeCoef.Text = TimeCoef.ToString();
                string[] t = tbTime.Text.Split(":");
                HourAngle = Double.Parse(t[0])*GetRad(30);
                MinuteAngle = Double.Parse(t[1])*GetRad(6);
                SecondAngle = Double.Parse(t[2])*GetRad(6);

                Angle = GetRad(Double.Parse(tbAngle.Text));
                dH = GetRad(360 / Math.Pow(60, 3)) * TimeCoef;
                dM = GetRad(360 / Math.Pow(60, 2)) * TimeCoef;
                dS = GetRad(360 / 60) * TimeCoef; 
                AngVel = 0;
                tbDisplay.Text = "Moving...";
                pl = new Polyline();
                pl.Stroke = Brushes.Red;
                start = false;
            }
            CompositionTarget.Rendering -= StartAnimation;
            if (paused)
            {
                PauseBtn.Content = "Pause";
                CompositionTarget.Rendering += StartAnimation;
            }
            else
            {
               PauseBtn.Content = "Resume";
            }
            paused = !paused;
        }

        private void StartAnimation(object sender, EventArgs e)
        {
            //get trajectory's new angle
            GetTrack();
            // Display moving pendulum on screen:
            Point pt = new Point(
            PendulumLine.X1 + NominalLength * Math.Sin(Angle),
            PendulumLine.Y1 + NominalLength * Math.Cos(Angle));
            ball.Center = pt;
            PendulumLine.X2 = pt.X;
            PendulumLine.Y2 = pt.Y;
            // Display moving Hands of hours
            HourAngle += dH; MinuteAngle += dM; SecondAngle += dS;
            HourHand.X2 = 140 + 20 * Math.Sin(HourAngle); HourHand.Y2 = 42 - 20 * Math.Cos(HourAngle);
            MinuteHand.X2 = 140 + 20 * Math.Sin(MinuteAngle); MinuteHand.Y2 = 42 - 20 * Math.Cos(MinuteAngle);
            SecondHand.X2 = 140 + 20 * Math.Sin(SecondAngle); SecondHand.Y2 = 42 - 20 * Math.Cos(SecondAngle);
            if (Math.Abs(Angle) <= 0.01 && Math.Abs(AngVel) <= 0.01)
            {
                tbDisplay.Text = "Stopped";
                CompositionTarget.Rendering -= StartAnimation;
                start = true;
            }
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ClockInitialize();
            tbDisplay.Text = "Stopped";
            CompositionTarget.Rendering -= StartAnimation;
            start = true;
            paused = true;
        }

        private void ClockInitialize()
        {
            tbMass.Text = "1";
            tbLength.Text = "40";
            tbDamping.Text = "0.1";
            tbAngle.Text = "45";
            tbTimeCoef.Text = "1";
            tbTime.Text = "0:0:0";
            PendulumLine.X2 = 140;
            PendulumLine.Y2 = 100;
            ball.Center = new Point(140, 100);
            HourHand.X2 = 140; HourHand.Y2 = 22; HourAngle = 0;
            MinuteHand.X2 = 140; MinuteHand.Y2 = 22; MinuteAngle = 0;
            SecondHand.X2 = 140; SecondHand.Y2 = 22; SecondAngle = 0;
            PauseBtn.Content = "Start";
        }
        private double sec_der (double phi, double v) { 
            return -(DampCoef*v/(Mass*Length) + g*Math.Sin(phi)/Length);
        }
        static double GetRad(double phi) { return phi * Math.PI / 180; }
        private void GetTrack() //using Runge-Kutt for second grade dif eq
        {
            double K_1a, K_2a, K_3a, K_4a, K_1z, K_2z, K_3z, K_4z;
            K_1a = AngVel; K_1z = sec_der(Angle, AngVel);
            K_2a = AngVel + step * K_1z / 2; K_2z = sec_der(Angle + step * K_1a / 2, AngVel + step * K_1z / 2);
            K_3a = AngVel + step * K_2z / 2; K_3z = sec_der(Angle + step * K_2a / 2, AngVel + step * K_2z / 2);
            K_4a = AngVel + step * K_3z; K_4z = sec_der(Angle + step * K_3a, AngVel + step * K_3z);
            Angle += step * (K_1a + 2 * K_2a + 2 * K_3a + K_4a) / 6;
            AngVel += step * (K_1z + 2 * K_2z + 2 * K_3z + K_4z) / 6;
        }
    }
}

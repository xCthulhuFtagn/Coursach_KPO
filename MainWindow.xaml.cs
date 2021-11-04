using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Swing
{
    public partial class Clock : Window
    {
        private double Mass = 1;
        private double NominalLength = 1;
        private double Length = 1;
        private double DampCoef = 0.5;
        private double Angle = 45, AngVel = 0, AngAcc = 0;
        private double step = 0.1;
        private double HourAngle = 0, MinuteAngle = 0, SecondAngle = 0;
        private  double dH = GetRad(360 / Math.Pow(60, 3)), dM = GetRad(360 / Math.Pow(60, 2)), dS = GetRad(360 / 60);
        private Queue<double> track = new Queue<double>();
        Polyline pl = new Polyline();
        const double g = 9.81;
        public Clock()
        {
            InitializeComponent();
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            HourAngle = 0; MinuteAngle = 0; SecondAngle = 0;
            Mass = Double.Parse(tbMass.Text);
            Length = Double.Parse(tbLength.Text);
            NominalLength = 40;
            DampCoef = Double.Parse(tbDamping.Text);
            Angle = GetRad(Double.Parse(tbAngle.Text));
            tbDisplay.Text = "Moving...";
            GetTrack();
            pl = new Polyline();
            pl.Stroke = Brushes.Red;
            CompositionTarget.Rendering += StartAnimation;
        }

        private void StartAnimation(object sender, EventArgs e)
        {
            //get trajectory
            if(track.Count > 0) Angle = track.Dequeue();
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
            if (track.Count == 0)
            {
                tbDisplay.Text = "Stopped";
                CompositionTarget.Rendering -= StartAnimation;
            }
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            PendulumInitialize();
            tbDisplay.Text = "Stopped";
            CompositionTarget.Rendering -= StartAnimation;
        }

        private void PendulumInitialize()
        {
            tbMass.Text = "1";
            tbLength.Text = "40";
            tbDamping.Text = "0.1";
            tbAngle.Text = "45";
            PendulumLine.X2 = 140;
            PendulumLine.Y2 = 100;
            ball.Center = new Point(140, 100);
            HourHand.X2 = 140; HourHand.Y2 = 22; HourAngle = 0;
            MinuteHand.X2 = 140; MinuteHand.Y2 = 22; MinuteAngle = 0;
            SecondHand.X2 = 140; SecondHand.Y2 = 22; SecondAngle = 0;
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            PendulumLine.X2 = 140;
            PendulumLine.Y2 = 100;
            ball.Center = new Point(140, 100);
            tbDisplay.Text = "Stopped";
            CompositionTarget.Rendering -= StartAnimation;
            track.Clear();
        }
        private double sec_der (double phi, double v) { return -(DampCoef*v/(Mass*Length) + g*Math.Sin(phi)/Length); }
        static double GetRad(double phi) { return phi * Math.PI / 180; }
        private void GetTrack()
        {
            double K_1a, K_2a, K_3a, K_4a, K_1z, K_2z, K_3z, K_4z;
            for(double t = 0; Math.Abs(Angle) >= 0.01 || Math.Abs(AngVel) >= 0.01; t += step)
            {
                track.Enqueue(Angle);
                K_1a = AngVel; K_1z = sec_der(Angle, AngVel);
                K_2a = AngVel + step * K_1z / 2; K_2z = sec_der(Angle + step * K_1a / 2, AngVel + step * K_1z / 2);
                K_3a = AngVel + step * K_2z / 2; K_3z = sec_der(Angle + step * K_2a / 2, AngVel + step * K_2z / 2);
                K_4a = AngVel + step * K_3z; K_4z = sec_der(Angle + step * K_3a, AngVel + step * K_3z);
                Angle += step * (K_1a + 2 * K_2a + 2 * K_3a + K_4a) / 6;
                AngVel += step * (K_1z + 2 * K_2z + 2 * K_3z + K_4z) / 6;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EmergeFramework;
using System.Windows.Controls.Primitives;

namespace EmergeRuntime
{
    /// <summary>
    /// Interaction logic for RemCtlWnd.xaml
    /// </summary>
    public partial class RemCtlWnd : Window
    {
        private enum Direction
        {
            Halt,
            Forward,
            Reverse
        }
        private Direction m_Direction;

        private RobotSpecification m_specRobot;
        private CommLink m_CommLink;

        public RemCtlWnd(RobotSpecification specRobot, CommLink commLink)
        {
            InitializeComponent();

            m_specRobot = specRobot;
            m_CommLink = commLink;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (SpecControl cs in m_specRobot.ControlSpecs)
            {
                object ctl = grdLayoutRoot.FindName(cs.ControlName);
                if (ctl is Button)
                {
                    Button btn = (Button)ctl;
                    btn.IsEnabled = cs.IsEnabled;
                    if(!string.IsNullOrEmpty(cs.Text))
                        btn.Content = cs.Text;
                }
                else if (ctl is ToggleButton)
                {
                    ToggleButton btn = (ToggleButton)ctl;
                    btn.IsEnabled = cs.IsEnabled;
                    if (!string.IsNullOrEmpty(cs.Text))
                        btn.Content = cs.Text;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSlow_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("S1");
        }

        private void btnMedium_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("S2");
        }

        private void btnFast_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("S3");
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("FW");
            m_Direction = Direction.Forward;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("HL");
            m_Direction = Direction.Halt;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("BK");
            m_Direction = Direction.Reverse;
        }

        private void btnLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendCommand("LF");
        }

        private void btnLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_Direction == Direction.Forward)
                SendCommand("FW");
            else if (m_Direction == Direction.Reverse)
                SendCommand("BK");
            else
                SendCommand("HL");
        }

        private void btnRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendCommand("RT");
        }

        private void btnRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_Direction == Direction.Forward)
                SendCommand("FW");
            else if (m_Direction == Direction.Reverse)
                SendCommand("BK");
            else
                SendCommand("HL");
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                case Key.D1:
                    SendCommand("S1");
                    break;

                case Key.F2:
                case Key.D2:
                    SendCommand("S2");
                    break;

                case Key.F3:
                case Key.D3:
                    SendCommand("S3");
                    break;

                case Key.NumPad8:
                    SendCommand("FW");
                    m_Direction = Direction.Forward;
                    break;

                case Key.NumPad5:
                    SendCommand("HL");
                    m_Direction = Direction.Halt;
                    break;

                case Key.NumPad2:
                    SendCommand("BK");
                    m_Direction = Direction.Reverse;
                    break;

                case Key.NumPad4:
                    SendCommand("LF");
                    break;

                case Key.NumPad6:
                    SendCommand("RT");
                    break;

                case Key.R:
                    SendCommand("VU");
                    break;

                case Key.F:
                    SendCommand("VH");
                    break;

                case Key.V:
                    SendCommand("VD");
                    break;

                case Key.D:
                    SendCommand("VL");
                    break;

                case Key.G:
                    SendCommand("VR");
                    break;

                case Key.S:
                    SendCommand("VS");
                    break;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad4:
                case Key.NumPad6:
                    if (m_Direction == Direction.Forward)
                        SendCommand("FW");
                    else if (m_Direction == Direction.Reverse)
                        SendCommand("BK");
                    else
                        SendCommand("HL");
                    break;
            }
        }

        private void btnVidUp_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VU");
        }

        private void btnVidLeft_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VL");
        }

        private void btnVidCenter_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VH");
        }

        private void btnVidRight_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VR");
        }

        private void btnVidDown_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VD");
        }

        private void btnVidScan_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("VS");
        }

        private void btnPower1_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("P1");
        }

        private void btnPower1_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("p1");
        }

        private void btnPower2_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("P2");
        }

        private void btnPower2_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("p2");
        }

        private void btnPower3_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("P3");
        }

        private void btnPower3_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("p3");
        }

        private void btnFunc1_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("F1");
        }

        private void btnFunc1_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("f1");
        }

        private void btnFunc2_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("F2");
        }

        private void btnFunc2_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("f2");
        }

        private void btnFunc3_Checked(object sender, RoutedEventArgs e)
        {
            SendCommand("F3");
        }

        private void btnFunc3_Unchecked(object sender, RoutedEventArgs e)
        {
            SendCommand("f3");
        }

        private void SendCommand(string command)
        {
            m_CommLink.Send(command);
        }
    }
}

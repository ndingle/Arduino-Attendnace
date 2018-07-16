using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StudyAttendance
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {

        string message = "";
        Brush backgroundColour = Brushes.White;
        float timeout = 0;
        ArduinoSerialComms arduino;

        public uint UIDResult = 0;


        public PopupWindow(string message,
                           Brush backgroundColour,
                           float timeout,
                           ArduinoSerialComms arduino = null)
        {

            InitializeComponent();
            this.message = message;
            this.backgroundColour = backgroundColour;
            this.timeout = timeout;

            //If we have an arduino to talk to then, setup the event
            if (arduino != null)
            {
                this.arduino = arduino;
                arduino.TagReceived += TagReceived;
            }

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
            anim.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, anim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Using the constructor settings, set the window up
            txbMessage.Text = message;
            brdColours.Background = backgroundColour;
            
            //If we are given a timeout value, then we fadeout
            if (timeout > 0)
            {
                Task.Delay(TimeSpan.FromMilliseconds(timeout)).ContinueWith(_ =>
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
                });
            }
            else
                btnCancel.Visibility = Visibility.Visible;
            
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (arduino != null)
                arduino.TagReceived -= TagReceived;

            Close();
        }


        /// <summary>
        /// The receiver function used to handle arduino communication for UID scanning
        /// </summary>
        /// <param name="uid">The uid received</param>
        void TagReceived(uint uid)
        {
            UIDResult = uid;

            //We done, disconnect and close window
            Dispatcher.BeginInvoke(new ThreadStart(() => {
                arduino.TagReceived -= TagReceived;
                Close();
            }));

        }

    }
}

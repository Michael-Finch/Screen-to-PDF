using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenToPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int coordsTLX = 0;
        int coordsTLY = 0;
        int coordsBRX = 0;
        int coordsBRY = 0;
        int coordsTPX = 0;
        int coordsTPY = 0;
        int numPages = 0;
        int delay = 0;

        int coordinateMode = 0; //0 = none, 1 = top left corner, 2 = bottom right corner, 3 = turn page button

        String filename = "";
        String directory = "";

        public MainWindow()
        {
            InitializeComponent();

            //Populate textboxes with reasonable defaults
            txtboxNumPages.Text = "0";
            txtboxDelay.Text = "0";
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if(coordinateMode != 0)
            {
                this.CaptureMouse();
            }
            else
            {
                this.ReleaseMouseCapture();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Not setting coordinates
            if (coordinateMode == 0)
            {
                //Do nothing
            }
            else
            {
                //Get mouse position
                var relativePosition = e.GetPosition(this);
                var point = PointToScreen(relativePosition);
                int mouseX = (int)point.X;
                int mouseY = (int)point.Y;

                //Update label
                //Top left corner
                if (coordinateMode == 1)
                {
                    coordsTLX = mouseX;
                    coordsTLY = mouseY;
                    lblCoordinatesTLC.Content = "(" + coordsTLX + "," + coordsTLY + ")";
                    txtboxOutputLog.AppendText("Top left corner set to (" + coordsTLX + "," + coordsTLY + ")\n");
                }
                //Bottom right corner
                else if (coordinateMode == 2)
                {
                    coordsBRX = mouseX;
                    coordsBRY = mouseY;
                    lblCoordinatesBRC.Content = "(" + coordsBRX + "," + coordsBRY + ")";
                    txtboxOutputLog.AppendText("Bottom right corner set to (" + coordsBRX + "," + coordsBRY + ")\n");
                }
                //Turn page button
                else if (coordinateMode == 3)
                {
                    coordsTPX = mouseX;
                    coordsTPY = mouseY;
                    lblCoordinatesTP.Content = "(" + coordsTPX + "," + coordsTPY + ")";
                    txtboxOutputLog.AppendText("Turn page set to (" + coordsTPX + "," + coordsTPY + ")\n");
                }

                coordinateMode = 0;
            }
        }

        private void btnCoordinatesTLC_Click(object sender, RoutedEventArgs e)
        {
            coordinateMode = 1;
            txtboxOutputLog.AppendText("Setting coordinates of top left corner\n");
        }

        private void btnCoordinatesBRC_Click(object sender, RoutedEventArgs e)
        {
            coordinateMode = 2;
            txtboxOutputLog.AppendText("Setting coordinates of bottom right corner\n");
        }

        private void btnCoordinatesTP_Click(object sender, RoutedEventArgs e)
        {
            coordinateMode = 3;
            txtboxOutputLog.AppendText("Setting coordinates of turn page button\n");
        }

        //Update number of pages and ensure clean input
        private void txtboxNumPages_LostFocus(object sender, RoutedEventArgs e)
        {
            int numToTry;
            bool validNumber = false;
            if (int.TryParse(txtboxNumPages.Text, out numToTry))
            {
                if(numToTry >= 0)
                {
                    validNumber = true;
                    numPages = numToTry;
                    txtboxOutputLog.AppendText("Pages set to " + numPages + "\n");
                }
            }
            if(validNumber == false)
            {
                txtboxOutputLog.AppendText("Invalid number of pages, please enter a non-ngegative integer.\n");
                txtboxNumPages.Text = numPages.ToString();
            }
        }

        //Update delay and ensure clean input
        private void txtboxDelay_LostFocus(object sender, RoutedEventArgs e)
        {
            int numToTry;
            bool validNumber = false;
            if (int.TryParse(txtboxDelay.Text, out numToTry))
            {
                if (numToTry >= 0)
                {
                    validNumber = true;
                    delay = numToTry;
                    txtboxOutputLog.AppendText("Delay set to " + delay + " ms\n");
                }
            }
            if (validNumber == false)
            {
                txtboxOutputLog.AppendText("Invalid delay, please enter a non-ngegative integer.\n");
                txtboxDelay.Text = delay.ToString();
            }
        }

        //Keep output log scrolled to the bottom
        private void txtboxOutputLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtboxOutputLog.ScrollToEnd();
        }
    }
}

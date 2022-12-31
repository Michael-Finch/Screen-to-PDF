using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using System.Threading.Tasks;

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
        int numPages = 1;
        int delay = 500;

        int coordinateMode = 0; //0 = none, 1 = top left corner, 2 = bottom right corner, 3 = turn page button

        String fileName = "";
        String directory = "";

        public MainWindow()
        {
            InitializeComponent();

            //Populate textboxes with reasonable defaults
            txtboxNumPages.Text = "1";
            txtboxDelay.Text = "500";
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (coordinateMode != 0)
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
                if (numToTry >= 0)
                {
                    validNumber = true;
                    numPages = numToTry;
                    txtboxOutputLog.AppendText("Pages set to " + numPages + "\n");
                }
            }
            if (validNumber == false)
            {
                txtboxOutputLog.AppendText("Invalid number of pages, please enter a non-ngegative integer\n");
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
                txtboxOutputLog.AppendText("Invalid delay, please enter a non-ngegative integer\n");
                txtboxDelay.Text = delay.ToString();
            }
        }

        //Keep output log scrolled to the bottom
        private void txtboxOutputLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtboxOutputLog.ScrollToEnd();
        }

        //Begin clicking through the book and saving screenshots
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (coordsTLX == coordsBRX && coordsTLY == coordsBRY)
            {
                txtboxOutputLog.AppendText("Error: Corners cannot be the same\n");
            }
            else if (coordsTLX > coordsBRX || coordsTLY > coordsBRY)
            {
                txtboxOutputLog.AppendText("Error: Corners cannot be in wrong positions\n");
            }
            else
            {
                txtboxOutputLog.AppendText("Waiting for save location\n");

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files | *.pdf";
                saveFileDialog.DefaultExt = "pdf";

                if (saveFileDialog.ShowDialog() == true)
                {
                    //Set the file name and save location
                    fileName = Path.GetFileName(saveFileDialog.FileName);
                    directory = Path.GetDirectoryName(saveFileDialog.FileName);
                    txtboxOutputLog.AppendText("Saving file " + fileName + "\n");
                    txtboxOutputLog.AppendText("Saving to directory " + directory + "\n");

                    txtboxOutputLog.AppendText("Do not move the cursor\nPress escape to close the program at any time\n");

                    //Set up progress bar
                    progressBar.Value = 0;
                    double percentPerPage = 75.0 / numPages; //Save 25% of the bar for creating the PDF file

                    //Sleep to avoid screenshotting save file dialog
                    await Task.Delay(1000);

                    //Go through each page
                    for (int i = 1; i <= numPages; ++i)
                    {
                        txtboxOutputLog.AppendText("Screenshotting page " + i + "\n");
                        //Save a screenshot
                        double screenLeft = SystemParameters.VirtualScreenLeft;
                        double screenTop = SystemParameters.VirtualScreenTop;
                        double screenWidth = SystemParameters.VirtualScreenWidth;
                        double screenHeight = SystemParameters.VirtualScreenHeight;

                        Bitmap bmp = new Bitmap(coordsBRX - coordsTLX, coordsBRY - coordsTLY);
                        Graphics g = Graphics.FromImage(bmp);

                        g.CopyFromScreen(coordsTLX, coordsTLY, 0, 0, bmp.Size);
                        bmp.Save(directory + "\\" + i + ".png", ImageFormat.Png);

                        if(i != numPages)
                        {
                            txtboxOutputLog.AppendText("Turning to page " + (int)(i + 1) + "\n");
                            //Click to turn the page
                            MouseOperations.SetCursorPosition(coordsTPX, coordsTPY);
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);

                            //Update progress bar
                            progressBar.Value = i * percentPerPage;

                            //Wait for page to turn
                            await Task.Delay(delay);
                        }
                    }

                    //Update log and progress bar
                    txtboxOutputLog.AppendText("Creating PDF\n");
                    await Task.Delay(500);

                    //Create a new pdf
                    iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(0, 0, coordsBRX - coordsTLX, coordsBRY - coordsTLY);
                    var ms = new MemoryStream();
                    var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
                    document.Open();

                    //Update log and progress bar
                    progressBar.Value = 80;
                    txtboxOutputLog.AppendText("Adding images to PDF\n");
                    await Task.Delay(500);

                    //Add each image to the pdf
                    for (int i = 1; i <= numPages; ++i)
                    {
                        var image = iTextSharp.text.Image.GetInstance(directory + "/" + i + ".png");
                        document.Add(image);
                    }

                    //Update log and progress bar
                    progressBar.Value = 90;
                    txtboxOutputLog.AppendText("Saving PDF\n");
                    await Task.Delay(500);

                    //Close the file
                    document.Close();
                    //Write the file
                    File.WriteAllBytes(saveFileDialog.FileName, ms.ToArray());

                    //Update log and progress bar
                    progressBar.Value = 95;
                    txtboxOutputLog.AppendText("Cleaning up\n");
                    await Task.Delay(500);

                    //Delete temporary image files
                    for (int i = 1; i <= numPages; ++i)
                    {
                        File.Delete(directory + "/" + i + ".png");
                    }
                    txtboxOutputLog.AppendText("Done!\n");

                    //Update progress bar
                    progressBar.Value = 100;
                }
                else
                {
                    txtboxOutputLog.AppendText("Error receiving save location\n");
                }
            }
        }
    }
}

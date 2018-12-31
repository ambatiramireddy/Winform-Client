using AddAppAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddWinFormsApp
{
    public partial class Form1 : Form
    {

        MessageClient client = new MessageClient();
        long screenId = 1;
        List<Message> ownerMessages;
        Timer timer = new Timer();
        int lastDisplayedOwnerMessageIndex = -1;
        System.Threading.Timer updateDisplayedMessagesTimer;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;//to remove form border
            this.WindowState = FormWindowState.Maximized;//to see form in full screen

            this.webBrowser1.Location = new Point(0, 0);
            this.webBrowser1.Margin = new Padding(0);
            this.webBrowser1.Visible = false;
            this.webBrowser1.ScrollBarsEnabled = false;

            this.pictureBox1.Location = new Point(0, 0);
            this.pictureBox1.Margin = new Padding(0);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Visible = false;

            DisplayText("Welcome");

            //triggers immediately and triggers once for every 1 minutes
            System.Threading.Timer getUserMessagesTimer = new System.Threading.Timer((state) =>
            {
                client.GetUserMessagesAsync(screenId).Wait();
            }, null, 0, 1 * 60 * 1000);

            //triggers immediately and triggers once for every 4 minutes
            //updateDisplayedMessagesTimer = new System.Threading.Timer((state) =>
            //{
            //    client.UpdateBookingStatusAsync().Wait();
            //}, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            GetOwnerMessages();

            timer.Interval = 1000 * 1; //every 5 seconds
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void GetOwnerMessages()
        {
            ownerMessages = client.GetOwnerMessages(screenId).ToList();
        }

        private void DisplayText(string text, string sender = null)
        {
            pictureBox1.Visible = false;
            webBrowser1.Visible = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("<html style='margin:0px;'>");
            sb.Append("<body>");
            sb.Append(@"<table style='height:100%;width:100%;text-align:center;vertical-align:middle;background-color:#0074D9;'>
                <tr><td>
                    <table>
                        <tr><td style='text-align:left;padding-left:50px;color:white;font-size:40pt'>" + text + @"</td></tr>");
            if (!string.IsNullOrWhiteSpace(sender))
            {
                sb.Append("<tr><td style='text-align:right;padding-right:20px;color:white;font-size:30pt'>-" + sender + "</td></tr>");
            }
            sb.Append("</table></td></tr></table>");
            sb.Append("</body>");
            sb.Append("</html>");

            webBrowser1.DocumentText = sb.ToString();
        }

        private void DisplayPicture(string imagePath)
        {
            webBrowser1.Visible = false;
            pictureBox1.Visible = true;
            Image img = Image.FromFile(imagePath);
            pictureBox1.Image = img;
            pictureBox1.Size = new Size(this.Width, this.Height);
        }

        private void DisplayMessage(Message message)
        {
            if (message.Type.Equals(MessageType.Text))
            {
                DisplayText(message.Content, message.Sender);
            }
            else if (message.Type.Equals(MessageType.Picture))
            {
                DisplayPicture(message.Content);
            }
        }

        Message currentUserMessage = null;
        Message previousUserMessage = null;
        int ownerMessageEndTime = 0;
        void timer_Tick(object sender, EventArgs e)
        {
            var currentDatetime = DateTime.UtcNow;
            var currentTime = (int)currentDatetime.TimeOfDay.TotalSeconds;
            previousUserMessage = currentUserMessage;
            if (client.UserMessages.Count > 0 && client.UserMessages.ContainsKey(currentTime))
            {
                currentUserMessage = client.UserMessages[currentTime];
                DisplayMessage(currentUserMessage);
                timer.Interval = currentUserMessage.Duration * 1000;

                //to update current message's displayed date and start time properties
                currentUserMessage.ScreenBooking.DisplayedDate = currentDatetime.Date;
                currentUserMessage.ScreenBooking.DisplayedStartTime = currentTime;
                client.DisplayedBookings.Add(currentUserMessage.ScreenBooking);

                //to reser owner message end time
                ownerMessageEndTime = 0;
            }
            else if (currentTime > ownerMessageEndTime)
            {
                lastDisplayedOwnerMessageIndex = (lastDisplayedOwnerMessageIndex == ownerMessages.Count - 1) ? 0 : lastDisplayedOwnerMessageIndex + 1;
                var message = ownerMessages[lastDisplayedOwnerMessageIndex];
                ownerMessageEndTime = currentTime + message.Duration;
                DisplayMessage(message);
                timer.Interval = 1 * 1000; // trigger timer for every 1 sec so that it can look for user messages
            }

            if (previousUserMessage != null && !previousUserMessage.ScreenBooking.Displayed)
            {
                //to update previous message's duration and displayed properties
                previousUserMessage.ScreenBooking.DisplayedDuration = currentTime - previousUserMessage.ScreenBooking.DisplayedStartTime;
                previousUserMessage.ScreenBooking.Displayed = true;
                client.DisplayedBookings.Add(previousUserMessage.ScreenBooking);
            }

            //trigger to update booking status
            if (client.DisplayedBookings.Count > 0)
            {
                Task.Run(() => client.UpdateBookingStatusAsync().Wait());
            }
        }

    } //class
}

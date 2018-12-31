using AddAppAPI.Enums;
using AddAppAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AddWinFormsApp
{
    public class MessageClient
    {
        HttpClient client;
        string apiBaseUrl = "http://localhost:63231/api/";
        public ConcurrentDictionary<int, Message> UserMessages = new ConcurrentDictionary<int, Message>();
        public ConcurrentBag<Message> OwnerMessages = new ConcurrentBag<Message>();
        public List<ScreenBooking> DisplayedBookings = new List<ScreenBooking>();

        public MessageClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(apiBaseUrl);
        }

        public async Task GetUserMessagesAsync(long screenId)
        {
            var response = await client.GetAsync($"ScreenBooking/GetScreenBookingsByScreenId?id={screenId}");
            if (response.IsSuccessStatusCode)
            {
                var allScreenBookings = await response.Content.ReadAsAsync<List<ScreenBooking>>();

                var currentDatetime = DateTime.UtcNow;
                var currentDate = currentDatetime.Date;
                var currentTime = (int)currentDatetime.TimeOfDay.TotalSeconds;
                int targetTime = currentTime + 2 * 60;
                var screenBookings = allScreenBookings.Where(s => !s.Displayed && s.ScheduledDate == currentDate && s.ScheduledStartTime >= currentTime && s.ScheduledStartTime < targetTime).OrderBy(s => s.ScheduledStartTime).ToArray();
                foreach (var item in screenBookings)
                {
                    var requestId = item.RequestId;
                    Request request = await this.GetRequestInfo(requestId);
                    if (request != null)
                    {
                        int startTime = item.ScheduledStartTime;
                        var message = new Message
                        {
                            ScreenBooking = item,
                            Type = (MessageType)Enum.Parse(typeof(MessageType), request.MessageTypeId.ToString()),
                            Content = request.Text,
                           // Sender = request.UserId.ToString(),
                            StartTime = startTime,
                            Duration = item.ScheduledDuration
                        };

                        if (!UserMessages.ContainsKey(startTime))
                        {
                            UserMessages.TryAdd(startTime, message);
                        }
                        else
                        {
                            UserMessages.TryUpdate(startTime, message, UserMessages[startTime]);
                        }
                    }

                }
            }

            return;
        }

        public async Task UpdateBookingStatusAsync()
        {
            for (int i = DisplayedBookings.Count - 1; i >= 0; i--)
            {
                var booking = DisplayedBookings[i];
                var jsonString = JsonConvert.SerializeObject(booking);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"/api/ScreenBooking/{booking.Id}", httpContent);
                if (response.IsSuccessStatusCode && booking.Displayed)
                {
                    Message message;
                    UserMessages.TryRemove(booking.ScheduledStartTime, out message);
                    DisplayedBookings.Remove(booking);
                }
            }
        }

        private async Task<Request> GetRequestInfo(long requestId)
        {
            Request request = null;
            var response = await client.GetAsync($"/api/Request/{requestId}");
            if (response.IsSuccessStatusCode)
            {
                request = await response.Content.ReadAsAsync<Request>();
            }

            return request;
        }

        //public IEnumerable<Message> GetUserMessages(string screenId)
        //{
        //    DateTime date = DateTime.UtcNow;
        //    int startMinutes = date.Hour * 60 + date.Minute;
        //    List<Message> userMessages = new List<Message>();

        //    userMessages.Add(new Message { Type = MessageType.Text, Content = "UM1:Happy Holi", Sender = "Sai", StartMinutes = startMinutes + 1, Duration = 1 });
        //    userMessages.Add(new Message { Type = MessageType.Text, Content = "UM2:హోలీ శుభాకాంక్షలు", Sender = "Ramireddy", StartMinutes = startMinutes + 2, Duration = 1 });
        //    userMessages.Add(new Message { Type = MessageType.Text, Content = "UM3:होली मुबारक", Sender = "Sourav", StartMinutes = startMinutes + 3, Duration = 1 });
        //    userMessages.Add(new Message { Type = MessageType.Text, Content = "UM4:இனிய ஹோலி", Sender = "Gopi", StartMinutes = startMinutes + 4, Duration = 1 });
        //    userMessages.Add(new Message { Type = MessageType.Image, Content = @"..\..\Landscapes\1.jpg", StartMinutes = startMinutes + 5, Duration = 1 });

        //    return userMessages;
        //}

        public IEnumerable<Message> GetOwnerMessages(long screenId)
        {
            List<Message> ownerMessages = new List<Message>();

            ownerMessages.Add(new Message { Type = MessageType.Text, Content = "OM1: Hi.................", Duration = 5 });
            ownerMessages.Add(new Message { Type = MessageType.Text, Content = "OM2: Hello.................", Duration = 5 });
            ownerMessages.Add(new Message { Type = MessageType.Picture, Content = @"..\..\Landscapes\3.jpg", Duration = 5 });
            ownerMessages.Add(new Message { Type = MessageType.Picture, Content = @"..\..\Landscapes\2.jpg", Duration = 10 });

            return ownerMessages;
        }
    }
}

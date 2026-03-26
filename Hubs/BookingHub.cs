using Microsoft.AspNetCore.SignalR;

namespace FootballBooking_BE.Hubs
{
    public class BookingHub : Hub
    {
        public async Task SendBookingUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveBookingUpdate", message);
        }
    }
}

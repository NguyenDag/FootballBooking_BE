using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "football_booking");
    }
}

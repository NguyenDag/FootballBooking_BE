using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FootballBooking_BE.Models;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FootballBooking_BE.Services.Implementations
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "football_booking")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc rỗng.");

            // Chỉ cho phép ảnh
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP, GIF).");

            // Giới hạn 10MB
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File ảnh không được vượt quá 10MB.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation()
                    .Width(1200).Height(800).Crop("limit") // giới hạn kích thước
                    .Quality("auto")
                    .FetchFormat("auto"),
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new InvalidOperationException($"Upload thất bại: {result.Error.Message}");

            return result.SecureUrl.AbsoluteUri;
        }
    }
}

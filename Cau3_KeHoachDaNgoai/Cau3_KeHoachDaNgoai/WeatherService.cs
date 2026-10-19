using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Cau3_KeHoachDaNgoai
{
    // =========================================
    // NGOẠI LỆ KHÔNG TÌM THẤY ĐỊA DANH
    // =========================================

    public class DiaDiemKhongTimThayException : Exception
    {
        public DiaDiemKhongTimThayException()
            : base("Không tìm thấy địa danh trên Open-Meteo.")
        {
        }
    }

    // =========================================
    // MODEL KẾT QUẢ DỰ BÁO
    // =========================================

    public class DuBaoThoiTiet
    {
        public DateOnly Ngay { get; set; }

        public int WeatherCode { get; set; }

        public double NhietDoCao { get; set; }

        public double NhietDoThap { get; set; }

        public int XacSuatMua { get; set; }
    }

    // =========================================
    // MODEL GEOCODING API
    // =========================================

    public class GeoResponse
    {
        [JsonPropertyName("results")]
        public List<GeoLocation>? Results { get; set; }
    }

    public class GeoLocation
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    // =========================================
    // MODEL WEATHER FORECAST API
    // =========================================

    public class ForecastResponse
    {
        [JsonPropertyName("daily")]
        public ForecastDaily? Daily { get; set; }
    }

    public class ForecastDaily
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("weather_code")]
        public List<int?> WeatherCode { get; set; } = new();

        [JsonPropertyName("temperature_2m_max")]
        public List<double?> TemperatureMax { get; set; } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<double?> TemperatureMin { get; set; } = new();

        [JsonPropertyName("precipitation_probability_max")]
        public List<int?> RainProbability { get; set; } = new();
    }

    // =========================================
    // SERVICE GỌI OPEN-METEO API
    // =========================================

    public static class WeatherService
    {
        // Dùng chung một HttpClient, Timeout 8 giây.
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        public static async Task<DuBaoThoiTiet?>
            LayDuBaoAsync(
                string diaDiem,
                DateOnly ngayDi,
                CancellationToken cancellationToken = default)
        {
            // 1. KIỂM TRA NGÀY DỰ BÁO
            DateOnly homNay =
                DateOnly.FromDateTime(DateTime.Today);

            // Forecast 16 ngày, tính cả hôm nay.
            if (ngayDi < homNay ||
                ngayDi > homNay.AddDays(15))
            {
                return null;
            }

            // 2. GỌI GEOCODING API LẤY TỌA ĐỘ
            string tenDiaDiem =
                Uri.EscapeDataString(diaDiem.Trim());

            string geoUrl =
                "https://geocoding-api.open-meteo.com/v1/search" +
                "?name=" + tenDiaDiem +
                "&count=1&language=vi&format=json";

            using var geoResponse =
                await Client.GetAsync(
                    geoUrl,
                    cancellationToken);

            geoResponse.EnsureSuccessStatusCode();

            var geo = await geoResponse.Content
                .ReadFromJsonAsync<GeoResponse>(
                    cancellationToken: cancellationToken);

            if (geo?.Results == null ||
                geo.Results.Count == 0)
            {
                throw new DiaDiemKhongTimThayException();
            }

            GeoLocation location = geo.Results[0];

            string latitude = location.Latitude.ToString(
                CultureInfo.InvariantCulture);

            string longitude = location.Longitude.ToString(
                CultureInfo.InvariantCulture);

            // 3. GỌI WEATHER FORECAST API
            string weatherUrl =
                "https://api.open-meteo.com/v1/forecast" +
                "?latitude=" + latitude +
                "&longitude=" + longitude +
                "&daily=weather_code," +
                "temperature_2m_max," +
                "temperature_2m_min," +
                "precipitation_probability_max" +
                "&timezone=auto" +
                "&forecast_days=16";

            using var weatherResponse =
                await Client.GetAsync(
                    weatherUrl,
                    cancellationToken);

            weatherResponse.EnsureSuccessStatusCode();

            var forecast = await weatherResponse.Content
                .ReadFromJsonAsync<ForecastResponse>(
                    cancellationToken: cancellationToken);

            ForecastDaily? daily = forecast?.Daily;

            if (daily == null)
                return null;

            // 4. TÌM ĐÚNG NGÀY ĐI TRONG DAILY.TIME
            string ngayCanTim = ngayDi.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            int index = daily.Time.FindIndex(
                x => x == ngayCanTim);

            if (index < 0)
                return null;

            if (index >= daily.WeatherCode.Count ||
                index >= daily.TemperatureMax.Count ||
                index >= daily.TemperatureMin.Count ||
                index >= daily.RainProbability.Count)
            {
                return null;
            }

            // 5. LẤY DỰ BÁO CÙNG CHỈ SỐ
            int? weatherCode =
                daily.WeatherCode[index];

            double? nhietDoCao =
                daily.TemperatureMax[index];

            double? nhietDoThap =
                daily.TemperatureMin[index];

            int? xacSuatMua =
                daily.RainProbability[index];

            if (!weatherCode.HasValue ||
                !nhietDoCao.HasValue ||
                !nhietDoThap.HasValue ||
                !xacSuatMua.HasValue)
            {
                return null;
            }

            // 6. TRẢ KẾT QUẢ
            return new DuBaoThoiTiet
            {
                Ngay = ngayDi,
                WeatherCode = weatherCode.Value,
                NhietDoCao = nhietDoCao.Value,
                NhietDoThap = nhietDoThap.Value,
                XacSuatMua = xacSuatMua.Value
            };
        }
    }
}

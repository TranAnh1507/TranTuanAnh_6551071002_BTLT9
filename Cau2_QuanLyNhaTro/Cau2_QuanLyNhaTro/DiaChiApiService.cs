
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Cau2_QuanLyNhaTro;

public sealed class DiaPhuong
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("wards")]
    public List<DiaPhuong>? Wards { get; set; }

    public override string ToString() => Name;
}

public static class DiaChiApiService
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri(
            "https://provinces.open-api.vn/api/v2/"),
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static async Task<List<DiaPhuong>>
        LayTinhThanhAsync(CancellationToken ct = default)
    {
        return await Client.GetFromJsonAsync<List<DiaPhuong>>(
            "", ct) ?? new();
    }

    public static async Task<List<DiaPhuong>>
        LayPhuongXaAsync(
            int maTinh,
            CancellationToken ct = default)
    {
        var tinh = await Client.GetFromJsonAsync<DiaPhuong>(
            $"p/{maTinh}?depth=2", ct);

        return tinh?.Wards ?? new();
    }
}

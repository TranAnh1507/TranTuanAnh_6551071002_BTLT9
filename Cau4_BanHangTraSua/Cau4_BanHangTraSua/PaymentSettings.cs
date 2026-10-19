using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Cau4_BanHangTraSua;

public sealed class PaymentSettings
{
    public string BankId { get; set; } = "";
    public string AccountNo { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Template { get; set; } = "compact2";
    public string ShopName { get; set; } = "";
}

public static class PaymentSettingsReader
{
    public static PaymentSettings Load()
    {
        // Đọc lại JSON mỗi lần tạo QR.
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return config.GetSection("Payment")
            .Get<PaymentSettings>()
            ?? throw new InvalidOperationException(
                "Thiếu mục Payment trong appsettings.json.");
    }

    public static void Validate(PaymentSettings p)
    {
        if (string.IsNullOrWhiteSpace(p.BankId) ||
            string.IsNullOrWhiteSpace(p.AccountNo) ||
            p.AccountNo.Contains(
                "CHANGE",
                StringComparison.OrdinalIgnoreCase) ||
            p.AccountNo.Contains(
                "YOUR",
                StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(p.AccountName) ||
            string.IsNullOrWhiteSpace(p.Template))
        {
            throw new InvalidOperationException(
                "Hãy điền BankId, AccountNo, " +
                "AccountName và Template trong appsettings.json.");
        }
    }

    public static string BuildVietQrUrl(
        PaymentSettings payment,
        decimal amount,
        string description)
    {
        Validate(payment);

        if (amount <= 0 ||
            amount > 9999999999999m ||
            decimal.Truncate(amount) != amount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Số tiền phải là số nguyên dương.");
        }

        static string E(string value) =>
            Uri.EscapeDataString(value.Trim());

        string money = amount.ToString(
            "0", CultureInfo.InvariantCulture);

        return
            $"https://img.vietqr.io/image/" +
            $"{E(payment.BankId)}-" +
            $"{E(payment.AccountNo)}-" +
            $"{E(payment.Template)}.png" +
            $"?amount={E(money)}" +
            $"&addInfo={E(description)}" +
            $"&accountName={E(payment.AccountName)}";
    }
}

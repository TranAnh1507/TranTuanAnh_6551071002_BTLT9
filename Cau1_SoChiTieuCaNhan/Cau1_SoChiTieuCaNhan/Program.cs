
using Cau1_SoChiTieuCaNhan.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Cau1_SoChiTieuCaNhan
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Ghi log vào thư mục Logs cạnh file exe.
            string logPath = Path.Combine(
                AppContext.BaseDirectory,
                "Logs",
                "log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
                        "[{Level:u3}] {Message:lj}" +
                        "{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                string environment =
                    Environment.GetEnvironmentVariable(
                        "DOTNET_ENVIRONMENT") ?? "Production";

                // Đọc cấu hình từ JSON.
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false,
                        reloadOnChange: false)
                    .AddJsonFile(
                        $"appsettings.{environment}.json",
                        optional: true,
                        reloadOnChange: false)
                    .Build();

                var settings = configuration.Get<AppSettings>()
                    ?? throw new Exception(
                        "Không đọc được cấu hình ứng dụng.");

                string connectionString =
                    settings.ConnectionStrings.DefaultConnection;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new Exception(
                        "Chưa cấu hình chuỗi kết nối SQL Server.");
                }

                // Tạo DbContextOptions dùng chung.
                var options =
                    new DbContextOptionsBuilder<
                        Cau1_SoChiTieuCaNhanContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                // Mỗi thao tác sẽ tạo một DbContext mới.
                Func<Cau1_SoChiTieuCaNhanContext> dbFactory =
                    () => new Cau1_SoChiTieuCaNhanContext(options);

                // Kiểm tra kết nối trước khi mở Form.
                using (var db = dbFactory())
                {
                    db.Database.OpenConnection();

                    int soKhoanChi = db.Set<ChiTieu>().Count();

                    db.Database.CloseConnection();

                    Log.Information(
                        "Kết nối database thành công. Số khoản chi: {SoLuong}",
                        soKhoanChi);
                }

                Log.Information(
                    "Ứng dụng Sổ chi tiêu cá nhân khởi động.");

                Application.Run(new FormChiTieu(dbFactory));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khởi động ứng dụng");

                MessageBox.Show(
                    "Không thể khởi động ứng dụng!\n\n" +
                    "Chi tiết lỗi:\n" +
                    ex.GetBaseException().Message,
                    "Lỗi kết nối SQL Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Log.Information("Ứng dụng kết thúc.");
                Log.CloseAndFlush();
            }
        }
    }
}

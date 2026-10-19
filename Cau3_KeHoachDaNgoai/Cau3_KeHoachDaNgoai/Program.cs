using Cau3_KeHoachDaNgoai.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Cau3_KeHoachDaNgoai;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        string logsDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Cau3_KeHoachDaNgoai",
            "Logs");

        Directory.CreateDirectory(logsDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(logsDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string connectionString =
                config.GetConnectionString("DefaultConnection")
                ?? throw new Exception(
                    "Chưa cấu hình kết nối database.");

            var options =
                new DbContextOptionsBuilder<
                    Cau3_KeHoachDaNgoaiContext>()
                .UseSqlServer(connectionString)
                .Options;

            Func<Cau3_KeHoachDaNgoaiContext> dbFactory =
                () => new Cau3_KeHoachDaNgoaiContext(options);

            // Kiểm tra database khi khởi động.
            using (var db = dbFactory())
            {
                int soChuyen = db.Set<ChuyenDi>().Count();

                Log.Information(
                    "Kết nối CSDL thành công, có {SoChuyen} chuyến.",
                    soChuyen);
            }

            Application.Run(new FormChuyenDi(dbFactory));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lỗi khởi động ứng dụng");

            MessageBox.Show(
                "Không thể khởi động ứng dụng.\n" +
                "Vui lòng kiểm tra SQL Server và appsettings.json.",
                "Lỗi khởi động",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}


using Cau4_BanHangTraSua.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Cau4_BanHangTraSua;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        string logs = Path.Combine(
            AppContext.BaseDirectory,
            "Logs",
            "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logs,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
                    "[{Level:u3}] {Message:lj}" +
                    "{NewLine}{Exception}")
            .CreateLogger();

        Application.ThreadException += (_, e) =>
        {
            Log.Error(e.Exception,
                "Lỗi giao diện chưa xử lý");

            MessageBox.Show(
                "Đã xảy ra lỗi, thông tin đã được ghi lại.",
                "Thông báo");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Log.Fatal(ex, "Lỗi không xử lý");

            Log.CloseAndFlush();
        };

        try
        {
            string connectionString =
                new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false)
                    .Build()
                    .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Thiếu connection string.");

            var options =
                new DbContextOptionsBuilder<
                    Cau4_BanHangTraSuaContext>()
                .UseSqlServer(connectionString)
                .Options;

            Func<Cau4_BanHangTraSuaContext> factory =
                () => new Cau4_BanHangTraSuaContext(options);

            using (var db = factory())
            {
                db.Set<DanhMucMon>().Count();
                db.Set<MonUong>().Count();
                db.Set<HoaDon>().Count();
            }

            Application.Run(new FormBanHang(factory));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Không thể khởi động");

            MessageBox.Show(
                "Không khởi động được.\n" +
                "Kiểm tra SQL Server và appsettings.json.\n\n" +
                ex.GetBaseException().Message,
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

using Cau2_QuanLyNhaTro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Cau2_QuanLyNhaTro;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(
            UnhandledExceptionMode.CatchException);

        ApplicationConfiguration.Initialize();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
                    "[{Level:u3}] {Message:lj}" +
                    "{NewLine}{Exception}")
            .CreateLogger();

        Application.ThreadException += (sender, e) =>
        {
            Log.Error(e.Exception,
                "Ngoại lệ chưa xử lý trên UI thread");

            MessageBox.Show(
                "Đã xảy ra lỗi, thông tin đã được ghi lại.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        };

        AppDomain.CurrentDomain.UnhandledException +=
            (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Log.Fatal(ex,
                        "Ngoại lệ chưa xử lý. Terminating={Terminating}",
                        e.IsTerminating);
                }
                else
                {
                    Log.Fatal(
                        "Ngoại lệ không xác định: {Error}",
                        e.ExceptionObject);
                }

                Log.CloseAndFlush();
            };

        TaskScheduler.UnobservedTaskException +=
            (sender, e) =>
            {
                Log.Error(e.Exception,
                    "Lỗi Task chưa được xử lý");

                e.SetObserved();
            };

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var settings = config.Get<AppSettings>()
                ?? throw new Exception(
                    "Không đọc được cấu hình ứng dụng.");

            var connectionString =
                settings.ConnectionStrings.DefaultConnection;

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Thiếu chuỗi kết nối CSDL.");

            var options =
                new DbContextOptionsBuilder<
                    Cau2_QuanLyNhaTroContext>()
                .UseSqlServer(connectionString)
                .Options;

            Func<Cau2_QuanLyNhaTroContext> dbFactory =
                () => new Cau2_QuanLyNhaTroContext(options);

            // Kiểm tra cả kết nối và bảng dữ liệu.
            using (var db = dbFactory())
            {
                db.Set<PhongTro>().Count();
                db.Set<KhachThue>().Count();
            }

            Log.Information(
                "Khởi động ứng dụng Quản lý nhà trọ");

            Application.Run(new FormKhachThue(dbFactory));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lỗi khởi động ứng dụng");

            MessageBox.Show(
                "Không thể khởi động ứng dụng.\n" +
                "Vui lòng kiểm tra SQL Server và file cấu hình.",
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


namespace Cau1_SoChiTieuCaNhan
{
    public sealed class AppSettings
    {
        public ConnectionStringsSettings ConnectionStrings { get; set; }
            = new();
    }

    public sealed class ConnectionStringsSettings
    {
        public string DefaultConnection { get; set; }
            = string.Empty;
    }
}

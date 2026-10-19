using System.Globalization;
using Cau3_KeHoachDaNgoai.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cau3_KeHoachDaNgoai;

public class FormChuyenDi : Form
{
    private readonly Func<Cau3_KeHoachDaNgoaiContext> _dbFactory;

    private string _language;
    private CultureInfo _culture;
    private ChuyenDiRow? _selected;
    private DuBaoThoiTiet? _forecast;
    private string _weatherPlace = "";
    private CancellationTokenSource? _weatherCts;

    private string _statusVi = "";
    private string _statusEn = "";

    // Các control nhập dữ liệu
    private TextBox txtTenChuyen = new();
    private TextBox txtDiaDiem = new();
    private DateTimePicker dtpNgayDi = new();
    private NumericUpDown nudSoNguoi = new();
    private NumericUpDown nudNganSach = new();
    private ComboBox cboPhuongTien = new();
    private CheckBox chkDaHoanThanh = new();
    private TextBox txtGhiChu = new();

    // Nút CRUD
    private Button btnThem = new();
    private Button btnSua = new();
    private Button btnXoa = new();
    private Button btnLamMoi = new();

    // Tìm kiếm
    private TextBox txtTim = new();
    private CheckBox chkChuaHoanThanh = new();
    private DateTimePicker dtpTuNgay = new();
    private DateTimePicker dtpDenNgay = new();
    private Button btnTim = new();
    private Button btnBoLoc = new();
    private DataGridView dgvChuyenDi = new();

    // Thời tiết
    private Button btnXemThoiTiet = new();
    private ComboBox cboNgonNgu = new();
    private ProgressBar progressWeather = new();
    private Panel pnlWeather = new();
    private Panel pnlAdvice = new();

    private Label lblWeatherDay = new();
    private Label lblWeatherIcon = new();
    private Label lblWeatherDescription = new();
    private Label lblTemperature = new();
    private Label lblRain = new();
    private Label lblAdvice = new();
    private Label lblStatus = new();

    // Nhãn cần dịch
    private GroupBox grpThongTin = new();
    private GroupBox grpWeather = new();
    private GroupBox grpTim = new();

    private Label lblTen = new();
    private Label lblDiaDiem = new();
    private Label lblNgay = new();
    private Label lblSoNguoi = new();
    private Label lblNganSach = new();
    private Label lblPhuongTien = new();
    private Label lblGhiChu = new();
    private Label lblNgonNgu = new();
    private Label lblTim = new();
    private Label lblTuNgay = new();
    private Label lblDenNgay = new();

    public FormChuyenDi(
        Func<Cau3_KeHoachDaNgoaiContext> dbFactory)
    {
        _dbFactory = dbFactory;

        _language = UserSettingsStore.Load().Language;
        _culture = CultureInfo.GetCultureInfo(
            _language == "en" ? "en-US" : "vi-VN");

        KhoiTaoGiaoDien();
        GanSuKien();
        ApDungNgonNgu();

        Shown += (s, e) => TaiDanhSach();
    }

    private string T(string vi, string en)
        => _language == "en" ? en : vi;

    private void SetStatus(string vi, string en)
    {
        _statusVi = vi;
        _statusEn = en;
        lblStatus.Text = T(vi, en);
    }

    private Label TaoLabel(int x, int y, int width)
    {
        return new Label
        {
            Location = new Point(x, y),
            Size = new Size(width, 27),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private Button TaoNut(int x, int y, int width)
    {
        return new Button
        {
            Location = new Point(x, y),
            Size = new Size(width, 34),
            FlatStyle = FlatStyle.System,
            Cursor = Cursors.Hand
        };
    }

    private void TaoCot(
        string name, string property, int width)
    {
        dgvChuyenDi.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = property,
                FillWeight = width,
                ReadOnly = true
            });
    }

    // ==========================================
    // 1. KHỞI TẠO GIAO DIỆN
    // ==========================================

    private void KhoiTaoGiaoDien()
    {
        StartPosition = FormStartPosition.CenterScreen;
        // Cửa sổ mở lớn hơn và có thể phóng to bằng nút Maximize.
        ClientSize = new Size(1250, 780);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(1126, 720);
        Font = new Font("Segoe UI", 9);
        BackColor = Color.FromArgb(248, 249, 252);

        // ===== NHÓM NHẬP THÔNG TIN =====

        grpThongTin.Location = new Point(12, 12);
        grpThongTin.Size = new Size(550, 366);
        Controls.Add(grpThongTin);

        lblTen = TaoLabel(15, 35, 135);
        txtTenChuyen.Location = new Point(155, 35);
        txtTenChuyen.Size = new Size(375, 27);
        txtTenChuyen.MaxLength = 100;

        lblDiaDiem = TaoLabel(15, 73, 135);
        txtDiaDiem.Location = new Point(155, 73);
        txtDiaDiem.Size = new Size(375, 27);
        txtDiaDiem.MaxLength = 100;

        lblNgay = TaoLabel(15, 111, 135);
        dtpNgayDi.Location = new Point(155, 111);
        dtpNgayDi.Size = new Size(200, 27);
        dtpNgayDi.Format = DateTimePickerFormat.Custom;
        dtpNgayDi.Value = DateTime.Today.AddDays(6);

        lblSoNguoi = TaoLabel(15, 149, 135);
        nudSoNguoi.Location = new Point(155, 149);
        nudSoNguoi.Size = new Size(200, 27);
        nudSoNguoi.Minimum = 1;
        nudSoNguoi.Maximum = 1000000;
        nudSoNguoi.Value = 1;

        lblNganSach = TaoLabel(15, 187, 135);
        nudNganSach.Location = new Point(155, 187);
        nudNganSach.Size = new Size(200, 27);
        nudNganSach.Minimum = 0;
        nudNganSach.Maximum = 999999999999999999m;
        nudNganSach.Increment = 100000;
        nudNganSach.DecimalPlaces = 0;
        nudNganSach.ThousandsSeparator = true;

        lblPhuongTien = TaoLabel(15, 225, 135);
        cboPhuongTien.Location = new Point(155, 225);
        cboPhuongTien.Size = new Size(200, 27);
        cboPhuongTien.DropDownStyle =
            ComboBoxStyle.DropDownList;

        chkDaHoanThanh.Location = new Point(155, 263);
        chkDaHoanThanh.Size = new Size(350, 27);

        lblGhiChu = TaoLabel(15, 301, 135);
        txtGhiChu.Location = new Point(155, 301);
        txtGhiChu.Size = new Size(375, 48);
        txtGhiChu.Multiline = true;
        txtGhiChu.MaxLength = 200;
        txtGhiChu.ScrollBars = ScrollBars.Vertical;

        grpThongTin.Controls.AddRange(new Control[]
        {
            lblTen, txtTenChuyen,
            lblDiaDiem, txtDiaDiem,
            lblNgay, dtpNgayDi,
            lblSoNguoi, nudSoNguoi,
            lblNganSach, nudNganSach,
            lblPhuongTien, cboPhuongTien,
            chkDaHoanThanh,
            lblGhiChu, txtGhiChu
        });

        // ===== CÁC NÚT CRUD =====

        btnThem = TaoNut(27, 391, 105);
        btnSua = TaoNut(143, 391, 105);
        btnXoa = TaoNut(259, 391, 105);
        btnLamMoi = TaoNut(375, 391, 120);

        Controls.AddRange(new Control[]
        {
            btnThem, btnSua, btnXoa, btnLamMoi
        });

        // ===== NGÔN NGỮ =====

        lblNgonNgu = TaoLabel(782, 23, 110);
        Controls.Add(lblNgonNgu);

        cboNgonNgu.Location = new Point(902, 23);
        cboNgonNgu.Size = new Size(195, 27);
        cboNgonNgu.DropDownStyle =
            ComboBoxStyle.DropDownList;

        cboNgonNgu.Items.AddRange(new object[]
        {
            "Tiếng Việt",
            "English"
        });

        cboNgonNgu.SelectedIndex =
            _language == "en" ? 1 : 0;

        Controls.Add(cboNgonNgu);

        // ===== NHÓM THỜI TIẾT =====

        btnXemThoiTiet = TaoNut(580, 23, 175);
        Controls.Add(btnXemThoiTiet);

        progressWeather.Location = new Point(580, 60);
        progressWeather.Size = new Size(517, 12);
        progressWeather.Style = ProgressBarStyle.Marquee;
        progressWeather.MarqueeAnimationSpeed = 25;
        progressWeather.Visible = false;
        Controls.Add(progressWeather);

        grpWeather.Location = new Point(575, 84);
        grpWeather.Size = new Size(523, 294);
        Controls.Add(grpWeather);

        pnlWeather.Location = new Point(10, 26);
        pnlWeather.Size = new Size(503, 258);
        pnlWeather.BackColor = Color.White;
        grpWeather.Controls.Add(pnlWeather);

        lblWeatherDay.Location = new Point(12, 10);
        lblWeatherDay.Size = new Size(470, 28);
        lblWeatherDay.Font = new Font(
            "Segoe UI", 10, FontStyle.Bold);

        lblWeatherIcon.Location = new Point(14, 62);
        lblWeatherIcon.Size = new Size(110, 100);
        lblWeatherIcon.Font = new Font("Segoe UI Emoji", 52);
        lblWeatherIcon.TextAlign =
            ContentAlignment.MiddleCenter;

        lblWeatherDescription.Location =
            new Point(145, 58);
        lblWeatherDescription.Size = new Size(338, 37);
        lblWeatherDescription.Font = new Font(
            "Segoe UI", 15, FontStyle.Bold);

        lblTemperature.Location = new Point(145, 103);
        lblTemperature.Size = new Size(338, 26);

        lblRain.Location = new Point(145, 137);
        lblRain.Size = new Size(338, 26);

        pnlAdvice.Location = new Point(12, 194);
        pnlAdvice.Size = new Size(478, 53);

        lblAdvice.Location = new Point(10, 6);
        lblAdvice.Size = new Size(458, 41);
        lblAdvice.TextAlign =
            ContentAlignment.MiddleCenter;
        lblAdvice.Font = new Font(
            "Segoe UI", 9, FontStyle.Bold);

        pnlAdvice.Controls.Add(lblAdvice);

        pnlWeather.Controls.AddRange(new Control[]
        {
            lblWeatherDay, lblWeatherIcon,
            lblWeatherDescription, lblTemperature,
            lblRain, pnlAdvice
        });

        // ===== TÌM KIẾM VÀ LỌC =====

        grpTim.Location = new Point(12, 440);
        grpTim.Size = new Size(1086, 89);
        Controls.Add(grpTim);

        lblTim = TaoLabel(13, 19, 115);
        txtTim.Location = new Point(130, 19);
        txtTim.Size = new Size(305, 27);

        chkChuaHoanThanh.Location =
            new Point(455, 19);
        chkChuaHoanThanh.Size = new Size(270, 27);

        lblTuNgay = TaoLabel(13, 53, 115);
        dtpTuNgay.Location = new Point(130, 53);
        dtpTuNgay.Size = new Size(175, 27);
        dtpTuNgay.Format = DateTimePickerFormat.Custom;
        dtpTuNgay.Value = DateTime.Today;

        lblDenNgay = TaoLabel(325, 53, 95);
        dtpDenNgay.Location = new Point(425, 53);
        dtpDenNgay.Size = new Size(175, 27);
        dtpDenNgay.Format = DateTimePickerFormat.Custom;
        dtpDenNgay.Value = DateTime.Today.AddDays(90);

        btnTim = TaoNut(735, 45, 145);
        btnBoLoc = TaoNut(890, 45, 145);

        grpTim.Controls.AddRange(new Control[]
        {
            lblTim, txtTim, chkChuaHoanThanh,
            lblTuNgay, dtpTuNgay,
            lblDenNgay, dtpDenNgay,
            btnTim, btnBoLoc
        });

        // ===== BẢNG DANH SÁCH =====

        dgvChuyenDi.Location = new Point(12, 540);
        dgvChuyenDi.Size = new Size(1086, 165);
        dgvChuyenDi.BackgroundColor = Color.White;
        dgvChuyenDi.ReadOnly = true;
        dgvChuyenDi.MultiSelect = false;
        dgvChuyenDi.AllowUserToAddRows = false;
        dgvChuyenDi.AllowUserToDeleteRows = false;
        dgvChuyenDi.RowHeadersVisible = false;
        dgvChuyenDi.AutoGenerateColumns = false;
        dgvChuyenDi.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;
        dgvChuyenDi.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;
        dgvChuyenDi.ColumnHeadersHeight = 34;

        TaoCot("MaChuyen", "MaChuyen", 55);
        TaoCot("TenChuyen", "TenChuyen", 175);
        TaoCot("DiaDiem", "DiaDiem", 125);
        TaoCot("NgayDi", "NgayDi", 110);
        TaoCot("SoNguoi", "SoNguoi", 75);
        TaoCot("NganSach", "NganSachHienThi", 130);
        TaoCot("PhuongTien", "PhuongTienHienThi", 105);

        dgvChuyenDi.Columns.Add(
            new DataGridViewCheckBoxColumn
            {
                Name = "DaHoanThanh",
                DataPropertyName = "DaHoanThanh",
                FillWeight = 100,
                ReadOnly = true
            });

        Controls.Add(dgvChuyenDi);

        lblStatus.Location = new Point(12, 711);
        lblStatus.Size = new Size(1086, 25);
        lblStatus.ForeColor = Color.DarkSlateBlue;
        Controls.Add(lblStatus);

        // Tự giãn các khung khi kéo cửa sổ hoặc nhấn Maximize.
        ClientSizeChanged += (_, _) => CapNhatBoCucTheoKichThuoc();
        CapNhatBoCucTheoKichThuoc();
    }

    // ==========================================
    // BỐ CỤC TỰ GIÃN THEO KÍCH THƯỚC CỬA SỔ
    // Không can thiệp CRUD, EF Core hoặc API.
    // ==========================================

    private void CapNhatBoCucTheoKichThuoc()
    {
        if (grpWeather.IsDisposed || dgvChuyenDi.IsDisposed)
            return;

        SuspendLayout();
        grpWeather.SuspendLayout();
        pnlWeather.SuspendLayout();
        grpTim.SuspendLayout();

        try
        {
            int rong = ClientSize.Width;
            int cao = ClientSize.Height;
            const int le = 12;

            // Khung nhập giữ kích thước ổn định ở bên trái.
            // Phần bên phải tự mở rộng theo chiều ngang.
            grpThongTin.SetBounds(12, 12, 550, 366);

            int traiThoiTiet = 575;
            int rongThoiTiet = Math.Max(360, rong - traiThoiTiet - le);

            btnXemThoiTiet.SetBounds(580, 23, 175, 34);
            lblNgonNgu.SetBounds(rong - 315, 23, 95, 27);
            cboNgonNgu.SetBounds(rong - 210, 23, 195, 27);
            progressWeather.SetBounds(580, 60, rong - 592, 12);

            grpWeather.SetBounds(traiThoiTiet, 84, rongThoiTiet, 294);
            pnlWeather.SetBounds(10, 26, rongThoiTiet - 20, 258);

            int trongWeather = pnlWeather.ClientSize.Width;
            lblWeatherDay.SetBounds(12, 10, trongWeather - 24, 28);
            lblWeatherDescription.SetBounds(
                145, 58, Math.Max(150, trongWeather - 157), 37);
            lblTemperature.SetBounds(
                145, 103, Math.Max(150, trongWeather - 157), 26);
            lblRain.SetBounds(
                145, 137, Math.Max(150, trongWeather - 157), 26);
            pnlAdvice.SetBounds(12, 194, trongWeather - 24, 53);
            lblAdvice.SetBounds(10, 6, pnlAdvice.Width - 20, 41);

            // Giữ nguyên hai hàng bộ lọc; chỉ nới rộng khung và
            // đẩy hai nút sang mép phải khi cửa sổ lớn hơn.
            int rongLoc = rong - 2 * le;
            grpTim.SetBounds(le, 440, rongLoc, 89);
            btnBoLoc.SetBounds(rongLoc - 158, 45, 145, 34);
            btnTim.SetBounds(rongLoc - 313, 45, 145, 34);

            // Bảng dữ liệu tự giãn cả chiều ngang và chiều cao.
            int caoBang = Math.Max(80, cao - 540 - 36);
            dgvChuyenDi.SetBounds(le, 540, rong - 2 * le, caoBang);

            // Dòng trạng thái luôn nằm sát đáy cửa sổ.
            lblStatus.SetBounds(le, cao - 29, rong - 2 * le, 25);
        }
        finally
        {
            grpTim.ResumeLayout(true);
            pnlWeather.ResumeLayout(true);
            grpWeather.ResumeLayout(true);
            ResumeLayout(true);
        }
    }

    // ==========================================
    // 2. PHƯƠNG TIỆN VÀ ĐA NGÔN NGỮ
    // ==========================================

    private void TaoDanhSachPhuongTien()
    {
        string? giaTriCu =
            (cboPhuongTien.SelectedItem as VehicleOption)?.Storage;

        var danhSach = new List<VehicleOption>
        {
            new("Xe máy", T("Xe máy", "Motorbike")),
            new("Ô tô", T("Ô tô", "Car")),
            new("Xe khách", T("Xe khách", "Coach bus")),
            new("Tàu hỏa", T("Tàu hỏa", "Train"))
        };

        cboPhuongTien.DataSource = null;
        cboPhuongTien.DisplayMember = "Display";
        cboPhuongTien.ValueMember = "Storage";
        cboPhuongTien.DataSource = danhSach;

        var mucCu = danhSach.FirstOrDefault(x =>
            x.Storage == giaTriCu);

        if (mucCu != null)
            cboPhuongTien.SelectedItem = mucCu;
        else
            cboPhuongTien.SelectedIndex = 0;
    }

    private string DinhDangTien(decimal tien)
    {
        return tien.ToString("N0", _culture) +
            T(" đ", " VND");
    }

    private string HienThiPhuongTien(string giaTri)
    {
        return giaTri switch
        {
            "Xe máy" => T("Xe máy", "Motorbike"),
            "Ô tô" => T("Ô tô", "Car"),
            "Xe khách" => T("Xe khách", "Coach bus"),
            "Tàu hỏa" => T("Tàu hỏa", "Train"),
            _ => giaTri
        };
    }

    private void ApDungNgonNgu()
    {
        _culture = CultureInfo.GetCultureInfo(
            _language == "en" ? "en-US" : "vi-VN");

        Text = T("Kế hoạch dã ngoại cuối tuần",
                 "Weekend Trip Planner");

        grpThongTin.Text = T(
            "Thông tin chuyến đi", "Trip information");

        grpWeather.Text = T(
            "Dự báo thời tiết", "Weather forecast");

        grpTim.Text = T(
            "Tìm kiếm / Lọc", "Search / Filters");

        lblTen.Text = T("Tên chuyến", "Trip name");
        lblDiaDiem.Text = T("Địa điểm", "Location");
        lblNgay.Text = T("Ngày đi", "Trip date");
        lblSoNguoi.Text = T("Số người", "People");
        lblNganSach.Text = T("Ngân sách", "Budget");
        lblPhuongTien.Text = T("Phương tiện", "Transport");
        lblGhiChu.Text = T("Ghi chú", "Notes");
        lblNgonNgu.Text = T("Ngôn ngữ", "Language");

        chkDaHoanThanh.Text = T(
            "Đã hoàn thành", "Completed");

        btnThem.Text = T("Thêm", "Add");
        btnSua.Text = T("Sửa", "Edit");
        btnXoa.Text = T("Xóa", "Delete");
        btnLamMoi.Text = T("Làm mới", "Refresh");

        btnXemThoiTiet.Text = T(
            "Xem thời tiết", "Check weather");

        lblTim.Text = T("Tìm địa điểm", "Search trips");

        chkChuaHoanThanh.Text = T(
            "Chỉ hiện chuyến chưa hoàn thành",
            "Only incomplete trips");

        lblTuNgay.Text = T("Từ ngày", "From date");
        lblDenNgay.Text = T("Đến ngày", "To date");

        btnTim.Text = T("Tìm", "Search");
        btnBoLoc.Text = T("Bỏ lọc", "Clear filters");

        txtTim.PlaceholderText = T(
            "Tên chuyến hoặc địa điểm",
            "Trip name or location");

        string format = _language == "en"
            ? "MM/dd/yyyy"
            : "dd/MM/yyyy";

        dtpNgayDi.CustomFormat = format;
        dtpTuNgay.CustomFormat = format;
        dtpDenNgay.CustomFormat = format;

        TaoDanhSachPhuongTien();

        dgvChuyenDi.Columns["MaChuyen"]!.HeaderText =
            T("Mã", "ID");

        dgvChuyenDi.Columns["TenChuyen"]!.HeaderText =
            T("Tên chuyến", "Trip name");

        dgvChuyenDi.Columns["DiaDiem"]!.HeaderText =
            T("Địa điểm", "Location");

        dgvChuyenDi.Columns["NgayDi"]!.HeaderText =
            T("Ngày đi", "Trip date");

        dgvChuyenDi.Columns["SoNguoi"]!.HeaderText =
            T("Số người", "People");

        dgvChuyenDi.Columns["NganSach"]!.HeaderText =
            T("Ngân sách", "Budget");

        dgvChuyenDi.Columns["PhuongTien"]!.HeaderText =
            T("Phương tiện", "Transport");

        dgvChuyenDi.Columns["DaHoanThanh"]!.HeaderText =
            T("Hoàn thành", "Completed");

        dgvChuyenDi.Columns["NgayDi"]!
            .DefaultCellStyle.Format = format;

        dgvChuyenDi.Columns["NgayDi"]!
            .DefaultCellStyle.FormatProvider = _culture;
        // Đổi cả dữ liệu tiền và phương tiện trên lưới.
        if (dgvChuyenDi.DataSource is List<ChuyenDiRow> rows)
        {
            foreach (var row in rows)
            {
                row.NganSachHienThi =
                    DinhDangTien(row.NganSach);

                row.PhuongTienHienThi =
                    HienThiPhuongTien(row.PhuongTien);
            }

            dgvChuyenDi.Refresh();
        }

        lblStatus.Text = T(_statusVi, _statusEn);

        HienThiThoiTiet();
    }

    // ==========================================
    // 3. GẮN CÁC SỰ KIỆN
    // ==========================================

    private void GanSuKien()
    {
        btnThem.Click += (s, e) => ThemChuyenDi();
        btnSua.Click += (s, e) => SuaChuyenDi();
        btnXoa.Click += (s, e) => XoaChuyenDi();
        btnLamMoi.Click += (s, e) => LamMoi();

        btnTim.Click += (s, e) => TaiDanhSach();

        btnBoLoc.Click += (s, e) =>
        {
            txtTim.Clear();
            chkChuaHoanThanh.Checked = false;
            dtpTuNgay.Value = DateTime.Today;
            dtpDenNgay.Value = DateTime.Today.AddDays(90);
            TaiDanhSach();
        };

        chkChuaHoanThanh.CheckedChanged +=
            (s, e) => TaiDanhSach();

        dgvChuyenDi.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;

            var row = dgvChuyenDi.Rows[e.RowIndex]
                .DataBoundItem as ChuyenDiRow;

            if (row != null)
                ChonChuyenDi(row);
        };

        cboNgonNgu.SelectedIndexChanged += (s, e) =>
        {
            string language =
                cboNgonNgu.SelectedIndex == 1 ? "en" : "vi";

            if (_language == language)
                return;

            _language = language;

            UserSettingsStore.Save(_language);
            ApDungNgonNgu();
        };

        btnXemThoiTiet.Click += async (s, e) =>
        {
            await XemThoiTietAsync();
        };
    }

    // ==========================================
    // 4. TẢI DANH SÁCH + TÌM KIẾM
    // ==========================================

    private void TaiDanhSach()
    {
        try
        {
            DateOnly tuNgay = DateOnly.FromDateTime(
                dtpTuNgay.Value.Date);

            DateOnly denNgay = DateOnly.FromDateTime(
                dtpDenNgay.Value.Date);

            if (tuNgay > denNgay)
            {
                ThongBao(
                    "Từ ngày không được lớn hơn đến ngày.",
                    "From date must not be after To date.");

                return;
            }

            string tuKhoa = txtTim.Text.Trim();

            using var db = _dbFactory();

            IQueryable<ChuyenDi> query =
                db.Set<ChuyenDi>().AsNoTracking();

            query = query.Where(x =>
                x.NgayDi >= tuNgay &&
                x.NgayDi <= denNgay);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenChuyen.Contains(tuKhoa) ||
                    x.DiaDiem.Contains(tuKhoa));
            }

            if (chkChuaHoanThanh.Checked)
            {
                query = query.Where(x => !x.DaHoanThanh);
            }

            var danhSach = query
                .OrderBy(x => x.NgayDi)
                .ThenBy(x => x.MaChuyen)
                .ToList();

            var rows = danhSach.Select(x => new ChuyenDiRow
            {
                MaChuyen = x.MaChuyen,
                TenChuyen = x.TenChuyen,
                DiaDiem = x.DiaDiem,
                NgayDi = x.NgayDi.ToDateTime(
                    TimeOnly.MinValue),
                SoNguoi = x.SoNguoi,
                NganSach = x.NganSach,
                NganSachHienThi =
                    DinhDangTien(x.NganSach),
                PhuongTien = x.PhuongTien,
                PhuongTienHienThi =
                    HienThiPhuongTien(x.PhuongTien),
                DaHoanThanh = x.DaHoanThanh,
                GhiChu = x.GhiChu ?? ""
            }).ToList();

            dgvChuyenDi.DataSource = null;
            dgvChuyenDi.DataSource = rows;
            dgvChuyenDi.ClearSelection();

            _selected = null;

            SetStatus(
                $"Đang hiển thị {rows.Count} chuyến đi.",
                $"Showing {rows.Count} trips.");
        }
        catch (Exception ex)
        {
            BaoLoi(ex,
                "Không thể tải danh sách chuyến đi.",
                "Unable to load trips.");
        }
    }

    private void ChonChuyenDi(ChuyenDiRow row)
    {
        _selected = row;

        txtTenChuyen.Text = row.TenChuyen;
        txtDiaDiem.Text = row.DiaDiem;
        dtpNgayDi.Value = row.NgayDi;
        nudSoNguoi.Value = Math.Clamp(
            row.SoNguoi,
            (int)nudSoNguoi.Minimum,
            (int)nudSoNguoi.Maximum);

        nudNganSach.Value = Math.Clamp(
            row.NganSach,
            nudNganSach.Minimum,
            nudNganSach.Maximum);

        var option = cboPhuongTien.Items
            .OfType<VehicleOption>()
            .FirstOrDefault(x =>
                x.Storage == row.PhuongTien);

        if (option != null)
            cboPhuongTien.SelectedItem = option;

        chkDaHoanThanh.Checked = row.DaHoanThanh;
        txtGhiChu.Text = row.GhiChu;

        // Không giữ kết quả dự báo của chuyến khác.
        _forecast = null;
        _weatherPlace = row.DiaDiem;
        HienThiThoiTiet();
    }

    // ==========================================
    // 5. VALIDATION
    // ==========================================

    private bool DuLieuHopLe(bool themMoi)
    {
        if (string.IsNullOrWhiteSpace(txtTenChuyen.Text))
        {
            ThongBao(
                "Tên chuyến không được để trống.",
                "Trip name is required.");
            txtTenChuyen.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtDiaDiem.Text))
        {
            ThongBao(
                "Địa điểm không được để trống.",
                "Location is required.");
            txtDiaDiem.Focus();
            return false;
        }

        if (themMoi &&
            dtpNgayDi.Value.Date < DateTime.Today)
        {
            ThongBao(
                "Ngày đi không được nhỏ hơn hôm nay.",
                "Trip date cannot be in the past.");
            return false;
        }

        if (nudSoNguoi.Value < 1)
        {
            ThongBao(
                "Số người phải lớn hơn hoặc bằng 1.",
                "Number of people must be at least 1.");
            return false;
        }

        if (nudNganSach.Value < 0)
        {
            ThongBao(
                "Ngân sách không được âm.",
                "Budget cannot be negative.");
            return false;
        }

        if (cboPhuongTien.SelectedItem is not VehicleOption)
        {
            ThongBao(
                "Vui lòng chọn phương tiện.",
                "Please select a transport type.");
            return false;
        }

        return true;
    }

    private ChuyenDi LayDuLieuForm()
    {
        var vehicle =
            (VehicleOption)cboPhuongTien.SelectedItem!;

        return new ChuyenDi
        {
            TenChuyen = txtTenChuyen.Text.Trim(),
            DiaDiem = txtDiaDiem.Text.Trim(),
            NgayDi = DateOnly.FromDateTime(
                dtpNgayDi.Value.Date),
            SoNguoi = (int)nudSoNguoi.Value,
            NganSach = nudNganSach.Value,
            PhuongTien = vehicle.Storage,
            DaHoanThanh = chkDaHoanThanh.Checked,
            GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text)
                ? null
                : txtGhiChu.Text.Trim()
        };
    }

    // ==========================================
    // 6. THÊM CHUYẾN ĐI
    // ==========================================

    private void ThemChuyenDi()
    {
        if (!DuLieuHopLe(themMoi: true))
            return;

        try
        {
            using var db = _dbFactory();

            var chuyen = LayDuLieuForm();

            db.Set<ChuyenDi>().Add(chuyen);
            db.SaveChanges();

            Log.Information(
                "Thêm chuyến MaChuyen={MaChuyen}",
                chuyen.MaChuyen);

            ThongBao(
                "Thêm chuyến đi thành công!",
                "Trip added successfully.");

            XoaTrangNhap();
            TaiDanhSach();
        }
        catch (Exception ex)
        {
            BaoLoi(ex,
                "Không thể thêm chuyến đi.",
                "Unable to add trip.");
        }
    }

    // ==========================================
    // 7. SỬA CHUYẾN ĐI
    // ==========================================

    private void SuaChuyenDi()
    {
        if (_selected == null)
        {
            ThongBao(
                "Vui lòng chọn chuyến đi cần sửa.",
                "Please select a trip to edit.");
            return;
        }

        if (!DuLieuHopLe(themMoi: false))
            return;

        try
        {
            int maChuyen = _selected.MaChuyen;

            using var db = _dbFactory();

            var chuyen = db.Set<ChuyenDi>()
                .FirstOrDefault(x =>
                    x.MaChuyen == maChuyen);

            if (chuyen == null)
            {
                ThongBao(
                    "Chuyến đi không còn tồn tại.",
                    "Trip no longer exists.");
                TaiDanhSach();
                return;
            }

            var input = LayDuLieuForm();

            chuyen.TenChuyen = input.TenChuyen;
            chuyen.DiaDiem = input.DiaDiem;
            chuyen.NgayDi = input.NgayDi;
            chuyen.SoNguoi = input.SoNguoi;
            chuyen.NganSach = input.NganSach;
            chuyen.PhuongTien = input.PhuongTien;
            chuyen.DaHoanThanh = input.DaHoanThanh;
            chuyen.GhiChu = input.GhiChu;

            db.SaveChanges();

            Log.Information(
                "Sửa chuyến MaChuyen={MaChuyen}",
                maChuyen);

            ThongBao(
                "Cập nhật chuyến đi thành công!",
                "Trip updated successfully.");

            XoaTrangNhap();
            TaiDanhSach();
        }
        catch (Exception ex)
        {
            BaoLoi(ex,
                "Không thể sửa chuyến đi.",
                "Unable to update trip.");
        }
    }

    // ==========================================
    // 8. XÓA CHUYẾN ĐI
    // ==========================================

    private void XoaChuyenDi()
    {
        if (_selected == null)
        {
            ThongBao(
                "Vui lòng chọn chuyến đi cần xóa.",
                "Please select a trip to delete.");
            return;
        }

        int maChuyen = _selected.MaChuyen;

        DialogResult confirm = MessageBox.Show(
            T("Bạn có chắc muốn xóa chuyến đi này?",
              "Are you sure you want to delete this trip?"),
            T("Xác nhận xóa", "Confirm deletion"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            using var db = _dbFactory();

            var chuyen = db.Set<ChuyenDi>()
                .FirstOrDefault(x =>
                    x.MaChuyen == maChuyen);

            if (chuyen == null)
            {
                ThongBao(
                    "Chuyến đi không còn tồn tại.",
                    "Trip no longer exists.");
                TaiDanhSach();
                return;
            }

            db.Set<ChuyenDi>().Remove(chuyen);
            db.SaveChanges();

            Log.Information(
                "Xóa chuyến MaChuyen={MaChuyen}",
                maChuyen);

            ThongBao(
                "Xóa chuyến đi thành công!",
                "Trip deleted successfully.");

            XoaTrangNhap();
            TaiDanhSach();
        }
        catch (Exception ex)
        {
            BaoLoi(ex,
                "Không thể xóa chuyến đi.",
                "Unable to delete trip.");
        }
    }

    // ==========================================
    // 9. LÀM MỚI
    // ==========================================

    private void XoaTrangNhap()
    {
        _selected = null;

        txtTenChuyen.Clear();
        txtDiaDiem.Clear();
        txtGhiChu.Clear();

        dtpNgayDi.Value = DateTime.Today.AddDays(6);
        nudSoNguoi.Value = 1;
        nudNganSach.Value = 0;

        cboPhuongTien.SelectedIndex = 0;
        chkDaHoanThanh.Checked = false;

        dgvChuyenDi.ClearSelection();

        _forecast = null;
        _weatherPlace = "";
        HienThiThoiTiet();
    }

    private void LamMoi()
    {
        txtTim.Clear();
        chkChuaHoanThanh.Checked = false;
        dtpTuNgay.Value = DateTime.Today;
        dtpDenNgay.Value = DateTime.Today.AddDays(90);

        XoaTrangNhap();
        TaiDanhSach();
    }

    // ==========================================
    // 10. THÔNG BÁO VÀ XỬ LÝ LỖI
    // ==========================================

    private void ThongBao(string vi, string en)
    {
        MessageBox.Show(
            T(vi, en),
            T("Thông báo", "Notification"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BaoLoi(
        Exception ex, string vi, string en)
    {
        Log.Error(ex, "Lỗi thao tác dữ liệu");

        MessageBox.Show(
            T(vi, en) + "\n" +
            T("Vui lòng kiểm tra dữ liệu và kết nối SQL Server.",
              "Please check your data and SQL Server connection."),
            T("Lỗi ứng dụng", "Application error"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    // ==========================================
    // 11. MÔ TẢ VÀ BIỂU TƯỢNG THỜI TIẾT
    // ==========================================

    private string MoTaThoiTiet(int code)
    {
        return code switch
        {
            0 => T("Trời quang", "Clear sky"),
            >= 1 and <= 3 => T("Có mây", "Cloudy"),
            45 or 48 => T("Sương mù", "Fog"),
            >= 51 and <= 67 => T("Mưa", "Rain"),
            >= 71 and <= 77 => T("Tuyết", "Snow"),
            >= 80 and <= 82 => T("Mưa rào", "Showers"),
            >= 85 and <= 86 => T("Tuyết rào", "Snow showers"),
            >= 95 and <= 99 => T("Dông", "Thunderstorm"),
            _ => T("Không xác định", "Unknown")
        };
    }

    private string BieuTuongThoiTiet(int code)
    {
        return code switch
        {
            0 => "☀️",
            >= 1 and <= 3 => "☁️",
            45 or 48 => "🌫️",
            >= 51 and <= 67 => "🌧️",
            >= 71 and <= 77 => "❄️",
            >= 80 and <= 82 => "🌧️",
            >= 85 and <= 86 => "❄️",
            >= 95 and <= 99 => "⛈️",
            _ => "🌤️"
        };
    }

    private void HienThiThoiTiet()
    {
        pnlWeather.BackColor = Color.White;

        if (_forecast == null)
        {
            lblWeatherDay.Text = T(
                "Chọn chuyến đi và nhấn Xem thời tiết",
                "Select a trip and check the weather");

            lblWeatherIcon.Text = "🌤️";
            lblWeatherDescription.Text = T(
                "Chưa có dự báo", "No forecast yet");

            lblTemperature.Text = "";
            lblRain.Text = "";
            lblAdvice.Text = "";

            pnlAdvice.BackColor = Color.White;
            return;
        }

        var data = _forecast;

        string ngay = data.Ngay.ToDateTime(
            TimeOnly.MinValue).ToString(
                _language == "en" ? "MM/dd/yyyy" : "dd/MM/yyyy",
                _culture);

        lblWeatherDay.Text = T(
            $"Dự báo ngày {ngay} - {_weatherPlace}",
            $"Forecast for {ngay} - {_weatherPlace}");

        lblWeatherIcon.Text =
            BieuTuongThoiTiet(data.WeatherCode);

        lblWeatherDescription.Text =
            MoTaThoiTiet(data.WeatherCode);

        lblTemperature.Text = T(
            $"Nhiệt độ: {data.NhietDoThap:F0}°C - {data.NhietDoCao:F0}°C",
            $"Temperature: {data.NhietDoThap:F0}°C - {data.NhietDoCao:F0}°C");

        lblRain.Text = T(
            $"Khả năng mưa: {data.XacSuatMua}%",
            $"Rain probability: {data.XacSuatMua}%");

        if (data.XacSuatMua >= 60)
        {
            pnlAdvice.BackColor = Color.FromArgb(
                255, 223, 171);

            lblAdvice.ForeColor = Color.DarkOrange;

            lblAdvice.Text = T(
                "⚠ Nên mang áo mưa hoặc cân nhắc dời lịch",
                "⚠ Bring a raincoat or consider rescheduling");
        }
        else
        {
            pnlAdvice.BackColor = Color.FromArgb(
                232, 242, 232);

            lblAdvice.ForeColor = Color.DarkGreen;

            lblAdvice.Text = T(
                "Thời tiết có vẻ thuận lợi cho chuyến đi",
                "Weather looks suitable for the trip");
        }
    }

    // ==========================================
    // 12. GỌI API THỜI TIẾT BẤT ĐỒNG BỘ
    // ==========================================

    private async Task XemThoiTietAsync()
    {
        if (_selected == null)
        {
            ThongBao(
                "Vui lòng chọn một chuyến đi trong danh sách.",
                "Please select a trip from the list.");
            return;
        }

        var chuyen = _selected;

        btnXemThoiTiet.Enabled = false;
        progressWeather.Visible = true;

        _forecast = null;
        _weatherPlace = chuyen.DiaDiem;

        lblWeatherDescription.Text = T(
            "Đang tải...", "Loading...");

        lblWeatherDay.Text = T(
            "Đang tải dự báo thời tiết...",
            "Loading weather forecast...");

        lblWeatherIcon.Text = "⏳";
        lblTemperature.Text = "";
        lblRain.Text = "";
        lblAdvice.Text = "";
        pnlAdvice.BackColor = Color.White;

        using var cts = new CancellationTokenSource();
        _weatherCts = cts;

        try
        {
            DateOnly ngayDi =
                DateOnly.FromDateTime(chuyen.NgayDi);

            var data = await WeatherService.LayDuBaoAsync(
                chuyen.DiaDiem,
                ngayDi,
                cts.Token);

            if (cts.IsCancellationRequested || IsDisposed)
                return;

            if (data == null)
            {
                _forecast = null;
                HienThiThoiTiet();

                lblWeatherDescription.Text = T(
                    "Chưa có dữ liệu dự báo",
                    "Forecast not available");

                SetStatus(
                    "Ngày đi nằm ngoài khoảng dự báo hoặc thiếu dữ liệu.",
                    "Trip date is outside the forecast range or data is missing.");

                return;
            }

            _forecast = data;
            HienThiThoiTiet();

            SetStatus(
                "Đã cập nhật dự báo thời tiết.",
                "Weather forecast updated.");
        }
        catch (OperationCanceledException)
            when (cts.IsCancellationRequested)
        {
            // Form bị đóng trong lúc tải.
        }
        catch (DiaDiemKhongTimThayException ex)
        {
            Log.Warning(ex,
                "Không tìm thấy địa điểm {DiaDiem}",
                chuyen.DiaDiem);

            HienThiThoiTiet();

            ThongBao(
                "Không tìm thấy địa danh.\n" +
                "Hãy thử tên không dấu, ví dụ: Da Lat.",
                "Location not found.\n" +
                "Try an unaccented name, e.g. Da Lat.");
        }
        catch (TaskCanceledException ex)
        {
            Log.Warning(ex, "Open-Meteo phản hồi quá thời gian");

            HienThiThoiTiet();

            ThongBao(
                "Kết nối thời tiết quá thời gian 8 giây.",
                "Weather request timed out after 8 seconds.");
        }
        catch (HttpRequestException ex)
        {
            Log.Warning(ex, "Không kết nối được API thời tiết");

            HienThiThoiTiet();

            ThongBao(
                "Không tải được thời tiết, vui lòng kiểm tra mạng.",
                "Cannot load weather. Please check your connection.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lỗi xử lý dữ liệu thời tiết");

            HienThiThoiTiet();

            ThongBao(
                "Có lỗi khi xử lý dự báo thời tiết.",
                "An error occurred while processing the forecast.");
        }
        finally
        {
            if (ReferenceEquals(_weatherCts, cts))
                _weatherCts = null;

            if (!IsDisposed && !Disposing)
            {
                progressWeather.Visible = false;
                btnXemThoiTiet.Enabled = true;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _weatherCts?.Cancel();
        base.OnFormClosing(e);
    }
}

// ==========================================
// LỚP HIỂN THỊ - KHÔNG PHẢI ENTITY EF CORE
// ==========================================

public sealed class ChuyenDiRow
{
    public int MaChuyen { get; set; }
    public string TenChuyen { get; set; } = "";
    public string DiaDiem { get; set; } = "";
    public DateTime NgayDi { get; set; }

    public int SoNguoi { get; set; }
    public decimal NganSach { get; set; }
    public string NganSachHienThi { get; set; } = "";

    public string PhuongTien { get; set; } = "";
    public string PhuongTienHienThi { get; set; } = "";

    public bool DaHoanThanh { get; set; }
    public string GhiChu { get; set; } = "";
}

public sealed class VehicleOption
{
    public string Storage { get; }
    public string Display { get; }

    public VehicleOption(string storage, string display)
    {
        Storage = storage;
        Display = display;
    }

    public override string ToString() => Display;
}

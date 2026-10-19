
using System.Globalization;
using Cau1_SoChiTieuCaNhan.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cau1_SoChiTieuCaNhan
{
    public class FormChiTieu : Form
    {
        private readonly Func<Cau1_SoChiTieuCaNhanContext> _dbFactory;
        private int? _maCT;
        private bool _dangDatLaiBoLoc = false;

        private readonly CultureInfo _viVN =
            CultureInfo.GetCultureInfo("vi-VN");

        // Nhóm nhập thông tin
        private DateTimePicker dtpNgayChi = new();
        private TextBox txtNoiDung = new();
        private ComboBox cboLoaiChi = new();
        private NumericUpDown nudSoTien = new();
        private TextBox txtGhiChu = new();

        // Nhóm tìm kiếm
        private TextBox txtTimNoiDung = new();
        private ComboBox cboLocLoaiChi = new();
        private DateTimePicker dtpTuNgay = new();
        private DateTimePicker dtpDenNgay = new();

        // Các nút chức năng
        private Button btnThem = new();
        private Button btnSua = new();
        private Button btnXoa = new();
        private Button btnLamMoi = new();
        private Button btnTim = new();

        // Bảng dữ liệu và tổng chi
        private DataGridView dgvChiTieu = new();
        private Label lblTongChi = new();

        public FormChiTieu(
            Func<Cau1_SoChiTieuCaNhanContext> dbFactory)
        {
            _dbFactory = dbFactory;

            KhoiTaoGiaoDien();
            GanSuKien();

            Shown += (s, e) => TaiDanhSach();
        }

        // ==========================================
        // 1. THIẾT KẾ GIAO DIỆN
        // ==========================================

        private void KhoiTaoGiaoDien()
        {
            Text = "Sổ chi tiêu cá nhân";
            Name = "FormChiTieu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 675);
            MinimumSize = new Size(1136, 714);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(248, 249, 252);

            // ===== THÔNG TIN KHOẢN CHI =====

            GroupBox grpThongTin = new()
            {
                Text = "Thông tin khoản chi",
                Location = new Point(12, 12),
                Size = new Size(620, 200)
            };
            Controls.Add(grpThongTin);

            grpThongTin.Controls.Add(
                TaoLabel("Ngày chi", 15, 32, 85));

            dtpNgayChi.Name = "dtpNgayChi";
            dtpNgayChi.Location = new Point(105, 30);
            dtpNgayChi.Size = new Size(200, 28);
            dtpNgayChi.Format = DateTimePickerFormat.Custom;
            dtpNgayChi.CustomFormat = "dd/MM/yyyy";
            dtpNgayChi.Value = DateTime.Today;
            grpThongTin.Controls.Add(dtpNgayChi);

            grpThongTin.Controls.Add(
                TaoLabel("Loại chi", 320, 32, 85));

            cboLoaiChi.Name = "cboLoaiChi";
            cboLoaiChi.Location = new Point(408, 30);
            cboLoaiChi.Size = new Size(192, 28);
            cboLoaiChi.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoaiChi.Items.AddRange(new object[]
            {
                "Ăn uống",
                "Đi lại",
                "Học tập",
                "Giải trí",
                "Khác"
            });
            cboLoaiChi.SelectedIndex = 0;
            grpThongTin.Controls.Add(cboLoaiChi);

            grpThongTin.Controls.Add(
                TaoLabel("Nội dung", 15, 72, 85));

            txtNoiDung.Name = "txtNoiDung";
            txtNoiDung.Location = new Point(105, 70);
            txtNoiDung.Size = new Size(200, 27);
            txtNoiDung.MaxLength = 100;
            txtNoiDung.PlaceholderText = "Nhập nội dung chi...";
            grpThongTin.Controls.Add(txtNoiDung);

            grpThongTin.Controls.Add(
                TaoLabel("Số tiền", 320, 72, 85));

            nudSoTien.Name = "nudSoTien";
            nudSoTien.Location = new Point(408, 70);
            nudSoTien.Size = new Size(192, 27);
            nudSoTien.Minimum = 0;
            nudSoTien.Maximum = 999999999999999999m;
            nudSoTien.Increment = 1000;
            nudSoTien.DecimalPlaces = 0;
            nudSoTien.ThousandsSeparator = true;
            grpThongTin.Controls.Add(nudSoTien);

            grpThongTin.Controls.Add(
                TaoLabel("Ghi chú", 15, 112, 85));

            txtGhiChu.Name = "txtGhiChu";
            txtGhiChu.Location = new Point(105, 110);
            txtGhiChu.Size = new Size(495, 70);
            txtGhiChu.Multiline = true;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;
            txtGhiChu.MaxLength = 200;
            grpThongTin.Controls.Add(txtGhiChu);

            // ===== TÌM KIẾM =====

            GroupBox grpTimKiem = new()
            {
                Text = "Tìm kiếm / Lọc",
                Location = new Point(645, 12),
                Size = new Size(463, 200)
            };
            Controls.Add(grpTimKiem);

            grpTimKiem.Controls.Add(
                TaoLabel("Nội dung", 15, 32, 85));

            txtTimNoiDung.Name = "txtTimNoiDung";
            txtTimNoiDung.Location = new Point(105, 30);
            txtTimNoiDung.Size = new Size(335, 27);
            txtTimNoiDung.PlaceholderText =
                "Nhập nội dung cần tìm...";
            grpTimKiem.Controls.Add(txtTimNoiDung);

            grpTimKiem.Controls.Add(
                TaoLabel("Loại chi", 15, 70, 85));

            cboLocLoaiChi.Name = "cboLocLoaiChi";
            cboLocLoaiChi.Location = new Point(105, 68);
            cboLocLoaiChi.Size = new Size(335, 28);
            cboLocLoaiChi.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboLocLoaiChi.Items.AddRange(new object[]
            {
                "Tất cả",
                "Ăn uống",
                "Đi lại",
                "Học tập",
                "Giải trí",
                "Khác"
            });

            cboLocLoaiChi.SelectedIndex = 0;
            grpTimKiem.Controls.Add(cboLocLoaiChi);

            grpTimKiem.Controls.Add(
                TaoLabel("Từ ngày", 15, 110, 85));

            dtpTuNgay.Name = "dtpTuNgay";
            dtpTuNgay.Location = new Point(105, 108);
            dtpTuNgay.Size = new Size(127, 28);
            dtpTuNgay.Format = DateTimePickerFormat.Custom;
            dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            dtpTuNgay.Value = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);
            grpTimKiem.Controls.Add(dtpTuNgay);

            grpTimKiem.Controls.Add(
                TaoLabel("Đến ngày", 238, 110, 80));

            dtpDenNgay.Name = "dtpDenNgay";
            dtpDenNgay.Location = new Point(315, 108);
            dtpDenNgay.Size = new Size(125, 28);
            dtpDenNgay.Format = DateTimePickerFormat.Custom;
            dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            dtpDenNgay.Value = DateTime.Today;
            grpTimKiem.Controls.Add(dtpDenNgay);

            btnTim = TaoNut("Tìm", 340, 152, 100);
            btnTim.Name = "btnTim";
            grpTimKiem.Controls.Add(btnTim);

            // ===== NÚT THAO TÁC =====

            btnThem = TaoNut("Thêm", 105, 223, 105);
            btnThem.Name = "btnThem";
            Controls.Add(btnThem);

            btnSua = TaoNut("Sửa", 220, 223, 105);
            btnSua.Name = "btnSua";
            Controls.Add(btnSua);

            btnXoa = TaoNut("Xóa", 335, 223, 105);
            btnXoa.Name = "btnXoa";
            Controls.Add(btnXoa);

            btnLamMoi = TaoNut("Làm mới", 450, 223, 110);
            btnLamMoi.Name = "btnLamMoi";
            Controls.Add(btnLamMoi);

            // ===== DATAGRIDVIEW =====

            dgvChiTieu.Name = "dgvChiTieu";
            dgvChiTieu.Location = new Point(12, 270);
            dgvChiTieu.Size = new Size(1096, 340);

            dgvChiTieu.ReadOnly = true;
            dgvChiTieu.AllowUserToAddRows = false;
            dgvChiTieu.AllowUserToDeleteRows = false;
            dgvChiTieu.AllowUserToResizeRows = false;
            dgvChiTieu.MultiSelect = false;

            dgvChiTieu.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvChiTieu.AutoGenerateColumns = false;
            dgvChiTieu.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvChiTieu.RowHeadersVisible = false;
            dgvChiTieu.BackgroundColor = Color.White;
            dgvChiTieu.BorderStyle = BorderStyle.FixedSingle;
            dgvChiTieu.RowTemplate.Height = 29;

            dgvChiTieu.EnableHeadersVisualStyles = false;
            dgvChiTieu.ColumnHeadersHeight = 35;

            dgvChiTieu.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(230, 236, 245);

            dgvChiTieu.Columns.Add(
                TaoCot("MaCt", "Mã", 45));

            dgvChiTieu.Columns.Add(
                TaoCot("NgayChi", "Ngày chi", 90));

            dgvChiTieu.Columns.Add(
                TaoCot("NoiDung", "Nội dung", 230));

            dgvChiTieu.Columns.Add(
                TaoCot("LoaiChi", "Loại chi", 110));

            dgvChiTieu.Columns.Add(
                TaoCot("SoTien", "Số tiền", 120));

            dgvChiTieu.Columns.Add(
                TaoCot("GhiChu", "Ghi chú", 250));

            dgvChiTieu.Columns["MaCt"]!
                .DefaultCellStyle.Format = "000";

            dgvChiTieu.Columns["NgayChi"]!
                .DefaultCellStyle.Format = "dd/MM/yyyy";

            dgvChiTieu.Columns["SoTien"]!
                .DefaultCellStyle.Format = "N0";

            dgvChiTieu.Columns["SoTien"]!
                .DefaultCellStyle.FormatProvider = _viVN;

            dgvChiTieu.Columns["SoTien"]!
                .DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;

            Controls.Add(dgvChiTieu);

            // ===== TỔNG CHI =====

            lblTongChi.Name = "lblTongChi";
            lblTongChi.Location = new Point(710, 620);
            lblTongChi.Size = new Size(398, 35);
            lblTongChi.TextAlign =
                ContentAlignment.MiddleRight;
            lblTongChi.Font = new Font(
                "Segoe UI", 12, FontStyle.Bold);
            lblTongChi.ForeColor = Color.Firebrick;
            lblTongChi.Text = "Tổng chi: 0 đ";
            Controls.Add(lblTongChi);
        }

        private Label TaoLabel(
            string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Button TaoNut(
            string text, int x, int y, int width)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 34),
                BackColor = Color.White,
                FlatStyle = FlatStyle.System,
                Cursor = Cursors.Hand
            };
        }

        private DataGridViewTextBoxColumn TaoCot(
            string tenThuocTinh,
            string tieuDe,
            float doRong)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = tenThuocTinh,
                DataPropertyName = tenThuocTinh,
                HeaderText = tieuDe,
                FillWeight = doRong,
                SortMode =
                    DataGridViewColumnSortMode.Automatic
            };
        }

        // ==========================================
        // 2. GẮN SỰ KIỆN
        // ==========================================

        private void GanSuKien()
        {
            btnThem.Click += (s, e) => ThemChiTieu();
            btnSua.Click += (s, e) => SuaChiTieu();
            btnXoa.Click += (s, e) => XoaChiTieu();

            btnLamMoi.Click += (s, e) => LamMoi();

            btnTim.Click += (s, e) =>
                TaiDanhSach(hienLoiNgay: true);

            dgvChiTieu.CellClick +=
                DgvChiTieu_CellClick;

            // Tự động lọc và cập nhật tổng chi.
            txtTimNoiDung.TextChanged +=
                (s, e) => LocTuDong();

            cboLocLoaiChi.SelectedIndexChanged +=
                (s, e) => LocTuDong();

            dtpTuNgay.ValueChanged +=
                (s, e) => LocTuDong();

            dtpDenNgay.ValueChanged +=
                (s, e) => LocTuDong();
        }

        private void LocTuDong()
        {
            if (!_dangDatLaiBoLoc)
                TaiDanhSach();
        }

        // ==========================================
        // 3. HIỂN THỊ DỮ LIỆU VÀ TÍNH TỔNG
        // ==========================================

        private void TaiDanhSach(bool hienLoiNgay = false)
        {
            try
            {
                DateOnly tuNgay =
                    DateOnly.FromDateTime(dtpTuNgay.Value.Date);

                DateOnly denNgay =
                    DateOnly.FromDateTime(dtpDenNgay.Value.Date);

                if (tuNgay > denNgay)
                {
                    dgvChiTieu.DataSource = null;
                    lblTongChi.Text = "Tổng chi: 0 đ";

                    if (hienLoiNgay)
                    {
                        MessageBox.Show(
                            "Từ ngày không được lớn hơn đến ngày.",
                            "Ngày không hợp lệ",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }

                    return;
                }

                string noiDung =
                    txtTimNoiDung.Text.Trim();

                string loaiChi =
                    cboLocLoaiChi.SelectedItem?
                        .ToString() ?? "Tất cả";

                using var db = _dbFactory();

                IQueryable<ChiTieu> query =
                    db.Set<ChiTieu>().AsNoTracking();

                // Lọc theo khoảng ngày.
                query = query.Where(x =>
                    x.NgayChi >= tuNgay &&
                    x.NgayChi <= denNgay);

                // Tìm theo nội dung.
                if (!string.IsNullOrWhiteSpace(noiDung))
                {
                    query = query.Where(x =>
                        x.NoiDung.Contains(noiDung));
                }

                // Lọc theo loại chi.
                if (loaiChi != "Tất cả")
                {
                    query = query.Where(x =>
                        x.LoaiChi == loaiChi);
                }

                var danhSach = query
                    .OrderByDescending(x => x.NgayChi)
                    .ThenByDescending(x => x.MaCt)
                    .ToList();

                // Chuyển sang dữ liệu hiển thị.
                var duLieuHienThi = danhSach
                    .Select(x => new ChiTieuRow
                    {
                        MaCt = x.MaCt,
                        NgayChi = x.NgayChi.ToDateTime(
                            TimeOnly.MinValue),
                        NoiDung = x.NoiDung,
                        LoaiChi = x.LoaiChi,
                        SoTien = x.SoTien,
                        GhiChu = x.GhiChu ?? ""
                    })
                    .ToList();

                dgvChiTieu.DataSource = null;
                dgvChiTieu.DataSource = duLieuHienThi;
                dgvChiTieu.ClearSelection();

                // Tổng chỉ tính các dòng đang hiển thị.
                decimal tongChi =
                    danhSach.Sum(x => x.SoTien);

                lblTongChi.Text =
                    $"Tổng chi: {tongChi.ToString("N0", _viVN)} đ";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi truy vấn danh sách chi tiêu");

                MessageBox.Show(
                    "Không thể tải danh sách chi tiêu.\n" +
                    "Vui lòng kiểm tra kết nối SQL Server.",
                    "Lỗi dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 4. CHỌN DÒNG TRONG DATAGRIDVIEW
        // ==========================================

        private void DgvChiTieu_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var dong =
                dgvChiTieu.Rows[e.RowIndex].DataBoundItem
                as ChiTieuRow;

            if (dong == null)
                return;

            _maCT = dong.MaCt;

            dtpNgayChi.Value = dong.NgayChi;
            txtNoiDung.Text = dong.NoiDung;

            cboLoaiChi.SelectedItem = dong.LoaiChi;

            if (dong.SoTien >= nudSoTien.Minimum &&
                dong.SoTien <= nudSoTien.Maximum)
            {
                nudSoTien.Value = dong.SoTien;
            }

            txtGhiChu.Text = dong.GhiChu;
        }

        // ==========================================
        // 5. KIỂM TRA DỮ LIỆU ĐẦU VÀO
        // ==========================================

        private bool DuLieuHopLe()
        {
            if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show(
                    "Nội dung chi không được để trống.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNoiDung.Focus();
                return false;
            }

            if (nudSoTien.Value <= 0)
            {
                MessageBox.Show(
                    "Số tiền phải lớn hơn 0.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                nudSoTien.Focus();
                return false;
            }

            if (dtpNgayChi.Value.Date > DateTime.Today)
            {
                MessageBox.Show(
                    "Ngày chi không được lớn hơn hôm nay.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                dtpNgayChi.Focus();
                return false;
            }

            if (cboLoaiChi.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn loại chi.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ==========================================
        // 6. THÊM KHOẢN CHI
        // ==========================================

        private void ThemChiTieu()
        {
            if (!DuLieuHopLe())
                return;

            try
            {
                using var db = _dbFactory();

                var chiTieu = new ChiTieu
                {
                    NgayChi = DateOnly.FromDateTime(
                        dtpNgayChi.Value.Date),

                    NoiDung = txtNoiDung.Text.Trim(),

                    LoaiChi = cboLoaiChi.SelectedItem!
                        .ToString()!,

                    SoTien = nudSoTien.Value,

                    GhiChu =
                        string.IsNullOrWhiteSpace(txtGhiChu.Text)
                        ? null
                        : txtGhiChu.Text.Trim()
                };

                db.Set<ChiTieu>().Add(chiTieu);
                db.SaveChanges();

                Log.Information(
                    "Thêm khoản chi MaCT={MaCT}, SoTien={SoTien}",
                    chiTieu.MaCt,
                    chiTieu.SoTien);

                MessageBox.Show(
                    "Thêm khoản chi thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                XoaTrangNhapLieu();
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi thêm khoản chi");

                MessageBox.Show(
                    "Không thể thêm khoản chi.\n" +
                    "Vui lòng kiểm tra dữ liệu và kết nối CSDL.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 7. SỬA KHOẢN CHI
        // ==========================================

        private void SuaChiTieu()
        {
            if (_maCT == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn một khoản chi cần sửa.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!DuLieuHopLe())
                return;

            try
            {
                using var db = _dbFactory();

                int maCT = _maCT.Value;

                var chiTieu = db.Set<ChiTieu>()
                    .FirstOrDefault(x => x.MaCt == maCT);

                if (chiTieu == null)
                {
                    MessageBox.Show(
                        "Khoản chi không còn tồn tại.",
                        "Thông báo");
                    TaiDanhSach();
                    return;
                }

                chiTieu.NgayChi =
                    DateOnly.FromDateTime(
                        dtpNgayChi.Value.Date);

                chiTieu.NoiDung =
                    txtNoiDung.Text.Trim();

                chiTieu.LoaiChi =
                    cboLoaiChi.SelectedItem!.ToString()!;

                chiTieu.SoTien = nudSoTien.Value;

                chiTieu.GhiChu =
                    string.IsNullOrWhiteSpace(txtGhiChu.Text)
                    ? null
                    : txtGhiChu.Text.Trim();

                db.SaveChanges();

                Log.Information(
                    "Sửa khoản chi MaCT={MaCT}",
                    maCT);

                MessageBox.Show(
                    "Cập nhật khoản chi thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                XoaTrangNhapLieu();
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi sửa khoản chi MaCT={MaCT}", _maCT);

                MessageBox.Show(
                    "Không thể cập nhật khoản chi.\n" +
                    "Vui lòng kiểm tra kết nối CSDL.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 8. XÓA KHOẢN CHI
        // ==========================================

        private void XoaChiTieu()
        {
            if (_maCT == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn khoản chi cần xóa.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult xacNhan = MessageBox.Show(
                $"Bạn có chắc muốn xóa khoản chi mã {_maCT}?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (xacNhan != DialogResult.Yes)
                return;

            try
            {
                using var db = _dbFactory();

                int maCT = _maCT.Value;

                var chiTieu = db.Set<ChiTieu>()
                    .FirstOrDefault(x => x.MaCt == maCT);

                if (chiTieu == null)
                {
                    MessageBox.Show(
                        "Khoản chi không còn tồn tại.",
                        "Thông báo");
                    TaiDanhSach();
                    return;
                }

                db.Set<ChiTieu>().Remove(chiTieu);
                db.SaveChanges();

                Log.Information(
                    "Xóa khoản chi MaCT={MaCT}",
                    maCT);

                MessageBox.Show(
                    "Xóa khoản chi thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                XoaTrangNhapLieu();
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi xóa khoản chi MaCT={MaCT}", _maCT);

                MessageBox.Show(
                    "Không thể xóa khoản chi.\n" +
                    "Vui lòng kiểm tra kết nối CSDL.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 9. LÀM MỚI
        // ==========================================

        private void XoaTrangNhapLieu()
        {
            _maCT = null;

            dtpNgayChi.Value = DateTime.Today;
            txtNoiDung.Clear();

            cboLoaiChi.SelectedIndex = 0;
            nudSoTien.Value = 0;
            txtGhiChu.Clear();

            dgvChiTieu.ClearSelection();
        }

        private void LamMoi()
        {
            XoaTrangNhapLieu();

            _dangDatLaiBoLoc = true;

            try
            {
                txtTimNoiDung.Clear();
                cboLocLoaiChi.SelectedIndex = 0;

                dtpTuNgay.Value = new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

                dtpDenNgay.Value = DateTime.Today;
            }
            finally
            {
                _dangDatLaiBoLoc = false;
            }

            TaiDanhSach();
        }
    }

    // ==========================================
    // MODEL HIỂN THỊ TRÊN DATAGRIDVIEW
    // KHÔNG PHẢI ENTITY DATABASE
    // ==========================================

    public sealed class ChiTieuRow
    {
        public int MaCt { get; set; }

        public DateTime NgayChi { get; set; }

        public string NoiDung { get; set; }
            = string.Empty;

        public string LoaiChi { get; set; }
            = string.Empty;

        public decimal SoTien { get; set; }

        public string GhiChu { get; set; }
            = string.Empty;
    }
}

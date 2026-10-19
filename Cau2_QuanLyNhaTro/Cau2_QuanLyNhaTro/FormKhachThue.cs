
using System.Data;
using System.Text.RegularExpressions;
using Cau2_QuanLyNhaTro.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cau2_QuanLyNhaTro;

public class FormKhachThue : Form
{
    private readonly Func<Cau2_QuanLyNhaTroContext> _dbFactory;
    private int? _maKhach;
    private bool _khoaSuKienDiaChi;
    private CancellationTokenSource? _wardRequest;

    private TextBox txtHoTen = new();
    private RadioButton radNam = new();
    private RadioButton radNu = new();
    private DateTimePicker dtpNgaySinh = new();
    private TextBox txtSDT = new();
    private ComboBox cboPhong = new();
    private ComboBox cboTinh = new();
    private ComboBox cboPhuongXa = new();
    private CheckBox chkTamTru = new();

    private TextBox txtTim = new();
    private ComboBox cboLocPhong = new();
    private ComboBox cboLocTinh = new();

    private Button btnThem = new();
    private Button btnSua = new();
    private Button btnXoa = new();
    private Button btnLamMoi = new();
    private Button btnTim = new();
    private Button btnBoLoc = new();
    private Button btnGayLoi = new();

    private DataGridView dgvKhach = new();
    private Label lblTrangThai = new();

    public FormKhachThue(
        Func<Cau2_QuanLyNhaTroContext> dbFactory)
    {
        _dbFactory = dbFactory;

        KhoiTaoGiaoDien();
        GanSuKien();

        Shown += async (s, e) =>
        {
            TaiDuLieuBanDau();
            await TaiTinhThanhAsync();
        };
    }

    // =========================================
    // 1. GIAO DIỆN
    // =========================================

    private Label TaoLabel(string text, int x, int y, int w = 105)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 27),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private Button TaoNut(
        string text, int x, int y, int w = 80)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 33),
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.System
        };
    }

    private void KhoiTaoGiaoDien()
    {
        Text = "Quản lý nhà trọ - Khách thuê";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1190, 670);
        MinimumSize = new Size(1206, 709);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9);
        BackColor = Color.FromArgb(248, 249, 252);
        KeyPreview = true;

        GroupBox grpNhap = new()
        {
            Text = "Thông tin khách thuê",
            Location = new Point(12, 12),
            Size = new Size(380, 470)
        };
        Controls.Add(grpNhap);

        grpNhap.Controls.Add(TaoLabel("Họ tên", 15, 35));
        txtHoTen.Location = new Point(125, 35);
        txtHoTen.Size = new Size(235, 27);
        txtHoTen.MaxLength = 100;
        grpNhap.Controls.Add(txtHoTen);

        GroupBox grpGioiTinh = new()
        {
            Text = "Giới tính",
            Location = new Point(15, 70),
            Size = new Size(345, 48)
        };

        radNam.Text = "Nam";
        radNam.Location = new Point(105, 18);
        radNam.Size = new Size(75, 25);
        radNam.Checked = true;

        radNu.Text = "Nữ";
        radNu.Location = new Point(205, 18);
        radNu.Size = new Size(75, 25);

        grpGioiTinh.Controls.Add(radNam);
        grpGioiTinh.Controls.Add(radNu);
        grpNhap.Controls.Add(grpGioiTinh);

        grpNhap.Controls.Add(TaoLabel("Ngày sinh", 15, 130));
        dtpNgaySinh.Location = new Point(125, 130);
        dtpNgaySinh.Size = new Size(235, 27);
        dtpNgaySinh.Format = DateTimePickerFormat.Custom;
        dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
        dtpNgaySinh.MaxDate = DateTime.Today;
        dtpNgaySinh.Value = new DateTime(2004, 1, 1);
        grpNhap.Controls.Add(dtpNgaySinh);

        grpNhap.Controls.Add(TaoLabel("Số điện thoại", 15, 170));
        txtSDT.Location = new Point(125, 170);
        txtSDT.Size = new Size(235, 27);
        txtSDT.MaxLength = 15;
        grpNhap.Controls.Add(txtSDT);

        grpNhap.Controls.Add(TaoLabel("Phòng", 15, 210));
        cboPhong.Location = new Point(125, 210);
        cboPhong.Size = new Size(235, 27);
        cboPhong.DropDownStyle =
            ComboBoxStyle.DropDownList;
        grpNhap.Controls.Add(cboPhong);

        grpNhap.Controls.Add(TaoLabel("Tỉnh/Thành", 15, 250));
        cboTinh.Location = new Point(125, 250);
        cboTinh.Size = new Size(235, 27);
        cboTinh.DropDownStyle =
            ComboBoxStyle.DropDown;
        cboTinh.AutoCompleteMode =
            AutoCompleteMode.SuggestAppend;
        cboTinh.AutoCompleteSource =
            AutoCompleteSource.ListItems;
        grpNhap.Controls.Add(cboTinh);

        grpNhap.Controls.Add(TaoLabel("Phường/Xã", 15, 290));
        cboPhuongXa.Location = new Point(125, 290);
        cboPhuongXa.Size = new Size(235, 27);
        cboPhuongXa.DropDownStyle =
            ComboBoxStyle.DropDown;
        cboPhuongXa.AutoCompleteMode =
            AutoCompleteMode.SuggestAppend;
        cboPhuongXa.AutoCompleteSource =
            AutoCompleteSource.ListItems;
        grpNhap.Controls.Add(cboPhuongXa);

        chkTamTru.Text = "Đã đăng ký tạm trú";
        chkTamTru.Location = new Point(20, 335);
        chkTamTru.AutoSize = true;
        grpNhap.Controls.Add(chkTamTru);

        btnThem = TaoNut("Thêm", 12, 380, 82);
        btnSua = TaoNut("Sửa", 100, 380, 82);
        btnXoa = TaoNut("Xóa", 188, 380, 82);
        btnLamMoi = TaoNut("Làm mới", 276, 380, 86);

        grpNhap.Controls.AddRange(new Control[]
        {
            btnThem, btnSua, btnXoa, btnLamMoi
        });

        // Nút thử lỗi ẩn, Ctrl + Shift + E để hiện.
        btnGayLoi = TaoNut("Gây lỗi thử", 225, 425, 137);
        btnGayLoi.Visible = false;
        grpNhap.Controls.Add(btnGayLoi);

        // ===== TÌM KIẾM =====

        GroupBox grpTim = new()
        {
            Text = "Tìm kiếm và lọc",
            Location = new Point(405, 12),
            Size = new Size(773, 110)
        };
        Controls.Add(grpTim);

        grpTim.Controls.Add(TaoLabel("Tên / SĐT", 10, 24, 80));

        txtTim.Location = new Point(92, 24);
        txtTim.Size = new Size(195, 27);
        txtTim.PlaceholderText = "Họ tên hoặc số điện thoại";
        grpTim.Controls.Add(txtTim);

        grpTim.Controls.Add(TaoLabel("Phòng", 300, 24, 50));

        cboLocPhong.Location = new Point(350, 24);
        cboLocPhong.Size = new Size(145, 27);
        cboLocPhong.DropDownStyle =
            ComboBoxStyle.DropDownList;
        grpTim.Controls.Add(cboLocPhong);

        grpTim.Controls.Add(TaoLabel("Tỉnh", 505, 24, 45));

        cboLocTinh.Location = new Point(550, 24);
        cboLocTinh.Size = new Size(210, 27);
        cboLocTinh.DropDownStyle =
            ComboBoxStyle.DropDownList;
        grpTim.Controls.Add(cboLocTinh);

        btnTim = TaoNut("Tìm", 550, 66, 95);
        btnBoLoc = TaoNut("Bỏ lọc", 655, 66, 105);
        grpTim.Controls.Add(btnTim);
        grpTim.Controls.Add(btnBoLoc);

        // ===== DATAGRIDVIEW =====

        dgvKhach.Location = new Point(405, 135);
        dgvKhach.Size = new Size(773, 475);
        dgvKhach.BackgroundColor = Color.White;
        dgvKhach.ReadOnly = true;
        dgvKhach.MultiSelect = false;
        dgvKhach.AllowUserToAddRows = false;
        dgvKhach.AllowUserToDeleteRows = false;
        dgvKhach.AllowUserToResizeRows = false;
        dgvKhach.RowHeadersVisible = false;
        dgvKhach.AutoGenerateColumns = false;
        dgvKhach.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;
        dgvKhach.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;
        dgvKhach.ColumnHeadersHeight = 35;

        dgvKhach.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "HoTen",
            HeaderText = "Họ tên",
            FillWeight = 135
        });

        dgvKhach.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "SoDienThoai",
            HeaderText = "SĐT",
            FillWeight = 100
        });

        dgvKhach.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TenPhong",
            HeaderText = "Phòng",
            FillWeight = 65
        });

        dgvKhach.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "GiaThue",
            HeaderText = "Giá thuê",
            FillWeight = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N0",
                Alignment =
                    DataGridViewContentAlignment.MiddleRight
            }
        });

        dgvKhach.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "QueQuan",
            HeaderText = "Quê quán",
            FillWeight = 200
        });

        dgvKhach.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = "DaDangKyTamTru",
            HeaderText = "Tạm trú",
            FillWeight = 75
        });

        Controls.Add(dgvKhach);

        lblTrangThai.Location = new Point(12, 626);
        lblTrangThai.Size = new Size(1166, 30);
        lblTrangThai.Text = "Đang khởi tạo ứng dụng...";
        lblTrangThai.ForeColor = Color.DarkSlateBlue;
        Controls.Add(lblTrangThai);
    }

    // =========================================
    // 2. CÁC SỰ KIỆN
    // =========================================

    private void GanSuKien()
    {
        btnThem.Click += (s, e) => ThemKhach();
        btnSua.Click += (s, e) => SuaKhach();
        btnXoa.Click += (s, e) => XoaKhach();
        btnLamMoi.Click += (s, e) => LamMoi();

        btnTim.Click += (s, e) => TaiDanhSach();
        btnBoLoc.Click += (s, e) =>
        {
            txtTim.Clear();

            if (cboLocPhong.Items.Count > 0)
                cboLocPhong.SelectedIndex = 0;

            if (cboLocTinh.Items.Count > 0)
                cboLocTinh.SelectedIndex = 0;

            TaiDanhSach();
        };

        dgvKhach.CellClick += async (s, e) =>
        {
            if (e.RowIndex < 0) return;

            if (dgvKhach.Rows[e.RowIndex].DataBoundItem
                is KhachRow row)
            {
                await ChonKhachAsync(row);
            }
        };

        cboTinh.SelectedIndexChanged += async (s, e) =>
        {
            if (_khoaSuKienDiaChi) return;

            if (cboTinh.SelectedItem is DiaPhuong tinh)
                await TaiPhuongXaAsync(tinh.Code);
            else
                ChoNhapPhuongXa();
        };

        btnGayLoi.Click += (s, e) =>
        {
            throw new InvalidOperationException(
                "Đây là lỗi thử nghiệm Task 7.");
        };

        KeyDown += (s, e) =>
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.E)
            {
                btnGayLoi.Visible = !btnGayLoi.Visible;
                e.SuppressKeyPress = true;
            }
        };
    }

    // =========================================
    // 3. NẠP DỮ LIỆU SQL SERVER
    // =========================================

    private void TaiDuLieuBanDau()
    {
        TaiPhong();
        TaiBoLocTinh();
        TaiDanhSach();
        XoaTrangNhap();
    }

    private void TaiPhong()
    {
        try
        {
            using var db = _dbFactory();

            var phong = db.Set<PhongTro>()
                .AsNoTracking()
                .OrderBy(x => x.TenPhong)
                .Select(x => new PhongItem
                {
                    MaPhong = x.MaPhong,
                    TenPhong = x.TenPhong,
                    GiaThue = x.GiaThue,
                    TinhTrang = x.TinhTrang
                })
                .ToList();

            // Cho phép nhiều người ở cùng phòng.
            // Đề chưa cung cấp sức chứa tối đa.
            cboPhong.DataSource = null;
            cboPhong.DisplayMember = "HienThi";
            cboPhong.ValueMember = "MaPhong";
            cboPhong.DataSource = phong;
            cboPhong.SelectedIndex = -1;

            var boLoc = new List<PhongLocItem>
            {
                new() { MaPhong = null, Ten = "Tất cả" }
            };

            boLoc.AddRange(phong.Select(x => new PhongLocItem
            {
                MaPhong = x.MaPhong,
                Ten = x.TenPhong
            }));

            cboLocPhong.DataSource = null;
            cboLocPhong.DisplayMember = "Ten";
            cboLocPhong.ValueMember = "MaPhong";
            cboLocPhong.DataSource = boLoc;
            cboLocPhong.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "tải danh sách phòng");
        }
    }

    private void TaiBoLocTinh()
    {
        try
        {
            using var db = _dbFactory();

            var tenTinh = db.Set<KhachThue>()
                .AsNoTracking()
                .Select(x => x.TenTinh)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            tenTinh.Insert(0, "Tất cả");

            cboLocTinh.DataSource = null;
            cboLocTinh.DataSource = tenTinh;
            cboLocTinh.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "tải bộ lọc tỉnh");
        }
    }

    private void TaiDanhSach()
    {
        try
        {
            using var db = _dbFactory();

            var query =
                from k in db.Set<KhachThue>().AsNoTracking()
                join p in db.Set<PhongTro>().AsNoTracking()
                    on k.MaPhong equals p.MaPhong
                select new { k, p };

            string tuKhoa = txtTim.Text.Trim();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.k.HoTen.Contains(tuKhoa) ||
                    x.k.SoDienThoai.Contains(tuKhoa));
            }

            if (cboLocPhong.SelectedItem is PhongLocItem locPhong
                && locPhong.MaPhong.HasValue)
            {
                int maPhong = locPhong.MaPhong.Value;
                query = query.Where(x =>
                    x.k.MaPhong == maPhong);
            }

            string locTinh =
                cboLocTinh.SelectedItem?.ToString()
                ?? "Tất cả";

            if (locTinh != "Tất cả")
            {
                query = query.Where(x =>
                    x.k.TenTinh == locTinh);
            }

            var duLieu = query
                .OrderBy(x => x.k.MaKhach)
                .Select(x => new
                {
                    x.k.MaKhach,
                    x.k.HoTen,
                    x.k.GioiTinh,
                    x.k.NgaySinh,
                    x.k.SoDienThoai,
                    x.k.MaTinh,
                    x.k.TenTinh,
                    x.k.MaPhuongXa,
                    x.k.TenPhuongXa,
                    x.k.DaDangKyTamTru,
                    x.k.MaPhong,
                    x.p.TenPhong,
                    x.p.GiaThue
                })
                .ToList();

            var rows = duLieu.Select(x => new KhachRow
            {
                MaKhach = x.MaKhach,
                HoTen = x.HoTen,
                GioiTinh = x.GioiTinh,
                NgaySinh = x.NgaySinh.ToDateTime(
                    TimeOnly.MinValue),
                SoDienThoai = x.SoDienThoai,
                MaTinh = x.MaTinh,
                TenTinh = x.TenTinh,
                MaPhuongXa = x.MaPhuongXa,
                TenPhuongXa = x.TenPhuongXa,
                DaDangKyTamTru = x.DaDangKyTamTru,
                MaPhong = x.MaPhong,
                TenPhong = x.TenPhong,
                GiaThue = x.GiaThue,
                QueQuan = $"{x.TenPhuongXa} - {x.TenTinh}"
            }).ToList();

            dgvKhach.DataSource = null;
            dgvKhach.DataSource = rows;
            dgvKhach.ClearSelection();

            _maKhach = null;

            lblTrangThai.Text =
                $"Đang hiển thị {rows.Count} khách thuê.";
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "tải danh sách khách");
        }
    }

    // =========================================
    // 4. GỌI API TỈNH / PHƯỜNG XÃ
    // =========================================

    private async Task TaiTinhThanhAsync()
    {
        _khoaSuKienDiaChi = true;

        cboTinh.DataSource = null;
        cboTinh.Items.Clear();
        cboTinh.Items.Add("Đang tải...");
        cboTinh.SelectedIndex = 0;
        cboTinh.Enabled = false;

        lblTrangThai.Text = "Đang tải tỉnh/thành từ API...";

        try
        {
            var ds = await DiaChiApiService.LayTinhThanhAsync();

            cboTinh.DataSource = null;
            cboTinh.Items.Clear();
            cboTinh.DisplayMember = "Name";
            cboTinh.ValueMember = "Code";
            cboTinh.DataSource = ds;
            cboTinh.SelectedIndex = -1;
            cboTinh.Text = "";
            cboTinh.Enabled = true;

            lblTrangThai.Text =
                $"Đã tải {ds.Count} tỉnh/thành từ API.";

            Log.Information(
                "Tải thành công {Count} tỉnh/thành",
                ds.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Không tải được API tỉnh/thành");

            cboTinh.DataSource = null;
            cboTinh.Items.Clear();
            cboTinh.Text = "";
            cboTinh.Enabled = true;

            lblTrangThai.Text =
                "Mất kết nối API - có thể nhập địa chỉ thủ công.";

            MessageBox.Show(
                "Không tải được danh sách địa chỉ, " +
                "vui lòng kiểm tra mạng.\n" +
                "Bạn vẫn có thể nhập tỉnh/thành và phường/xã bằng tay.",
                "Cảnh báo mạng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            _khoaSuKienDiaChi = false;
        }
    }

    private void ChoNhapPhuongXa(string tenCu = "")
    {
        _wardRequest?.Cancel();

        cboPhuongXa.DataSource = null;
        cboPhuongXa.Items.Clear();
        cboPhuongXa.Enabled = true;
        cboPhuongXa.Text = tenCu;
    }

    private async Task TaiPhuongXaAsync(
        int maTinh,
        int? maPhuongCanChon = null,
        string tenPhuongCu = "")
    {
        _wardRequest?.Cancel();
        var request = new CancellationTokenSource();
        _wardRequest = request;

        cboPhuongXa.DataSource = null;
        cboPhuongXa.Items.Clear();
        cboPhuongXa.Text = "Đang tải...";
        cboPhuongXa.Enabled = false;

        lblTrangThai.Text = "Đang tải danh sách phường/xã...";

        try
        {
            var ds = await DiaChiApiService.LayPhuongXaAsync(
                maTinh, request.Token);

            if (request.IsCancellationRequested)
                return;

            cboPhuongXa.DisplayMember = "Name";
            cboPhuongXa.ValueMember = "Code";
            cboPhuongXa.DataSource = ds;
            cboPhuongXa.SelectedIndex = -1;
            cboPhuongXa.Text = "";

            if (maPhuongCanChon.HasValue)
            {
                var xa = ds.FirstOrDefault(x =>
                    x.Code == maPhuongCanChon.Value);

                if (xa != null)
                    cboPhuongXa.SelectedItem = xa;
                else
                    cboPhuongXa.Text = tenPhuongCu;
            }
            else if (!string.IsNullOrWhiteSpace(tenPhuongCu))
            {
                cboPhuongXa.Text = tenPhuongCu;
            }

            lblTrangThai.Text =
                $"Đã tải {ds.Count} phường/xã.";
        }
        catch (OperationCanceledException)
            when (request.IsCancellationRequested)
        {
            // Người dùng đã chuyển tỉnh.
            // Bỏ qua kết quả yêu cầu cũ.
        }
        catch (Exception ex)
        {
            if (request.IsCancellationRequested)
                return;

            Log.Warning(ex,
                "Không tải được phường/xã MaTinh={MaTinh}",
                maTinh);

            cboPhuongXa.DataSource = null;
            cboPhuongXa.Items.Clear();
            cboPhuongXa.Text = tenPhuongCu;

            lblTrangThai.Text =
                "Không tải được phường/xã - được phép nhập tay.";

            MessageBox.Show(
                "Không tải được danh sách địa chỉ, " +
                "vui lòng kiểm tra mạng.\n" +
                "Bạn có thể nhập Phường/Xã thủ công.",
                "Cảnh báo mạng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            if (ReferenceEquals(_wardRequest, request))
            {
                _wardRequest = null;
                cboPhuongXa.Enabled = true;
            }

            request.Dispose();
        }
    }

    // =========================================
    // 5. CHỌN DÒNG ĐỂ SỬA
    // =========================================

    private async Task ChonKhachAsync(KhachRow row)
    {
        _maKhach = row.MaKhach;

        txtHoTen.Text = row.HoTen;
        txtSDT.Text = row.SoDienThoai;
        radNam.Checked = row.GioiTinh == "Nam";
        radNu.Checked = row.GioiTinh == "Nữ";
        dtpNgaySinh.Value = row.NgaySinh;
        chkTamTru.Checked = row.DaDangKyTamTru;

        cboPhong.SelectedValue = row.MaPhong;

        _khoaSuKienDiaChi = true;

        try
        {
            cboTinh.SelectedIndex = -1;

            DiaPhuong? tinh = null;

            if (row.MaTinh.HasValue)
            {
                tinh = cboTinh.Items
                    .OfType<DiaPhuong>()
                    .FirstOrDefault(x =>
                        x.Code == row.MaTinh.Value);
            }

            if (tinh != null)
                cboTinh.SelectedItem = tinh;
            else
                cboTinh.Text = row.TenTinh;
        }
        finally
        {
            _khoaSuKienDiaChi = false;
        }

        if (row.MaTinh.HasValue &&
            cboTinh.SelectedItem is DiaPhuong tinhChon)
        {
            await TaiPhuongXaAsync(
                tinhChon.Code,
                row.MaPhuongXa,
                row.TenPhuongXa);
        }
        else
        {
            ChoNhapPhuongXa(row.TenPhuongXa);
        }
    }

    // =========================================
    // 6. VALIDATION
    // =========================================

    private bool DuLieuHopLe()
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            CanhBao("Họ tên không được để trống.");
            txtHoTen.Focus();
            return false;
        }

        if (!Regex.IsMatch(
            txtSDT.Text.Trim(), @"^[0-9]{9,15}$"))
        {
            CanhBao("Số điện thoại phải gồm 9-15 chữ số.");
            txtSDT.Focus();
            return false;
        }

        if (dtpNgaySinh.Value.Date > DateTime.Today)
        {
            CanhBao("Ngày sinh không được ở tương lai.");
            return false;
        }

        if (cboPhong.SelectedItem is not PhongItem)
        {
            CanhBao("Vui lòng chọn phòng trọ.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(cboTinh.Text) ||
            cboTinh.Text == "Đang tải...")
        {
            CanhBao("Vui lòng chọn hoặc nhập tỉnh/thành.");
            return false;
        }

        if (!cboPhuongXa.Enabled ||
            string.IsNullOrWhiteSpace(cboPhuongXa.Text) ||
            cboPhuongXa.Text == "Đang tải...")
        {
            CanhBao("Vui lòng chọn hoặc nhập phường/xã.");
            return false;
        }

        if (cboTinh.Text.Trim().Length > 100 ||
            cboPhuongXa.Text.Trim().Length > 100)
        {
            CanhBao("Tên địa chỉ không được dài quá 100 ký tự.");
            return false;
        }

        return true;
    }

    private void CanhBao(string noiDung)
    {
        MessageBox.Show(
            noiDung,
            "Kiểm tra dữ liệu",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private KhachThue LayDuLieuForm()
    {
        var phong = (PhongItem)cboPhong.SelectedItem!;

        string tenTinh = cboTinh.Text.Trim();
        string tenXa = cboPhuongXa.Text.Trim();

        var tinh = cboTinh.SelectedItem as DiaPhuong;
        var xa = cboPhuongXa.SelectedItem as DiaPhuong;

        int? maTinh =
            tinh != null &&
            string.Equals(tinh.Name, tenTinh,
                StringComparison.OrdinalIgnoreCase)
                ? tinh.Code : null;

        int? maXa =
            maTinh.HasValue &&
            xa != null &&
            string.Equals(xa.Name, tenXa,
                StringComparison.OrdinalIgnoreCase)
                ? xa.Code : null;

        return new KhachThue
        {
            HoTen = txtHoTen.Text.Trim(),
            GioiTinh = radNam.Checked ? "Nam" : "Nữ",
            NgaySinh = DateOnly.FromDateTime(
                dtpNgaySinh.Value.Date),
            SoDienThoai = txtSDT.Text.Trim(),

            MaTinh = maTinh,
            TenTinh = tenTinh,

            MaPhuongXa = maXa,
            TenPhuongXa = tenXa,

            DaDangKyTamTru = chkTamTru.Checked,
            MaPhong = phong.MaPhong
        };
    }

    // =========================================
    // 7. ĐỒNG BỘ TRẠNG THÁI PHÒNG
    // =========================================

    private static void CapNhatTrangThaiPhong(
        Cau2_QuanLyNhaTroContext db,
        int maPhong)
    {
        var phong = db.Set<PhongTro>()
            .Single(x => x.MaPhong == maPhong);

        bool coKhach = db.Set<KhachThue>()
            .Any(x => x.MaPhong == maPhong);

        phong.TinhTrang =
            coKhach ? "Đang thuê" : "Trống";
    }

    // =========================================
    // 8. THÊM KHÁCH THUÊ
    // =========================================

    private void ThemKhach()
    {
        if (!DuLieuHopLe()) return;

        try
        {
            var khach = LayDuLieuForm();

            using var db = _dbFactory();
            using var tx = db.Database.BeginTransaction(
                IsolationLevel.Serializable);

            db.Set<KhachThue>().Add(khach);
            db.SaveChanges();

            CapNhatTrangThaiPhong(db, khach.MaPhong);
            db.SaveChanges();

            tx.Commit();

            Log.Information(
                "Thêm khách MaKhach={MaKhach}, MaPhong={MaPhong}",
                khach.MaKhach, khach.MaPhong);

            MessageBox.Show(
                "Thêm khách thuê thành công!",
                "Thông báo");

            LamMoiDuLieu();
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "thêm khách thuê");
        }
    }

    // =========================================
    // 9. SỬA KHÁCH THUÊ
    // =========================================

    private void SuaKhach()
    {
        if (_maKhach == null)
        {
            CanhBao("Vui lòng chọn khách cần sửa.");
            return;
        }

        if (!DuLieuHopLe()) return;

        try
        {
            var input = LayDuLieuForm();

            using var db = _dbFactory();
            using var tx = db.Database.BeginTransaction(
                IsolationLevel.Serializable);

            int maKhach = _maKhach.Value;

            var khach = db.Set<KhachThue>()
                .FirstOrDefault(x =>
                    x.MaKhach == maKhach);

            if (khach == null)
            {
                CanhBao("Khách thuê không còn tồn tại.");
                return;
            }

            int phongCu = khach.MaPhong;
            int phongMoi = input.MaPhong;

            khach.HoTen = input.HoTen;
            khach.GioiTinh = input.GioiTinh;
            khach.NgaySinh = input.NgaySinh;
            khach.SoDienThoai = input.SoDienThoai;

            khach.MaTinh = input.MaTinh;
            khach.TenTinh = input.TenTinh;
            khach.MaPhuongXa = input.MaPhuongXa;
            khach.TenPhuongXa = input.TenPhuongXa;

            khach.DaDangKyTamTru =
                input.DaDangKyTamTru;

            khach.MaPhong = phongMoi;

            db.SaveChanges();

            CapNhatTrangThaiPhong(db, phongCu);

            if (phongCu != phongMoi)
                CapNhatTrangThaiPhong(db, phongMoi);

            db.SaveChanges();
            tx.Commit();

            Log.Information(
                "Sửa khách MaKhach={MaKhach}, " +
                "PhongCu={PhongCu}, PhongMoi={PhongMoi}",
                maKhach, phongCu, phongMoi);

            MessageBox.Show(
                "Cập nhật khách thuê thành công!",
                "Thông báo");

            LamMoiDuLieu();
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "sửa khách thuê");
        }
    }

    // =========================================
    // 10. XÓA KHÁCH THUÊ
    // =========================================

    private void XoaKhach()
    {
        if (_maKhach == null)
        {
            CanhBao("Vui lòng chọn khách cần xóa.");
            return;
        }

        int maKhach = _maKhach.Value;

        var xacNhan = MessageBox.Show(
            $"Bạn có chắc muốn xóa khách mã {maKhach}?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (xacNhan != DialogResult.Yes)
            return;

        try
        {
            using var db = _dbFactory();
            using var tx = db.Database.BeginTransaction(
                IsolationLevel.Serializable);

            var khach = db.Set<KhachThue>()
                .FirstOrDefault(x =>
                    x.MaKhach == maKhach);

            if (khach == null)
            {
                CanhBao("Khách thuê không còn tồn tại.");
                return;
            }

            int maPhong = khach.MaPhong;

            db.Set<KhachThue>().Remove(khach);
            db.SaveChanges();

            CapNhatTrangThaiPhong(db, maPhong);
            db.SaveChanges();

            tx.Commit();

            Log.Information(
                "Xóa khách MaKhach={MaKhach}, MaPhong={MaPhong}",
                maKhach, maPhong);

            MessageBox.Show(
                "Xóa khách thuê thành công!",
                "Thông báo");

            LamMoiDuLieu();
        }
        catch (Exception ex)
        {
            BaoLoiCSDL(ex, "xóa khách thuê");
        }
    }

    // =========================================
    // 11. LÀM MỚI VÀ THÔNG BÁO LỖI
    // =========================================

    private void XoaTrangNhap()
    {
        _maKhach = null;
        _wardRequest?.Cancel();

        txtHoTen.Clear();
        txtSDT.Clear();

        radNam.Checked = true;
        dtpNgaySinh.Value = new DateTime(2004, 1, 1);

        if (cboPhong.Items.Count > 0)
            cboPhong.SelectedIndex = -1;

        chkTamTru.Checked = false;

        _khoaSuKienDiaChi = true;

        try
        {
            cboTinh.SelectedIndex = -1;
            cboTinh.Text = "";
            ChoNhapPhuongXa();
        }
        finally
        {
            _khoaSuKienDiaChi = false;
        }

        dgvKhach.ClearSelection();
    }

    private void LamMoiDuLieu()
    {
        TaiPhong();
        TaiBoLocTinh();
        TaiDanhSach();
        XoaTrangNhap();
    }

    private void LamMoi()
    {
        txtTim.Clear();
        LamMoiDuLieu();
    }

    private void BaoLoiCSDL(
        Exception ex, string thaoTac)
    {
        Log.Error(ex,
            "Lỗi CSDL khi {ThaoTac}", thaoTac);

        MessageBox.Show(
            $"Không thể {thaoTac}.\n" +
            "Vui lòng kiểm tra dữ liệu hoặc kết nối SQL Server.",
            "Lỗi ứng dụng",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _wardRequest?.Cancel();
        base.OnFormClosed(e);
    }
}

// =========================================
// CÁC LỚP HỖ TRỢ HIỂN THỊ
// =========================================

public sealed class PhongItem
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = "";
    public decimal GiaThue { get; set; }
    public string TinhTrang { get; set; } = "";

    public string HienThi =>
        $"{TenPhong} - {GiaThue:N0} đ ({TinhTrang})";
}

public sealed class PhongLocItem
{
    public int? MaPhong { get; set; }
    public string Ten { get; set; } = "";
}

public sealed class KhachRow
{
    public int MaKhach { get; set; }
    public string HoTen { get; set; } = "";
    public string GioiTinh { get; set; } = "";
    public DateTime NgaySinh { get; set; }
    public string SoDienThoai { get; set; } = "";

    public int? MaTinh { get; set; }
    public string TenTinh { get; set; } = "";
    public int? MaPhuongXa { get; set; }
    public string TenPhuongXa { get; set; } = "";

    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = "";
    public decimal GiaThue { get; set; }

    public bool DaDangKyTamTru { get; set; }
    public string QueQuan { get; set; } = "";
}

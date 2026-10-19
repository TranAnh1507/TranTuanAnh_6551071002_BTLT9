
using System.Data;
using System.Globalization;
using Cau4_BanHangTraSua.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cau4_BanHangTraSua;

public sealed class FormBanHang : Form
{
    private readonly Func<Cau4_BanHangTraSuaContext> _factory;

    private readonly List<CartLine> _cart = new();

    private static readonly HttpClient QrClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly CultureInfo _vi =
        CultureInfo.GetCultureInfo("vi-VN");

    private int? _reservedMaHd;
    private bool _transferPrepared;
    private bool _saving;
    private CancellationTokenSource? _qrCts;

    // CHỌN MÓN
    private readonly ComboBox cboDanhMuc = new();
    private readonly ListBox lstMon = new();
    private readonly NumericUpDown nudSoLy = new();
    private readonly Button btnThem = new();

    // GIỎ HÀNG
    private readonly DataGridView dgvGioHang = new();
    private readonly Label lblTongTien = new();
    private readonly Button btnXoaDong = new();
    private readonly Button btnHuyDon = new();

    // THANH TOÁN
    private readonly Button btnCk = new();
    private readonly Button btnTienMat = new();
    private readonly Button btnDaNhanTien = new();
    private readonly PictureBox picQr = new();

    private readonly Label lblHuongDan = new();
    private readonly Label lblOwner = new();
    private readonly Label lblBank = new();
    private readonly Label lblAccount = new();
    private readonly Label lblNoiDung = new();
    private readonly Label lblPaymentAmount = new();

    private readonly ProgressBar progress = new();
    private readonly Label lblShop = new();
    private readonly Button btnThucDon = new();

    // HÓA ĐƠN VÀ DOANH THU
    private readonly DateTimePicker dtpNgayHoaDon = new();
    private readonly DataGridView dgvHoaDon = new();
    private readonly DataGridView dgvChiTiet = new();
    private readonly Label lblDoanhThu = new();
    private readonly Label lblStatus = new();

    public FormBanHang(
        Func<Cau4_BanHangTraSuaContext> factory)
    {
        _factory = factory;

        Text = "Bán hàng - Quán trà sữa";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1260, 780);
        MinimumSize = new Size(1276, 819);
        MaximizeBox = false;

        Font = new Font("Segoe UI", 9);
        BackColor = Color.FromArgb(248, 249, 252);

        BuildUi();

        Shown += (_, _) =>
        {
            LoadMenu();
            LoadInvoices();
            RefreshCart();
        };
    }

    // =========================================
    // 1. CÁC HÀM TẠO GIAO DIỆN
    // =========================================

    private static Label L(
        string text, int x, int y, int w = 100)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 28),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Button B(
        string text, int x, int y, int w)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 34),
            FlatStyle = FlatStyle.System,
            Cursor = Cursors.Hand
        };
    }

    private static void Grid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.AutoGenerateColumns = false;

        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.BackgroundColor = Color.White;
        grid.ColumnHeadersHeight = 32;
    }

    private static void Col(
        DataGridView grid,
        string property,
        string title,
        float weight)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = title,
            FillWeight = weight
        });
    }

    private string Money(decimal value)
    {
        return value.ToString("N0", _vi) + " đ";
    }

    private decimal Total =>
        _cart.Sum(x => x.ThanhTien);

    // =========================================
    // 2. THIẾT KẾ FORM BÁN HÀNG
    // =========================================


    private void BuildUi()
    {
        // =====================================
        // 1. CẤU HÌNH FORM
        // =====================================
        ClientSize = new Size(1040, 590);
        MinimumSize = new Size(1056, 629);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Color.FromArgb(248, 249, 252);

        // Tách bán hàng và lịch sử hóa đơn
        var tabs = new TabControl
        {
            Location = new Point(8, 8),
            Size = new Size(1024, 545)
        };

        var tabBanHang = new TabPage("Bán hàng")
        {
            BackColor = Color.FromArgb(248, 249, 252)
        };

        var tabHoaDon = new TabPage("Lịch sử hóa đơn")
        {
            BackColor = Color.FromArgb(248, 249, 252)
        };

        tabs.TabPages.Add(tabBanHang);
        tabs.TabPages.Add(tabHoaDon);
        Controls.Add(tabs);

        // =====================================
        // 2. KHUNG CHỌN MÓN - BÊN TRÁI
        // =====================================
        var grpMenu = new GroupBox
        {
            Text = "Chọn món",
            Location = new Point(10, 14),
            Size = new Size(258, 475)
        };

        tabBanHang.Controls.Add(grpMenu);

        grpMenu.Controls.Add(
            L("Danh mục:", 10, 26, 72));

        cboDanhMuc.SetBounds(82, 26, 164, 27);
        cboDanhMuc.DropDownStyle =
            ComboBoxStyle.DropDownList;

        cboDanhMuc.SelectedIndexChanged +=
            (_, _) => LoadActiveMon();

        grpMenu.Controls.Add(cboDanhMuc);

        lstMon.SetBounds(10, 65, 238, 310);
        lstMon.DisplayMember = "Display";
        lstMon.IntegralHeight = false;

        grpMenu.Controls.Add(lstMon);

        grpMenu.Controls.Add(
            L("Số ly:", 10, 394, 50));

        nudSoLy.SetBounds(61, 394, 67, 27);
        nudSoLy.Minimum = 1;
        nudSoLy.Maximum = 10000;
        nudSoLy.Value = 1;

        grpMenu.Controls.Add(nudSoLy);

        btnThem.SetBounds(138, 390, 110, 34);
        btnThem.Text = "Thêm vào đơn";

        btnThem.Click += (_, _) =>
            AddToCart();

        grpMenu.Controls.Add(btnThem);

        // =====================================
        // 3. KHUNG ĐƠN HIỆN TẠI - Ở GIỮA
        // =====================================
        var grpCart = new GroupBox
        {
            Text = "Đơn hiện tại",
            Location = new Point(280, 14),
            Size = new Size(438, 475)
        };

        tabBanHang.Controls.Add(grpCart);

        dgvGioHang.SetBounds(10, 26, 418, 328);

        Grid(dgvGioHang);

        dgvGioHang.Columns.Clear();

        Col(dgvGioHang, "TenMon", "Tên món", 195);
        Col(dgvGioHang, "SoLy", "SL", 38);
        Col(dgvGioHang, "DonGia", "Đơn giá", 85);
        Col(dgvGioHang, "ThanhTien", "Thành tiền", 110);

        dgvGioHang.Columns[2]
            .DefaultCellStyle.Format = "N0";

        dgvGioHang.Columns[3]
            .DefaultCellStyle.Format = "N0";

        dgvGioHang.Columns[2]
            .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

        dgvGioHang.Columns[3]
            .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

        grpCart.Controls.Add(dgvGioHang);

        btnXoaDong.SetBounds(10, 365, 198, 33);
        btnXoaDong.Text = "Xóa dòng";

        btnXoaDong.Click += (_, _) =>
            RemoveSelected();

        grpCart.Controls.Add(btnXoaDong);

        btnHuyDon.SetBounds(217, 365, 211, 33);
        btnHuyDon.Text = "Hủy / đơn mới";

        btnHuyDon.Click += (_, _) =>
        {
            if (_cart.Count == 0)
            {
                ResetOrder();
                return;
            }

            string warning = _transferPrepared
                ? "Đã tạo mã QR. Chỉ hủy nếu chắc chắn " +
                  "khách CHƯA chuyển tiền. Hủy đơn này?"
                : "Hủy đơn hiện tại và xóa giỏ hàng?";

            if (MessageBox.Show(
                warning,
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                ResetOrder();
            }
        };

        grpCart.Controls.Add(btnHuyDon);

        lblTongTien.SetBounds(10, 416, 418, 44);

        lblTongTien.Font = new Font(
            "Segoe UI", 15, FontStyle.Bold);

        lblTongTien.ForeColor = Color.Firebrick;

        lblTongTien.TextAlign =
            ContentAlignment.MiddleCenter;

        lblTongTien.Text = "Tổng tiền: 0 đ";

        grpCart.Controls.Add(lblTongTien);

        // =====================================
        // 4. KHUNG THANH TOÁN - BÊN PHẢI
        // =====================================
        var grpPay = new GroupBox
        {
            Text = "Thanh toán",
            Location = new Point(730, 14),
            Size = new Size(277, 475)
        };

        tabBanHang.Controls.Add(grpPay);

        // Chuyển khoản
        btnCk.SetBounds(10, 27, 255, 33);
        btnCk.Text = "Thanh toán chuyển khoản";

        btnCk.Click += async (_, _) =>
            await ShowQrAsync();

        grpPay.Controls.Add(btnCk);

        // Tiền mặt
        btnTienMat.SetBounds(10, 68, 255, 33);
        btnTienMat.Text = "Tiền mặt";

        btnTienMat.Click += (_, _) =>
            PayCash();

        grpPay.Controls.Add(btnTienMat);

        // Thanh tiến trình tải QR
        progress.SetBounds(10, 106, 255, 7);
        progress.Style = ProgressBarStyle.Marquee;
        progress.Visible = false;

        grpPay.Controls.Add(progress);

        // Ảnh QR
        picQr.SetBounds(55, 117, 167, 170);
        picQr.SizeMode = PictureBoxSizeMode.Zoom;
        picQr.BackColor = Color.White;
        picQr.Visible = false;

        grpPay.Controls.Add(picQr);

        // Thông báo trước khi lập đơn
        lblHuongDan.SetBounds(13, 136, 249, 100);

        lblHuongDan.TextAlign =
            ContentAlignment.MiddleCenter;

        lblHuongDan.Text =
            "Lập đơn, sau đó chọn phương thức thanh toán.";

        lblHuongDan.Visible = true;

        grpPay.Controls.Add(lblHuongDan);

        // Thông tin quán
        lblShop.SetBounds(12, 288, 253, 18);
        lblShop.TextAlign =
            ContentAlignment.MiddleCenter;
        lblShop.Visible = false;

        grpPay.Controls.Add(lblShop);

        // Thông tin chuyển khoản
        lblOwner.SetBounds(12, 307, 253, 27);
        lblBank.SetBounds(12, 336, 253, 19);
        lblAccount.SetBounds(12, 357, 253, 19);
        lblPaymentAmount.SetBounds(12, 379, 253, 19);
        lblNoiDung.SetBounds(12, 401, 253, 22);

        foreach (var label in new[]
        {
        lblOwner,
        lblBank,
        lblAccount,
        lblPaymentAmount,
        lblNoiDung
    })
        {
            label.Font = new Font("Segoe UI", 8);
            label.AutoEllipsis = true;
            grpPay.Controls.Add(label);
        }

        // Xác nhận đã nhận tiền
        btnDaNhanTien.SetBounds(10, 435, 255, 32);
        btnDaNhanTien.Text = "Đã nhận tiền";

        btnDaNhanTien.BackColor =
            Color.FromArgb(25, 147, 77);

        btnDaNhanTien.ForeColor = Color.White;
        btnDaNhanTien.FlatStyle = FlatStyle.Flat;

        btnDaNhanTien.Enabled = false;

        btnDaNhanTien.Click += (_, _) =>
            ConfirmTransfer();

        grpPay.Controls.Add(btnDaNhanTien);

        // =====================================
        // 5. TAB LỊCH SỬ HÓA ĐƠN
        // =====================================
        var grpHistory = new GroupBox
        {
            Text = "Danh sách hóa đơn và doanh thu",
            Location = new Point(10, 14),
            Size = new Size(996, 475)
        };

        tabHoaDon.Controls.Add(grpHistory);

        grpHistory.Controls.Add(
            L("Ngày:", 12, 27, 48));

        dtpNgayHoaDon.SetBounds(66, 27, 165, 27);

        dtpNgayHoaDon.Format =
            DateTimePickerFormat.Custom;

        dtpNgayHoaDon.CustomFormat = "dd/MM/yyyy";
        dtpNgayHoaDon.Value = DateTime.Today;

        dtpNgayHoaDon.ValueChanged +=
            (_, _) => LoadInvoices();

        grpHistory.Controls.Add(dtpNgayHoaDon);

        var btnReload = B(
            "Tải hóa đơn", 241, 24, 130);

        btnReload.Click += (_, _) =>
            LoadInvoices();

        grpHistory.Controls.Add(btnReload);

        // Nút quản lý thực đơn
        btnThucDon.SetBounds(382, 24, 158, 33);
        btnThucDon.Text = "Quản lý thực đơn";

        btnThucDon.Click += (_, _) =>
        {
            if (_cart.Count > 0 &&
                MessageBox.Show(
                    "Thay đổi thực đơn không cập nhật giá " +
                    "trong giỏ hiện tại. Tiếp tục?",
                    "Thông báo",
                    MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
            {
                return;
            }

            using var form = new FormThucDon(_factory);
            form.ShowDialog(this);

            LoadMenu();
        };

        grpHistory.Controls.Add(btnThucDon);

        // Doanh thu
        lblDoanhThu.SetBounds(557, 20, 422, 40);

        lblDoanhThu.TextAlign =
            ContentAlignment.MiddleRight;

        lblDoanhThu.Font =
            new Font("Segoe UI", 11, FontStyle.Bold);

        lblDoanhThu.ForeColor = Color.Firebrick;

        grpHistory.Controls.Add(lblDoanhThu);

        // =====================================
        // 6. BẢNG DANH SÁCH HÓA ĐƠN
        // =====================================
        dgvHoaDon.SetBounds(12, 69, 470, 390);

        Grid(dgvHoaDon);
        dgvHoaDon.Columns.Clear();

        Col(dgvHoaDon, "MaHd", "Mã HD", 72);
        Col(dgvHoaDon, "NgayLap", "Giờ lập", 132);
        Col(dgvHoaDon, "HinhThucTt", "Hình thức", 133);
        Col(dgvHoaDon, "TongTien", "Tổng tiền", 110);

        dgvHoaDon.Columns[1]
            .DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

        dgvHoaDon.Columns[3]
            .DefaultCellStyle.Format = "N0";

        dgvHoaDon.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 ||
                dgvHoaDon.Rows[e.RowIndex].DataBoundItem
                    is not InvoiceRow row)
            {
                return;
            }

            LoadInvoiceDetails(row.MaHd);
        };

        grpHistory.Controls.Add(dgvHoaDon);

        // =====================================
        // 7. BẢNG CHI TIẾT HÓA ĐƠN
        // =====================================
        dgvChiTiet.SetBounds(494, 69, 485, 390);

        Grid(dgvChiTiet);
        dgvChiTiet.Columns.Clear();

        Col(dgvChiTiet, "TenMon", "Tên món", 195);
        Col(dgvChiTiet, "SoLy", "SL", 45);
        Col(dgvChiTiet, "DonGia", "Đơn giá", 100);
        Col(dgvChiTiet, "ThanhTien", "Thành tiền", 115);

        dgvChiTiet.Columns[2]
            .DefaultCellStyle.Format = "N0";

        dgvChiTiet.Columns[3]
            .DefaultCellStyle.Format = "N0";

        grpHistory.Controls.Add(dgvChiTiet);

        // =====================================
        // 8. THANH TRẠNG THÁI
        // =====================================
        lblStatus.SetBounds(12, 561, 1015, 23);
        lblStatus.ForeColor = Color.DimGray;

        Controls.Add(lblStatus);
    }


    // =========================================
    // 3. TẢI DANH MỤC VÀ MÓN ĐANG BÁN
    // =========================================

    private void LoadMenu()
    {
        try
        {
            using var db = _factory();

            var categories = db.Set<DanhMucMon>()
                .AsNoTracking()
                .Where(d =>
                    db.Set<MonUong>().Any(m =>
                        m.MaDm == d.MaDm &&
                        m.DangBan))
                .OrderBy(d => d.MaDm)
                .Select(d => new CategoryChoice
                {
                    MaDm = d.MaDm,
                    TenDm = d.TenDm
                })
                .ToList();

            cboDanhMuc.DataSource = null;
            cboDanhMuc.DisplayMember = "TenDm";
            cboDanhMuc.ValueMember = "MaDm";
            cboDanhMuc.DataSource = categories;

            if (categories.Count > 0)
                cboDanhMuc.SelectedIndex = 0;

            LoadActiveMon();
        }
        catch (Exception ex)
        {
            DatabaseError(ex, "tải thực đơn");
        }
    }

    private void LoadActiveMon()
    {
        try
        {
            if (cboDanhMuc.SelectedItem
                is not CategoryChoice category)
            {
                lstMon.DataSource = null;
                return;
            }

            using var db = _factory();

            var items = db.Set<MonUong>()
                .AsNoTracking()
                .Where(x =>
                    x.MaDm == category.MaDm &&
                    x.DangBan)
                .OrderBy(x => x.TenMon)
                .Select(x => new MenuChoice
                {
                    MaMon = x.MaMon,
                    TenMon = x.TenMon,
                    GiaBan = x.GiaBan
                })
                .ToList();

            lstMon.DataSource = null;
            lstMon.DisplayMember = "Display";
            lstMon.DataSource = items;
        }
        catch (Exception ex)
        {
            DatabaseError(ex, "tải món đang bán");
        }
    }

    // =========================================
    // 4. GIỎ HÀNG
    // =========================================

    private void EnsureCartEditable()
    {
        if (_transferPrepared)
        {
            throw new InvalidOperationException(
                "Đã tạo yêu cầu chuyển khoản. " +
                "Nhấn 'Hủy / đơn mới' nếu muốn sửa giỏ hàng.");
        }
    }

    private void AddToCart()
    {
        try
        {
            EnsureCartEditable();

            if (lstMon.SelectedItem is not MenuChoice mon)
            {
                MessageBox.Show("Chọn món để thêm.");
                return;
            }

            int quantity = (int)nudSoLy.Value;

            var found = _cart.FirstOrDefault(x =>
                x.MaMon == mon.MaMon);

            if (found == null)
            {
                _cart.Add(new CartLine
                {
                    MaMon = mon.MaMon,
                    TenMon = mon.TenMon,
                    DonGia = mon.GiaBan,
                    SoLy = quantity
                });
            }
            else
            {
                found.SoLy =
                    checked(found.SoLy + quantity);
            }

            RefreshCart();
        }
        catch (Exception ex)
        {
            Log.Warning(ex,
                "Không thể sửa giỏ hàng");

            MessageBox.Show(
                ex.Message,
                "Thông báo");
        }
    }

    private void RemoveSelected()
    {
        try
        {
            EnsureCartEditable();

            if (dgvGioHang.SelectedRows.Count == 0 ||
                dgvGioHang.SelectedRows[0].DataBoundItem
                    is not CartLine row)
            {
                return;
            }

            _cart.RemoveAll(x =>
                x.MaMon == row.MaMon);

            RefreshCart();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Thông báo");
        }
    }

    private void RefreshCart()
    {
        dgvGioHang.DataSource = null;

        dgvGioHang.DataSource = _cart
            .Select(x => x.Clone())
            .ToList();

        dgvGioHang.ClearSelection();

        lblTongTien.Text =
            "Tổng tiền: " + Money(Total);

        if (_cart.Count == 0)
        {
            btnDaNhanTien.Enabled = false;
        }
    }

    // =========================================
    // 5. CẤP MÃ HÓA ĐƠN TRƯỚC KHI TẠO QR
    // =========================================

    private int ReserveInvoiceId()
    {
        if (_reservedMaHd.HasValue)
            return _reservedMaHd.Value;

        using var db = _factory();

        db.Database.OpenConnection();

        try
        {
            using var command =
                db.Database.GetDbConnection()
                    .CreateCommand();

            command.CommandText =
                "SELECT NEXT VALUE FOR dbo.SeqHoaDon";

            int id = Convert.ToInt32(
                command.ExecuteScalar());

            _reservedMaHd = id;

            return id;
        }
        finally
        {
            db.Database.CloseConnection();
        }
    }

    // =========================================
    // 6. TẠO MÃ VIETQR
    // =========================================

    private async Task ShowQrAsync()
    {
        if (_saving)
            return;

        if (_cart.Count == 0 || Total <= 0)
        {
            MessageBox.Show(
                "Thêm món vào đơn trước khi tạo QR.");
            return;
        }

        btnCk.Enabled = false;
        btnTienMat.Enabled = false;
        btnHuyDon.Enabled = false;
        btnThucDon.Enabled = false;
        btnDaNhanTien.Enabled = false;

        progress.Visible = true;

        using var request =
            new CancellationTokenSource();

        _qrCts = request;

        try
        {
            int id = ReserveInvoiceId();

            _transferPrepared = true;

            btnThem.Enabled = false;
            btnXoaDong.Enabled = false;

            PaymentSettings payment =
                PaymentSettingsReader.Load();

            string description = $"TRASUA HD{id}";

            // Hiển thị thông tin chuyển khoản dạng chữ
            // để vẫn sử dụng được nếu mất mạng.

            lblOwner.Text =
                "Quán: " + payment.ShopName +
                " | " + payment.AccountName;

            lblBank.Text =
                "Ngân hàng: " + payment.BankId;

            lblAccount.Text =
                "Số tài khoản: " + payment.AccountNo;

            lblPaymentAmount.Text =
                "Số tiền: " + Money(Total);

            lblNoiDung.Text =
                "Nội dung: " + description;

            lblShop.Text = payment.ShopName;
            lblShop.Visible = true;

            lblHuongDan.Text =
                "Hãy kiểm tra thông báo ngân hàng " +
                "trước khi xác nhận đã nhận tiền.";

            lblHuongDan.Visible = false;
            picQr.Visible = true;

            string url =
                PaymentSettingsReader.BuildVietQrUrl(
                    payment,
                    Total,
                    description);

            byte[] bytes =
                await QrClient.GetByteArrayAsync(
                    url,
                    request.Token);

            if (request.IsCancellationRequested ||
                IsDisposed)
            {
                return;
            }

            using var stream =
                new MemoryStream(bytes);

            using var loaded =
                Image.FromStream(stream);

            Image clone = new Bitmap(loaded);

            var old = picQr.Image;
            picQr.Image = clone;
            old?.Dispose();

            btnDaNhanTien.Enabled = true;

            Log.Information(
                "Tạo VietQR cho MaHD={MaHD}, TongTien={TongTien}",
                id,
                Total);

            lblStatus.Text =
                "Đã tải VietQR. " +
                "Chỉ bấm 'Đã nhận tiền' SAU khi kiểm tra ngân hàng.";
        }
        catch (OperationCanceledException)
            when (request.IsCancellationRequested)
        {
            // Form đóng trong lúc tải QR.
        }
        catch (Exception ex)
        {
            if (IsDisposed)
                return;

            Log.Error(
                "Không tải được QR. MaHD={MaHD}, TongTien={TongTien}, ErrorType={ErrorType}",
                _reservedMaHd,
                Total,
                ex.GetType().Name);

            if (ex is not InvalidOperationException &&
                ex is not ArgumentOutOfRangeException)
            {
                // Thu ngân vẫn có thể xác nhận
                // sau khi kiểm tra tiền thực tế.
                btnDaNhanTien.Enabled = true;
            }

            picQr.Visible = false;
            lblHuongDan.Visible = true;

            lblHuongDan.Text =
                "Không tạo được ảnh QR. " +
                "Khách có thể chuyển tay theo thông tin bên dưới.";

            if (ex is InvalidOperationException ||
                ex is ArgumentOutOfRangeException)
            {
                _transferPrepared = false;

                btnDaNhanTien.Enabled = false;
                btnThem.Enabled = true;
                btnXoaDong.Enabled = true;
            }

            lblStatus.Text =
                "QR không khả dụng. " +
                "Kiểm tra cấu hình ngân hàng hoặc kết nối mạng.";

            MessageBox.Show(
                "Không tải được VietQR.\n" +
                "Kiểm tra cấu hình Payment và mạng.\n" +
                "Nếu đã có thông tin chuyển khoản hợp lệ, " +
                "khách có thể chuyển tay.",
                "Cảnh báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            if (ReferenceEquals(_qrCts, request))
                _qrCts = null;

            if (!IsDisposed && !Disposing)
            {
                progress.Visible = false;
                btnCk.Enabled = true;
                btnTienMat.Enabled = true;
                btnHuyDon.Enabled = true;
                btnThucDon.Enabled = true;
            }
        }
    }

    // =========================================
    // 7. XÁC NHẬN CHUYỂN KHOẢN
    // =========================================

    private void ConfirmTransfer()
    {
        if (!_transferPrepared ||
            !_reservedMaHd.HasValue)
        {
            MessageBox.Show(
                "Chưa tạo yêu cầu chuyển khoản.");
            return;
        }

        var confirm = MessageBox.Show(
            "Bạn ĐÃ kiểm tra tài khoản ngân hàng " +
            "và xác nhận thực sự nhận đủ " +
            Money(Total) + "?\n" +
            "VietQR KHÔNG tự xác nhận đã chuyển tiền.",
            "Xác nhận đã nhận tiền",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        SaveInvoice("Chuyển khoản");
    }

    // =========================================
    // 8. THANH TOÁN TIỀN MẶT
    // =========================================

    private void PayCash()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show(
                "Chưa có món trong giỏ hàng.");
            return;
        }

        if (_transferPrepared)
        {
            var warning = MessageBox.Show(
                "Đã tạo mã chuyển khoản.\n" +
                "Bạn đã kiểm tra chắc chắn khách CHƯA " +
                "chuyển khoản để tránh thu hai lần?",
                "Kiểm tra thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (warning != DialogResult.Yes)
                return;
        }

        var confirm = MessageBox.Show(
            "Xác nhận đã nhận tiền mặt: " +
            Money(Total) + "?",
            "Thanh toán tiền mặt",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        SaveInvoice("Tiền mặt");
    }

    // =========================================
    // 9. LƯU HÓA ĐƠN VÀ CHI TIẾT
    // =========================================

    private void SaveInvoice(string method)
    {
        if (_saving || _cart.Count == 0)
            return;

        _saving = true;

        try
        {
            int id = ReserveInvoiceId();

            using var db = _factory();

            // Một lần SaveChanges cho cả hóa đơn
            // và các dòng chi tiết.
            var invoice = new HoaDon
            {
                MaHd = id,
                NgayLap = DateTime.Now,
                TongTien = Total,
                HinhThucTt = method,
                DaThanhToan = true
            };

            db.Set<HoaDon>().Add(invoice);

            foreach (CartLine item in _cart)
            {
                db.Set<ChiTietHoaDon>().Add(
                    new ChiTietHoaDon
                    {
                        MaHd = id,
                        MaMon = item.MaMon,
                        SoLy = item.SoLy,
                        DonGia = item.DonGia
                    });
            }

            db.SaveChanges();

            Log.Information(
                "Thanh toán thành công: " +
                "MaHD={MaHD}, PhuongThuc={Method}, TongTien={TongTien}",
                id,
                method,
                Total);

            MessageBox.Show(
                $"Lưu hóa đơn HD{id} thành công ({method}).",
                "Hoàn tất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            dtpNgayHoaDon.Value = DateTime.Today;

            ResetOrder();
            LoadInvoices();
        }
        catch (Exception ex)
        {
            DatabaseError(ex, "lưu hóa đơn");
        }
        finally
        {
            _saving = false;
        }
    }

    // =========================================
    // 10. LÀM MỚI ĐƠN HÀNG
    // =========================================

    private void ResetOrder()
    {
        _cart.Clear();

        _reservedMaHd = null;
        _transferPrepared = false;

        btnThem.Enabled = true;
        btnXoaDong.Enabled = true;
        btnDaNhanTien.Enabled = false;

        picQr.Image?.Dispose();
        picQr.Image = null;
        picQr.Visible = false;

        lblHuongDan.Visible = true;

        lblHuongDan.Text =
            "Lập đơn, sau đó chọn phương thức thanh toán.";
        lblShop.Visible = false;
        lblOwner.Text = "";
        lblBank.Text = "";
        lblAccount.Text = "";
        lblNoiDung.Text = "";
        lblPaymentAmount.Text = "";

        RefreshCart();
    }

    // =========================================
    // 11. DANH SÁCH HÓA ĐƠN
    // =========================================

    private void LoadInvoices()
    {
        try
        {
            DateTime start =
                dtpNgayHoaDon.Value.Date;

            DateTime end =
                start.AddDays(1);

            using var db = _factory();

            var invoices = db.Set<HoaDon>()
                .AsNoTracking()
                .Where(h =>
                    h.NgayLap >= start &&
                    h.NgayLap < end)
                .OrderByDescending(h => h.NgayLap)
                .Select(h => new InvoiceRow
                {
                    MaHd = h.MaHd,
                    NgayLap = h.NgayLap,
                    HinhThucTt = h.HinhThucTt,
                    TongTien = h.TongTien
                })
                .ToList();

            dgvHoaDon.DataSource = null;
            dgvHoaDon.DataSource = invoices;
            dgvHoaDon.ClearSelection();

            dgvChiTiet.DataSource = null;

            decimal revenue = db.Set<HoaDon>()
                .AsNoTracking()
                .Where(h =>
                    h.NgayLap >= start &&
                    h.NgayLap < end &&
                    h.DaThanhToan)
                .Sum(h => (decimal?)h.TongTien) ?? 0m;

            lblDoanhThu.Text =
                "Doanh thu ngày: " + Money(revenue);

            lblStatus.Text =
                $"Có {invoices.Count} hóa đơn trong ngày.";
        }
        catch (Exception ex)
        {
            DatabaseError(ex, "tải hóa đơn");
        }
    }

    // =========================================
    // 12. CHI TIẾT HÓA ĐƠN
    // =========================================

    private void LoadInvoiceDetails(int invoiceId)
    {
        try
        {
            using var db = _factory();

            var details =
                (from c in db.Set<ChiTietHoaDon>()
                    .AsNoTracking()
                 join m in db.Set<MonUong>()
                    .AsNoTracking()
                    on c.MaMon equals m.MaMon
                 where c.MaHd == invoiceId
                 select new InvoiceDetailRow
                 {
                     TenMon = m.TenMon,
                     SoLy = c.SoLy,
                     DonGia = c.DonGia,
                     ThanhTien = c.SoLy * c.DonGia
                 })
                .ToList();

            dgvChiTiet.DataSource = null;
            dgvChiTiet.DataSource = details;
            dgvChiTiet.ClearSelection();
        }
        catch (Exception ex)
        {
            DatabaseError(ex, "tải chi tiết hóa đơn");
        }
    }

    // =========================================
    // 13. XỬ LÝ LỖI
    // =========================================

    private void DatabaseError(
        Exception ex,
        string operation)
    {
        Log.Error(ex,
            "Lỗi khi {Operation}",
            operation);

        MessageBox.Show(
            "Không thể " + operation +
            ". Vui lòng kiểm tra dữ liệu và kết nối SQL Server.",
            "Lỗi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _qrCts?.Cancel();
        picQr.Image?.Dispose();

        base.OnFormClosed(e);
    }
}

// =========================================
// 14. CÁC CLASS HỖ TRỢ
// =========================================

public sealed class MenuChoice
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public decimal GiaBan { get; set; }

    public string Display =>
        $"{TenMon} - {GiaBan:N0} đ";
}

public sealed class CartLine
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public int SoLy { get; set; }
    public decimal DonGia { get; set; }

    public decimal ThanhTien =>
        checked(SoLy * DonGia);

    public CartLine Clone()
    {
        return new CartLine
        {
            MaMon = MaMon,
            TenMon = TenMon,
            SoLy = SoLy,
            DonGia = DonGia
        };
    }
}

public sealed class InvoiceRow
{
    public int MaHd { get; set; }
    public DateTime NgayLap { get; set; }
    public string HinhThucTt { get; set; } = "";
    public decimal TongTien { get; set; }
}

public sealed class InvoiceDetailRow
{
    public string TenMon { get; set; } = "";
    public int SoLy { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

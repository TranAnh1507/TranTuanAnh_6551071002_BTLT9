
using Cau4_BanHangTraSua.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cau4_BanHangTraSua;

public sealed class FormThucDon : Form
{
    private readonly Func<Cau4_BanHangTraSuaContext> _factory;
    private int? _selectedMon;
    private int? _selectedCategory;

    private readonly TextBox txtTenMon = new();
    private readonly NumericUpDown nudGia = new();
    private readonly ComboBox cboDanhMuc = new();
    private readonly CheckBox chkDangBan = new();
    private readonly TextBox txtTim = new();
    private readonly ComboBox cboLocDanhMuc = new();
    private readonly DataGridView dgvMon = new();

    private readonly TextBox txtTenDm = new();
    private readonly TextBox txtTimDm = new();
    private readonly DataGridView dgvDanhMuc = new();

    public FormThucDon(
        Func<Cau4_BanHangTraSuaContext> factory)
    {
        _factory = factory;

        Text = "Quản lý thực đơn - Quán trà sữa";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(970, 620);
        MinimumSize = new Size(986, 659);
        Font = new Font("Segoe UI", 9);
        BackColor = Color.FromArgb(248, 250, 252);

        BuildUi();
        Shown += (_, _) => LoadAll();
    }

    private static Label L(
        string text, int x, int y, int w = 100)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 27),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Button B(
        string text,
        int x,
        int y,
        int w,
        EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 32)
        };

        button.Click += click;
        return button;
    }

    private void BuildUi()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };

        var tabMon = new TabPage("Quản lý món");
        var tabDm = new TabPage("Quản lý danh mục");

        tabs.TabPages.AddRange(new[]
        {
            tabMon, tabDm
        });

        Controls.Add(tabs);

        // =================================
        // TAB 1: QUẢN LÝ MÓN
        // =================================

        var edit = new GroupBox
        {
            Text = "Thông tin món",
            Location = new Point(12, 12),
            Size = new Size(920, 114)
        };

        tabMon.Controls.Add(edit);

        edit.Controls.Add(L("Tên món", 12, 26, 75));

        txtTenMon.SetBounds(90, 26, 245, 27);
        txtTenMon.MaxLength = 100;
        edit.Controls.Add(txtTenMon);

        edit.Controls.Add(L("Giá bán", 347, 26, 70));

        nudGia.SetBounds(418, 26, 155, 27);
        nudGia.Minimum = 1;
        nudGia.Maximum = 999999999999999999m;
        nudGia.Increment = 1000;
        nudGia.ThousandsSeparator = true;
        edit.Controls.Add(nudGia);

        edit.Controls.Add(L("Danh mục", 582, 26, 77));

        cboDanhMuc.SetBounds(660, 26, 240, 27);
        cboDanhMuc.DropDownStyle =
            ComboBoxStyle.DropDownList;
        edit.Controls.Add(cboDanhMuc);

        chkDangBan.Text = "Đang bán";
        chkDangBan.Checked = true;
        chkDangBan.SetBounds(90, 71, 120, 27);
        edit.Controls.Add(chkDangBan);

        edit.Controls.Add(B(
            "Thêm món", 300, 67, 110,
            (_, _) => AddMon()));

        edit.Controls.Add(B(
            "Sửa món", 420, 67, 105,
            (_, _) => EditMon()));

        edit.Controls.Add(B(
            "Xóa món", 535, 67, 105,
            (_, _) => DeleteMon()));

        edit.Controls.Add(B(
            "Làm mới", 650, 67, 105,
            (_, _) =>
            {
                ClearMon();
                LoadMon();
            }));

        // TÌM KIẾM MÓN

        var find = new GroupBox
        {
            Text = "Tìm kiếm món",
            Location = new Point(12, 137),
            Size = new Size(920, 67)
        };

        tabMon.Controls.Add(find);

        find.Controls.Add(L("Tên món", 10, 24, 75));

        txtTim.SetBounds(88, 24, 305, 27);
        txtTim.PlaceholderText = "Nhập tên món...";
        find.Controls.Add(txtTim);

        find.Controls.Add(L("Danh mục", 406, 24, 77));

        cboLocDanhMuc.SetBounds(484, 24, 280, 27);
        cboLocDanhMuc.DropDownStyle =
            ComboBoxStyle.DropDownList;
        find.Controls.Add(cboLocDanhMuc);

        find.Controls.Add(B(
            "Tìm", 779, 20, 125,
            (_, _) => LoadMon()));

        txtTim.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadMon();
                e.SuppressKeyPress = true;
            }
        };

        // DATAGRIDVIEW MÓN

        dgvMon.SetBounds(12, 215, 920, 340);
        ConfigureGrid(dgvMon);

        dgvMon.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "MaMon",
            HeaderText = "Mã",
            FillWeight = 50
        });

        dgvMon.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TenMon",
            HeaderText = "Tên món",
            FillWeight = 220
        });

        dgvMon.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TenDm",
            HeaderText = "Danh mục",
            FillWeight = 150
        });

        dgvMon.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "GiaBan",
            HeaderText = "Giá bán",
            FillWeight = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N0"
            }
        });

        dgvMon.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = "DangBan",
            HeaderText = "Đang bán",
            FillWeight = 75
        });

        tabMon.Controls.Add(dgvMon);

        dgvMon.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 ||
                dgvMon.Rows[e.RowIndex].DataBoundItem
                    is not MonGridRow row)
            {
                return;
            }

            _selectedMon = row.MaMon;
            txtTenMon.Text = row.TenMon;

            nudGia.Value = Math.Clamp(
                row.GiaBan,
                nudGia.Minimum,
                nudGia.Maximum);

            cboDanhMuc.SelectedValue = row.MaDm;
            chkDangBan.Checked = row.DangBan;
        };

        // =================================
        // TAB 2: QUẢN LÝ DANH MỤC
        // =================================

        var grpCat = new GroupBox
        {
            Text = "Danh mục món",
            Location = new Point(12, 12),
            Size = new Size(920, 105)
        };

        tabDm.Controls.Add(grpCat);

        grpCat.Controls.Add(
            L("Tên danh mục", 15, 28, 120));

        txtTenDm.SetBounds(140, 28, 325, 27);
        txtTenDm.MaxLength = 50;
        grpCat.Controls.Add(txtTenDm);

        grpCat.Controls.Add(B(
            "Thêm", 485, 25, 95,
            (_, _) => AddCategory()));

        grpCat.Controls.Add(B(
            "Sửa", 588, 25, 95,
            (_, _) => EditCategory()));

        grpCat.Controls.Add(B(
            "Xóa", 691, 25, 95,
            (_, _) => DeleteCategory()));

        grpCat.Controls.Add(B(
            "Làm mới", 794, 25, 110,
            (_, _) =>
            {
                ClearCategory();
                LoadAll();
            }));

        tabDm.Controls.Add(
            L("Tìm danh mục", 22, 124, 112));

        txtTimDm.SetBounds(140, 124, 285, 27);
        txtTimDm.PlaceholderText = "Nhập tên danh mục...";
        tabDm.Controls.Add(txtTimDm);

        tabDm.Controls.Add(B(
            "Tìm", 434, 121, 90,
            (_, _) => LoadAll()));

        tabDm.Controls.Add(B(
            "Bỏ lọc", 533, 121, 90,
            (_, _) =>
            {
                txtTimDm.Clear();
                LoadAll();
            }));

        var note = L(
            "Danh mục đang có món không thể xóa; " +
            "món đã có hóa đơn nên ngừng bán thay vì xóa.",
            22, 160, 880);

        note.ForeColor = Color.DarkSlateBlue;
        tabDm.Controls.Add(note);

        dgvDanhMuc.SetBounds(12, 192, 920, 363);
        ConfigureGrid(dgvDanhMuc);

        dgvDanhMuc.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaDm",
                HeaderText = "Mã",
                FillWeight = 50
            });

        dgvDanhMuc.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenDm",
                HeaderText = "Tên danh mục",
                FillWeight = 300
            });

        dgvDanhMuc.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoMon",
                HeaderText = "Số món",
                FillWeight = 90
            });

        tabDm.Controls.Add(dgvDanhMuc);

        dgvDanhMuc.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 ||
                dgvDanhMuc.Rows[e.RowIndex].DataBoundItem
                    is not DanhMucRow row)
            {
                return;
            }

            _selectedCategory = row.MaDm;
            txtTenDm.Text = row.TenDm;
        };
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.RowHeadersVisible = false;
        grid.MultiSelect = false;
        grid.AutoGenerateColumns = false;

        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        grid.BackgroundColor = Color.White;
        grid.ColumnHeadersHeight = 34;
    }

    private void Execute(
        Action<Cau4_BanHangTraSuaContext> operation,
        string description)
    {
        try
        {
            using var db = _factory();
            operation(db);
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Lỗi khi {Description}", description);

            MessageBox.Show(
                "Không thể " + description +
                ". Hãy kiểm tra dữ liệu hoặc kết nối SQL Server.",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadAll()
    {
        Execute(db =>
        {
            var categories = db.Set<DanhMucMon>()
                .AsNoTracking()
                .OrderBy(x => x.TenDm)
                .Select(x => new CategoryChoice
                {
                    MaDm = x.MaDm,
                    TenDm = x.TenDm
                })
                .ToList();

            cboDanhMuc.DataSource = null;
            cboDanhMuc.DisplayMember = "TenDm";
            cboDanhMuc.ValueMember = "MaDm";
            cboDanhMuc.DataSource = categories;

            cboDanhMuc.SelectedIndex =
                categories.Count > 0 ? 0 : -1;

            var filter = new List<CategoryChoice>
            {
                new() { MaDm = 0, TenDm = "Tất cả" }
            };

            filter.AddRange(categories.Select(x =>
                new CategoryChoice
                {
                    MaDm = x.MaDm,
                    TenDm = x.TenDm
                }));

            cboLocDanhMuc.DataSource = null;
            cboLocDanhMuc.DisplayMember = "TenDm";
            cboLocDanhMuc.ValueMember = "MaDm";
            cboLocDanhMuc.DataSource = filter;

            var counts = db.Set<MonUong>()
                .AsNoTracking()
                .GroupBy(m => m.MaDm)
                .Select(g => new
                {
                    MaDm = g.Key,
                    SoMon = g.Count()
                })
                .ToDictionary(
                    x => x.MaDm,
                    x => x.SoMon);

            string keyword = txtTimDm.Text.Trim();

            dgvDanhMuc.DataSource = categories
                .Where(x =>
                    keyword.Length == 0 ||
                    x.TenDm.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase))
                .Select(x => new DanhMucRow
                {
                    MaDm = x.MaDm,
                    TenDm = x.TenDm,
                    SoMon = counts.GetValueOrDefault(x.MaDm)
                })
                .OrderBy(x => x.MaDm)
                .ToList();

            dgvDanhMuc.ClearSelection();
        }, "tải danh mục");

        LoadMon();
    }

    private void LoadMon()
    {
        Execute(db =>
        {
            var query =
                from m in db.Set<MonUong>().AsNoTracking()
                join d in db.Set<DanhMucMon>().AsNoTracking()
                    on m.MaDm equals d.MaDm
                select new { m, d };

            string key = txtTim.Text.Trim();

            if (key.Length > 0)
            {
                query = query.Where(x =>
                    x.m.TenMon.Contains(key));
            }

            if (cboLocDanhMuc.SelectedItem
                    is CategoryChoice cat &&
                cat.MaDm != 0)
            {
                query = query.Where(x =>
                    x.m.MaDm == cat.MaDm);
            }

            dgvMon.DataSource = query
                .OrderBy(x => x.m.MaMon)
                .Select(x => new MonGridRow
                {
                    MaMon = x.m.MaMon,
                    TenMon = x.m.TenMon,
                    GiaBan = x.m.GiaBan,
                    DangBan = x.m.DangBan,
                    MaDm = x.m.MaDm,
                    TenDm = x.d.TenDm
                })
                .ToList();

            dgvMon.ClearSelection();
        }, "tải thực đơn");
    }

    private bool ValidMon()
    {
        return !string.IsNullOrWhiteSpace(txtTenMon.Text)
            && nudGia.Value > 0
            && cboDanhMuc.SelectedItem is CategoryChoice;
    }

    private void AddMon()
    {
        if (!ValidMon())
        {
            MessageBox.Show(
                "Nhập tên, giá > 0 và chọn danh mục.");
            return;
        }

        Execute(db =>
        {
            int dm =
                ((CategoryChoice)cboDanhMuc.SelectedItem!).MaDm;

            db.Set<MonUong>().Add(new MonUong
            {
                TenMon = txtTenMon.Text.Trim(),
                GiaBan = nudGia.Value,
                DangBan = chkDangBan.Checked,
                MaDm = dm
            });

            db.SaveChanges();
            ClearMon();
            LoadAll();
        }, "thêm món");
    }

    private void EditMon()
    {
        if (!_selectedMon.HasValue)
        {
            MessageBox.Show("Chọn một món cần sửa.");
            return;
        }

        if (!ValidMon())
        {
            MessageBox.Show(
                "Nhập tên, giá > 0 và chọn danh mục.");
            return;
        }

        Execute(db =>
        {
            var m = db.Set<MonUong>()
                .Find(_selectedMon.Value);

            if (m == null)
            {
                MessageBox.Show("Món không còn tồn tại.");
                return;
            }

            m.TenMon = txtTenMon.Text.Trim();
            m.GiaBan = nudGia.Value;

            m.MaDm =
                ((CategoryChoice)cboDanhMuc.SelectedItem!).MaDm;

            m.DangBan = chkDangBan.Checked;

            db.SaveChanges();
            ClearMon();
            LoadAll();
        }, "sửa món");
    }

    private void DeleteMon()
    {
        if (!_selectedMon.HasValue)
        {
            MessageBox.Show("Chọn món cần xóa.");
            return;
        }

        if (MessageBox.Show(
                "Xóa món đã chọn?",
                "Xác nhận",
                MessageBoxButtons.YesNo)
            != DialogResult.Yes)
        {
            return;
        }

        Execute(db =>
        {
            int id = _selectedMon.Value;

            if (db.Set<ChiTietHoaDon>()
                .Any(x => x.MaMon == id))
            {
                MessageBox.Show(
                    "Món đã có lịch sử bán. " +
                    "Hãy bỏ chọn 'Đang bán' thay vì xóa.");
                return;
            }

            var m = db.Set<MonUong>().Find(id);
            if (m == null) return;

            db.Set<MonUong>().Remove(m);
            db.SaveChanges();

            ClearMon();
            LoadAll();
        }, "xóa món");
    }

    private void AddCategory()
    {
        string name = txtTenDm.Text.Trim();

        if (name.Length == 0)
        {
            MessageBox.Show("Nhập tên danh mục.");
            return;
        }

        Execute(db =>
        {
            db.Set<DanhMucMon>().Add(new DanhMucMon
            {
                TenDm = name
            });

            db.SaveChanges();
            ClearCategory();
            LoadAll();
        }, "thêm danh mục");
    }

    private void EditCategory()
    {
        if (!_selectedCategory.HasValue)
        {
            MessageBox.Show("Chọn danh mục cần sửa.");
            return;
        }

        string name = txtTenDm.Text.Trim();

        if (name.Length == 0)
        {
            MessageBox.Show("Nhập tên danh mục.");
            return;
        }

        Execute(db =>
        {
            var dm = db.Set<DanhMucMon>()
                .Find(_selectedCategory.Value);

            if (dm == null) return;

            dm.TenDm = name;
            db.SaveChanges();

            ClearCategory();
            LoadAll();
        }, "sửa danh mục");
    }

    private void DeleteCategory()
    {
        if (!_selectedCategory.HasValue)
        {
            MessageBox.Show("Chọn danh mục cần xóa.");
            return;
        }

        if (MessageBox.Show(
                "Xóa danh mục?",
                "Xác nhận",
                MessageBoxButtons.YesNo)
            != DialogResult.Yes)
        {
            return;
        }

        Execute(db =>
        {
            int id = _selectedCategory.Value;

            if (db.Set<MonUong>()
                .Any(m => m.MaDm == id))
            {
                MessageBox.Show(
                    "Không thể xóa vì danh mục còn món. " +
                    "Hãy chuyển hoặc xóa món trước.");
                return;
            }

            var dm = db.Set<DanhMucMon>().Find(id);
            if (dm == null) return;

            db.Set<DanhMucMon>().Remove(dm);
            db.SaveChanges();

            ClearCategory();
            LoadAll();
        }, "xóa danh mục");
    }

    private void ClearMon()
    {
        _selectedMon = null;
        txtTenMon.Clear();
        nudGia.Value = 1;
        chkDangBan.Checked = true;

        if (cboDanhMuc.Items.Count > 0)
            cboDanhMuc.SelectedIndex = 0;

        dgvMon.ClearSelection();
    }

    private void ClearCategory()
    {
        _selectedCategory = null;
        txtTenDm.Clear();
        dgvDanhMuc.ClearSelection();
    }
}

// ======================================
// CÁC CLASS HỖ TRỢ
// ======================================

public sealed class CategoryChoice
{
    public int MaDm { get; set; }
    public string TenDm { get; set; } = "";
}

public sealed class MonGridRow
{
    public int MaMon { get; set; }
    public int MaDm { get; set; }
    public string TenMon { get; set; } = "";
    public string TenDm { get; set; } = "";
    public decimal GiaBan { get; set; }
    public bool DangBan { get; set; }
}

public sealed class DanhMucRow
{
    public int MaDm { get; set; }
    public string TenDm { get; set; } = "";
    public int SoMon { get; set; }
}

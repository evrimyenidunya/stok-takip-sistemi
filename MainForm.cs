using System.Data;
using Microsoft.Data.Sqlite;

namespace StokTakipSistemi;

public sealed class MainForm : Form
{
    private readonly UserSession session;

    private readonly DataGridView productGrid = CreateGrid();
    private readonly TextBox productCodeBox = new();
    private readonly TextBox productNameBox = new();
    private readonly ComboBox productCategoryBox = new();
    private readonly TextBox brandModelBox = new();
    private readonly NumericUpDown stockAmountBox = new();
    private readonly TextBox locationBox = new();
    private readonly TextBox productDescriptionBox = new();

    private readonly DataGridView categoryGrid = CreateGrid();
    private readonly TextBox categoryNameBox = new();
    private readonly TextBox categoryDescriptionBox = new();

    private readonly ComboBox stockInProductBox = new();
    private readonly NumericUpDown stockInQuantityBox = new();
    private readonly DateTimePicker stockInDatePicker = new();
    private readonly TextBox stockInDescriptionBox = new();

    private readonly ComboBox stockOutProductBox = new();
    private readonly NumericUpDown stockOutQuantityBox = new();
    private readonly TextBox usagePlaceBox = new();
    private readonly TextBox deliveredToBox = new();
    private readonly DateTimePicker stockOutDatePicker = new();
    private readonly TextBox stockOutDescriptionBox = new();

    private readonly DataGridView stockGrid = CreateGrid();
    private readonly TextBox searchBox = new();
    private readonly ComboBox stockCategoryFilterBox = new();
    private readonly ComboBox stockSortBox = new();

    private readonly DataGridView reportSummaryGrid = CreateGrid();
    private readonly DataGridView reportCategoryGrid = CreateGrid();

    public MainForm(UserSession session)
    {
        this.session = session;
        Text = $"Stok Takip Sistemi - {session.Username} ({session.Role})";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);

        var tabs = new TabControl { Dock = DockStyle.Fill };

        if (session.IsAdmin)
        {
            tabs.TabPages.Add(CreateProductsTab());
            tabs.TabPages.Add(CreateCategoriesTab());
        }

        tabs.TabPages.Add(CreateStockInTab());
        tabs.TabPages.Add(CreateStockOutTab());
        tabs.TabPages.Add(CreateCurrentStockTab());

        if (session.IsAdmin)
        {
            tabs.TabPages.Add(CreateReportsTab());
        }

        Controls.Add(tabs);

        LoadAllData();
    }

    private TabPage CreateProductsTab()
    {
        var tab = new TabPage("Ürün Tanımlama");
        var split = CreateSplitContainer();

        var formPanel = CreateFormPanel();
        AddLabeledControl(formPanel, "Ürün kodu", productCodeBox);
        AddLabeledControl(formPanel, "Ürün adı", productNameBox);
        AddLabeledControl(formPanel, "Kategori", productCategoryBox);
        AddLabeledControl(formPanel, "Marka / model", brandModelBox);
        stockAmountBox.Maximum = 1_000_000;
        AddLabeledControl(formPanel, "Mevcut stok", stockAmountBox);
        AddLabeledControl(formPanel, "Raf / lokasyon", locationBox);
        productDescriptionBox.Multiline = true;
        productDescriptionBox.Height = 72;
        AddLabeledControl(formPanel, "Açıklama", productDescriptionBox);

        var buttons = CreateButtonPanel();
        buttons.Controls.Add(CreateButton("Ekle", AddProduct));
        buttons.Controls.Add(CreateButton("Güncelle", UpdateProduct));
        buttons.Controls.Add(CreateButton("Sil", DeleteProduct));
        buttons.Controls.Add(CreateButton("Temizle", ClearProductForm));
        formPanel.Controls.Add(buttons);

        productGrid.SelectionChanged += (_, _) => FillProductFormFromGrid();

        split.Panel1.Controls.Add(formPanel);
        split.Panel2.Controls.Add(productGrid);
        tab.Controls.Add(split);
        return tab;
    }

    private TabPage CreateCategoriesTab()
    {
        var tab = new TabPage("Kategori Tanımlama");
        var split = CreateSplitContainer();
        var formPanel = CreateFormPanel();

        AddLabeledControl(formPanel, "Kategori adı", categoryNameBox);
        categoryDescriptionBox.Multiline = true;
        categoryDescriptionBox.Height = 90;
        AddLabeledControl(formPanel, "Açıklama", categoryDescriptionBox);

        var buttons = CreateButtonPanel();
        buttons.Controls.Add(CreateButton("Ekle", AddCategory));
        buttons.Controls.Add(CreateButton("Güncelle", UpdateCategory));
        buttons.Controls.Add(CreateButton("Sil", DeleteCategory));
        buttons.Controls.Add(CreateButton("Temizle", ClearCategoryForm));
        formPanel.Controls.Add(buttons);

        categoryGrid.SelectionChanged += (_, _) => FillCategoryFormFromGrid();

        split.Panel1.Controls.Add(formPanel);
        split.Panel2.Controls.Add(categoryGrid);
        tab.Controls.Add(split);
        return tab;
    }

    private TabPage CreateStockInTab()
    {
        var tab = new TabPage("Stok Giriş");
        var panel = CreateCenteredPanel();

        stockInQuantityBox.Maximum = 1_000_000;
        stockInQuantityBox.Minimum = 1;
        stockInDatePicker.Value = DateTime.Today;
        stockInDescriptionBox.Multiline = true;
        stockInDescriptionBox.Height = 80;

        AddLabeledControl(panel, "Ürün seçimi", stockInProductBox);
        AddLabeledControl(panel, "Giriş miktarı", stockInQuantityBox);
        AddLabeledControl(panel, "Giriş tarihi", stockInDatePicker);
        AddLabeledControl(panel, "Açıklama", stockInDescriptionBox);
        panel.Controls.Add(CreateButton("Stok Girişi Kaydet", SaveStockIn));

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateStockOutTab()
    {
        var tab = new TabPage("Stok Çıkış");
        var panel = CreateCenteredPanel();

        stockOutQuantityBox.Maximum = 1_000_000;
        stockOutQuantityBox.Minimum = 1;
        stockOutDatePicker.Value = DateTime.Today;
        stockOutDescriptionBox.Multiline = true;
        stockOutDescriptionBox.Height = 80;

        AddLabeledControl(panel, "Ürün seçimi", stockOutProductBox);
        AddLabeledControl(panel, "Çıkış miktarı", stockOutQuantityBox);
        AddLabeledControl(panel, "Kullanım yeri", usagePlaceBox);
        AddLabeledControl(panel, "Teslim alan kişi", deliveredToBox);
        AddLabeledControl(panel, "Çıkış tarihi", stockOutDatePicker);
        AddLabeledControl(panel, "Açıklama", stockOutDescriptionBox);
        panel.Controls.Add(CreateButton("Stok Çıkışı Kaydet", SaveStockOut));

        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateCurrentStockTab()
    {
        var tab = new TabPage("Mevcut Stok Listesi");
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        searchBox.Width = 220;
        searchBox.PlaceholderText = "Ürün adına göre ara";
        searchBox.TextChanged += (_, _) => LoadStockList();

        stockCategoryFilterBox.Width = 220;
        stockCategoryFilterBox.DropDownStyle = ComboBoxStyle.DropDownList;
        stockCategoryFilterBox.SelectedIndexChanged += (_, _) => LoadStockList();

        stockSortBox.Width = 170;
        stockSortBox.DropDownStyle = ComboBoxStyle.DropDownList;
        stockSortBox.Items.AddRange(["Stok artan", "Stok azalan"]);
        stockSortBox.SelectedIndex = 0;
        stockSortBox.SelectedIndexChanged += (_, _) => LoadStockList();

        filters.Controls.AddRange([
            new Label { Text = "Arama:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            searchBox,
            new Label { Text = "Kategori:", AutoSize = true, Padding = new Padding(16, 8, 0, 0) },
            stockCategoryFilterBox,
            new Label { Text = "Sıralama:", AutoSize = true, Padding = new Padding(16, 8, 0, 0) },
            stockSortBox
        ]);

        root.Controls.Add(filters, 0, 0);
        root.Controls.Add(stockGrid, 0, 1);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage CreateReportsTab()
    {
        var tab = new TabPage("Raporlama");
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

        var refreshButton = CreateButton("Raporları Yenile", LoadReports);
        root.Controls.Add(refreshButton, 0, 0);
        root.Controls.Add(reportSummaryGrid, 0, 1);
        root.Controls.Add(reportCategoryGrid, 0, 2);
        tab.Controls.Add(root);
        return tab;
    }

    private void LoadAllData()
    {
        LoadCategories();
        LoadProducts();
        LoadProductCombos();
        LoadStockFilters();
        LoadStockList();
        if (session.IsAdmin)
        {
            LoadReports();
        }
    }

    private void LoadCategories()
    {
        var table = Database.Query("SELECT Id, Name AS 'Kategori Adı', Description AS 'Açıklama' FROM Categories ORDER BY Name;");
        categoryGrid.DataSource = table;
        HideIdColumn(categoryGrid);

        productCategoryBox.DataSource = table.Copy();
        productCategoryBox.DisplayMember = "Kategori Adı";
        productCategoryBox.ValueMember = "Id";
        productCategoryBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private void LoadProducts()
    {
        productGrid.DataSource = Database.Query("""
            SELECT p.Id,
                   p.ProductCode AS 'Ürün Kodu',
                   p.ProductName AS 'Ürün Adı',
                   c.Name AS 'Kategori',
                   p.CategoryId,
                   p.BrandModel AS 'Marka / Model',
                   p.CurrentStock AS 'Mevcut Stok',
                   p.Location AS 'Lokasyon',
                   p.Description AS 'Açıklama'
            FROM Products p
            INNER JOIN Categories c ON c.Id = p.CategoryId
            ORDER BY p.ProductName;
            """);
        HideIdColumn(productGrid);
        if (productGrid.Columns.Contains("CategoryId"))
        {
            productGrid.Columns["CategoryId"]!.Visible = false;
        }
    }

    private void LoadProductCombos()
    {
        var table = Database.Query("""
            SELECT Id, ProductCode || ' - ' || ProductName AS ProductText
            FROM Products
            ORDER BY ProductName;
            """);

        ConfigureProductCombo(stockInProductBox, table.Copy());
        ConfigureProductCombo(stockOutProductBox, table.Copy());
    }

    private void LoadStockFilters()
    {
        var table = Database.Query("SELECT Id, Name FROM Categories ORDER BY Name;");
        var allRow = table.NewRow();
        allRow["Id"] = 0;
        allRow["Name"] = "Tüm kategoriler";
        table.Rows.InsertAt(allRow, 0);

        stockCategoryFilterBox.DataSource = table;
        stockCategoryFilterBox.DisplayMember = "Name";
        stockCategoryFilterBox.ValueMember = "Id";
    }

    private void LoadStockList()
    {
        var categoryId = stockCategoryFilterBox.SelectedValue is int id ? id : 0;
        var sortDirection = stockSortBox.SelectedIndex == 1 ? "DESC" : "ASC";
        var sql = $"""
            SELECT p.ProductCode AS 'Ürün Kodu',
                   p.ProductName AS 'Ürün Adı',
                   c.Name AS 'Kategori',
                   p.CurrentStock AS 'Mevcut Stok',
                   p.Location AS 'Lokasyon'
            FROM Products p
            INNER JOIN Categories c ON c.Id = p.CategoryId
            WHERE p.ProductName LIKE $search
              AND ($categoryId = 0 OR p.CategoryId = $categoryId)
            ORDER BY p.CurrentStock {sortDirection}, p.ProductName;
            """;

        stockGrid.DataSource = Database.Query(
            sql,
            new SqliteParameter("$search", $"%{searchBox.Text.Trim()}%"),
            new SqliteParameter("$categoryId", categoryId));
    }

    private void LoadReports()
    {
        reportSummaryGrid.DataSource = Database.Query("""
            SELECT COUNT(*) AS 'Toplam Ürün Çeşidi',
                   IFNULL(SUM(CurrentStock), 0) AS 'Toplam Stok Miktarı',
                   IFNULL(MIN(CurrentStock), 0) AS 'En Düşük Stok',
                   IFNULL(MAX(CurrentStock), 0) AS 'En Yüksek Stok'
            FROM Products;
            """);

        reportCategoryGrid.DataSource = Database.Query("""
            SELECT c.Name AS 'Kategori',
                   COUNT(p.Id) AS 'Ürün Çeşidi',
                   IFNULL(SUM(p.CurrentStock), 0) AS 'Toplam Stok'
            FROM Categories c
            LEFT JOIN Products p ON p.CategoryId = c.Id
            GROUP BY c.Id, c.Name
            ORDER BY c.Name;
            """);
    }

    private void AddProduct()
    {
        if (!ValidateProductForm())
        {
            return;
        }

        try
        {
            Database.Execute("""
                INSERT INTO Products
                    (ProductCode, ProductName, CategoryId, BrandModel, CurrentStock, Location, Description)
                VALUES
                    ($code, $name, $categoryId, $brand, $stock, $location, $description);
                """,
                new SqliteParameter("$code", productCodeBox.Text.Trim()),
                new SqliteParameter("$name", productNameBox.Text.Trim()),
                new SqliteParameter("$categoryId", Convert.ToInt32(productCategoryBox.SelectedValue)),
                new SqliteParameter("$brand", brandModelBox.Text.Trim()),
                new SqliteParameter("$stock", Convert.ToInt32(stockAmountBox.Value)),
                new SqliteParameter("$location", locationBox.Text.Trim()),
                new SqliteParameter("$description", productDescriptionBox.Text.Trim()));

            ClearProductForm();
            LoadAllData();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Bu ürün kodu zaten kayıtlı.", "Kayıt hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateProduct()
    {
        var id = SelectedId(productGrid);
        if (id is null || !ValidateProductForm())
        {
            return;
        }

        Database.Execute("""
            UPDATE Products
            SET ProductCode = $code,
                ProductName = $name,
                CategoryId = $categoryId,
                BrandModel = $brand,
                CurrentStock = $stock,
                Location = $location,
                Description = $description
            WHERE Id = $id;
            """,
            new SqliteParameter("$code", productCodeBox.Text.Trim()),
            new SqliteParameter("$name", productNameBox.Text.Trim()),
            new SqliteParameter("$categoryId", Convert.ToInt32(productCategoryBox.SelectedValue)),
            new SqliteParameter("$brand", brandModelBox.Text.Trim()),
            new SqliteParameter("$stock", Convert.ToInt32(stockAmountBox.Value)),
            new SqliteParameter("$location", locationBox.Text.Trim()),
            new SqliteParameter("$description", productDescriptionBox.Text.Trim()),
            new SqliteParameter("$id", id.Value));

        ClearProductForm();
        LoadAllData();
    }

    private void DeleteProduct()
    {
        var id = SelectedId(productGrid);
        if (id is null)
        {
            return;
        }

        if (MessageBox.Show("Seçili ürünü silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        Database.Execute("DELETE FROM Products WHERE Id = $id;", new SqliteParameter("$id", id.Value));
        ClearProductForm();
        LoadAllData();
    }

    private void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(categoryNameBox.Text))
        {
            MessageBox.Show("Kategori adı zorunludur.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Database.Execute("""
                INSERT INTO Categories (Name, Description)
                VALUES ($name, $description);
                """,
                new SqliteParameter("$name", categoryNameBox.Text.Trim()),
                new SqliteParameter("$description", categoryDescriptionBox.Text.Trim()));

            ClearCategoryForm();
            LoadAllData();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Bu kategori zaten kayıtlı.", "Kayıt hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateCategory()
    {
        var id = SelectedId(categoryGrid);
        if (id is null || string.IsNullOrWhiteSpace(categoryNameBox.Text))
        {
            return;
        }

        Database.Execute("""
            UPDATE Categories
            SET Name = $name, Description = $description
            WHERE Id = $id;
            """,
            new SqliteParameter("$name", categoryNameBox.Text.Trim()),
            new SqliteParameter("$description", categoryDescriptionBox.Text.Trim()),
            new SqliteParameter("$id", id.Value));

        ClearCategoryForm();
        LoadAllData();
    }

    private void DeleteCategory()
    {
        var id = SelectedId(categoryGrid);
        if (id is null)
        {
            return;
        }

        if (MessageBox.Show("Seçili kategoriyi silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Database.Execute("DELETE FROM Categories WHERE Id = $id;", new SqliteParameter("$id", id.Value));
            ClearCategoryForm();
            LoadAllData();
        }
        catch (SqliteException)
        {
            MessageBox.Show("Bu kategoriye bağlı ürün bulunduğu için kategori silinemez.", "Silme hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SaveStockIn()
    {
        if (stockInProductBox.SelectedValue is null)
        {
            MessageBox.Show("Önce ürün seçmelisiniz.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Database.ExecuteStockMovement(
            Convert.ToInt32(stockInProductBox.SelectedValue),
            "Giriş",
            Convert.ToInt32(stockInQuantityBox.Value),
            stockInDatePicker.Value,
            "",
            "",
            stockInDescriptionBox.Text.Trim(),
            session.Id);

        stockInQuantityBox.Value = 1;
        stockInDescriptionBox.Clear();
        LoadAllData();
        MessageBox.Show("Stok girişi kaydedildi.", "İşlem başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SaveStockOut()
    {
        if (stockOutProductBox.SelectedValue is null)
        {
            MessageBox.Show("Önce ürün seçmelisiniz.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Database.ExecuteStockMovement(
                Convert.ToInt32(stockOutProductBox.SelectedValue),
                "Çıkış",
                Convert.ToInt32(stockOutQuantityBox.Value),
                stockOutDatePicker.Value,
                usagePlaceBox.Text.Trim(),
                deliveredToBox.Text.Trim(),
                stockOutDescriptionBox.Text.Trim(),
                session.Id);

            stockOutQuantityBox.Value = 1;
            usagePlaceBox.Clear();
            deliveredToBox.Clear();
            stockOutDescriptionBox.Clear();
            LoadAllData();
            MessageBox.Show("Stok çıkışı kaydedildi.", "İşlem başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Stok yetersiz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private bool ValidateProductForm()
    {
        if (string.IsNullOrWhiteSpace(productCodeBox.Text) || string.IsNullOrWhiteSpace(productNameBox.Text))
        {
            MessageBox.Show("Ürün kodu ve ürün adı zorunludur.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (productCategoryBox.SelectedValue is null)
        {
            MessageBox.Show("Kategori seçmelisiniz.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void FillProductFormFromGrid()
    {
        if (productGrid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        productCodeBox.Text = row["Ürün Kodu"].ToString();
        productNameBox.Text = row["Ürün Adı"].ToString();
        productCategoryBox.SelectedValue = Convert.ToInt32(row["CategoryId"]);
        brandModelBox.Text = row["Marka / Model"].ToString();
        stockAmountBox.Value = Convert.ToDecimal(row["Mevcut Stok"]);
        locationBox.Text = row["Lokasyon"].ToString();
        productDescriptionBox.Text = row["Açıklama"].ToString();
    }

    private void FillCategoryFormFromGrid()
    {
        if (categoryGrid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        categoryNameBox.Text = row["Kategori Adı"].ToString();
        categoryDescriptionBox.Text = row["Açıklama"].ToString();
    }

    private void ClearProductForm()
    {
        productCodeBox.Clear();
        productNameBox.Clear();
        brandModelBox.Clear();
        stockAmountBox.Value = 0;
        locationBox.Clear();
        productDescriptionBox.Clear();
        productCodeBox.Focus();
    }

    private void ClearCategoryForm()
    {
        categoryNameBox.Clear();
        categoryDescriptionBox.Clear();
        categoryNameBox.Focus();
    }

    private static int? SelectedId(DataGridView grid)
    {
        if (grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show("Önce listeden bir kayıt seçmelisiniz.", "Seçim gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        return Convert.ToInt32(row["Id"]);
    }

    private static void ConfigureProductCombo(ComboBox comboBox, DataTable dataSource)
    {
        comboBox.DataSource = dataSource;
        comboBox.DisplayMember = "ProductText";
        comboBox.ValueMember = "Id";
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private static void HideIdColumn(DataGridView grid)
    {
        if (grid.Columns.Contains("Id"))
        {
            grid.Columns["Id"]!.Visible = false;
        }
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White
        };
    }

    private static SplitContainer CreateSplitContainer()
    {
        return new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 360,
            FixedPanel = FixedPanel.Panel1
        };
    }

    private static FlowLayoutPanel CreateFormPanel()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(16),
            WrapContents = false
        };
    }

    private static FlowLayoutPanel CreateCenteredPanel()
    {
        return new FlowLayoutPanel
        {
            Width = 520,
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(28),
            WrapContents = false
        };
    }

    private static FlowLayoutPanel CreateButtonPanel()
    {
        return new FlowLayoutPanel
        {
            Width = 320,
            Height = 92,
            FlowDirection = FlowDirection.LeftToRight
        };
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 150,
            Height = 34,
            Margin = new Padding(0, 8, 8, 0)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static void AddLabeledControl(FlowLayoutPanel panel, string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            Width = 320,
            Height = 22,
            Margin = new Padding(0, 8, 0, 0)
        };

        control.Width = 320;
        control.Margin = new Padding(0, 0, 0, 4);
        panel.Controls.Add(label);
        panel.Controls.Add(control);
    }
}

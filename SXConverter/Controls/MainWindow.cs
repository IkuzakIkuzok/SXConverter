
// (c) 2024 Kazuki Kohzuki

using SXConverter.Ufs;

namespace SXConverter.Controls;

[DesignerCategory("code")]
internal sealed class MainWindow : Form
{
    private readonly PathBox tb_source, tb_destination;
    private readonly NumericUpDown nud_timeStart, num_timeEnd, nud_wlStart, nud_wlEnd;
    private readonly Button trim;
    private readonly TextBox tb_metadata;

    private SpectraData? data;

    internal MainWindow()
    {
        this.Text = "SX Converter";
        this.Size = this.MinimumSize = this.MaximumSize = new(550, 430);

        #region source

        _ = new Label()
        {
            Text = "Source",
            Location = new(10, 10),
            Width = 70,
            Parent = this,
        };

        this.tb_source = new()
        {
            Location = new(80, 10 - 2),
            Width = 400,
            Parent = this,
        };

        _ = new BrowseButton<OpenFileDialog>()
        {
            Text = "...",
            Filter = "Comma-separated values|*.csv|Ultrafast Systems Data|*.ufs|All files|*.*",
            Target = this.tb_source,
            Location = new(490, 10 - 2),
            Size = new(30, 25),
            Parent = this
        };

        var load = new Button()
        {
            Text = "Load",
            Location = new(240, 40),
            Size = new(70, 30),
            Enabled = false,
            Parent = this,
        };
        load.Click += LoadData;
        this.tb_source.TextChanged += (s, e) => load.Enabled = !string.IsNullOrWhiteSpace(this.tb_source.Text);

        #endregion source

        #region info

        _ = new Label()
        {
            Text = "Time (ps)",
            Location = new(30, 100),
            Width = 100,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(130, 100),
            Size = new(40, 20),
            Parent = this,
        };

        this.nud_timeStart = new()
        {
            Location = new(170, 100),
            Size = new(80, 20),
            Enabled = false,
            Minimum = -1_000m,
            Maximum = 1_000_000m,
            DecimalPlaces = 3,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "To",
            Location = new(260, 100),
            Size = new Size(20, 20),
            Parent = this,
        };

        this.num_timeEnd = new()
        {
            Location = new(280, 100),
            Size = new(80, 20),
            Enabled = false,
            Minimum = -1_000m,
            Maximum = 1_000_000m,
            DecimalPlaces = 3,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "Wavelength (nm)",
            Location = new(30, 140),
            Width = 100,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(130, 140),
            Size = new(40, 20),
            Parent = this,
        };

        this.nud_wlStart = new()
        {
            Location = new(170, 140),
            Size = new(80, 20),
            Enabled = false,
            Minimum = 0m,
            Maximum = 100_000m,
            DecimalPlaces = 3,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "To",
            Location = new(260, 140),
            Size = new(20, 20),
            Parent = this,
        };

        this.nud_wlEnd = new()
        {
            Location = new(280, 140),
            Size = new(80, 20),
            Enabled = false,
            Minimum = 0m,
            Maximum = 100_000m,
            DecimalPlaces = 3,
            Parent = this,
        };

        this.trim = new Button()
        {
            Text = "Trim",
            Location = new(380, 115),
            Size = new(70, 30),
            Enabled = false,
            Parent = this,
        };
        this.trim.Click += TrimRange;

        _ = new Label()
        {
            Text = "Metadata",
            Location = new(30, 180),
            Width = 60,
            Parent = this,
        };

        this.tb_metadata = new()
        {
            Location = new(90, 180),
            Width = 390,
            Height = 100,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Enabled = false,
            Parent = this,
        };

        #endregion info

        #region destination

        _ = new Label()
        {
            Text = "Destination",
            Location = new(10, 320),
            Width = 70,
            Parent = this,
        };

        this.tb_destination = new()
        {
            Location = new(80, 320 - 2),
            Width = 400,
            Parent = this,
        };

        _ = new BrowseButton<SaveFileDialog>()
        {
            Text = "...",
            Filter = "Ultrafast Systems Data|*.ufs|Comma-separated values|*.csv|All files|*.*",
            Target = this.tb_destination,
            Location = new(490, 320 - 2),
            Size = new(30, 25),
            Parent = this
        };

        var save = new Button()
        {
            Text = "Save",
            Location = new(240, 350),
            Size = new(70, 30),
            Enabled = false,
            Parent = this,
        };
        save.Click += SaveData;
        this.tb_destination.TextChanged += (s, e) => save.Enabled = !string.IsNullOrWhiteSpace(this.tb_destination.Text) && this.data is not null;

        #endregion destination
    } // ctor ()

    internal MainWindow(string filename) : this()
    {
        this.tb_source.Text = filename;
        LoadData();
    } // ctor (string)

    private void LoadData(object? sender, EventArgs e)
        => LoadData();

    private void LoadData()
    {
        var src = this.tb_source.Text;
        if (string.IsNullOrWhiteSpace(src)) return;

        var ext = Path.GetExtension(src).ToUpper();

        try
        {
            if (ext == ".UFS")
                this.data = SpectraData.ReadFromUfs(src);
            else
                this.data = SpectraData.ReadFromCsv(src);
            SetDataInfo();
        }
        catch (Exception e)
        {
            MessageBox.Show(
                e.Message, 
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    } // private void LoadData ()

    private void SetDataInfo()
    {
        if (this.data is null) return;

        var tMin = (decimal)this.data.TimeMin;
        var tMax = (decimal)this.data.TimeMax;
        var wlMin = (decimal)this.data.WavelengthMin;
        var wlMax = (decimal)this.data.WavelengthMax;

        this.nud_timeStart.Value = tMin;
        this.num_timeEnd.Value = tMax;
        this.nud_wlStart.Value = wlMin;
        this.nud_wlEnd.Value = wlMax;

        this.nud_timeStart.Minimum = this.num_timeEnd.Minimum = tMin;
        this.nud_timeStart.Maximum = this.num_timeEnd.Maximum = tMax;
        this.nud_wlStart.Minimum = this.nud_wlEnd.Minimum = wlMin;
        this.nud_wlStart.Maximum = this.nud_wlEnd.Maximum = wlMax;

        this.tb_metadata.Text = this.data.Metadata.Replace("\r", Environment.NewLine);

        this.nud_timeStart.Enabled = this.num_timeEnd.Enabled = true;
        this.nud_wlStart.Enabled = this.nud_wlEnd.Enabled = true;
        this.trim.Enabled = true;
        this.tb_metadata.Enabled = true;
    } // private void SetDataInfo ()

    private void TrimRange(object? sender, EventArgs e)
        => TrimRange();

    private void TrimRange()
    {
        if (this.data is null) return;

        var timeStart = (double)this.nud_timeStart.Value;
        var timeEnd = (double)this.num_timeEnd.Value;
        var wlStart = (double)this.nud_wlStart.Value;
        var wlEnd = (double)this.nud_wlEnd.Value;

        if (timeStart >= timeEnd)
        {
            MessageBox.Show(
                "Invalid time range.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (wlStart >= wlEnd)
        {
            MessageBox.Show(
                "Invalid wavelength range.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var timeChanged = timeStart != this.data.TimeMin || timeEnd != this.data.TimeMax;
        var wlChanged = wlStart != this.data.WavelengthMin || wlEnd != this.data.WavelengthMax;

        if (!(timeChanged || wlChanged)) return;

        var dr = MessageBox.Show(
            "Do you want to trim the data?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        if (dr != DialogResult.Yes) return;

        if (timeChanged)
            this.data.TrimTime(timeStart, timeEnd);

        if (wlChanged)
            this.data.TrimWavelength(wlStart, wlEnd);

        SetDataInfo();
    } // private void TrimRange ()

    private void SaveData(object? sender, EventArgs e)
        => SaveData();

    private void SaveData()
    {
        if (this.data is null) return;
        var dst = this.tb_destination.Text;
        if (string.IsNullOrWhiteSpace(dst)) return;

        var ext = Path.GetExtension(dst).ToUpper();

        this.data.Metadata = this.tb_metadata.Text.Replace(Environment.NewLine, "\r");
        try
        {
            if (ext == ".UFS")
                this.data.WriteAsUfs(dst);
            else
                this.data.WriteAsCsv(dst);
            MessageBox.Show(
                "Data saved successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception e)
        {
            MessageBox.Show(
                e.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    } // private void SaveData ()
} // internal sealed class MainWindow : Form

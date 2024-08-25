
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
    private readonly Button save;

    private SpectraData? data;

    internal MainWindow()
    {
        this.Text = "SX Converter";
        this.Size = this.MinimumSize = this.MaximumSize = new(550, 430);
        this.MaximizeBox = false;
        this.Icon = Properties.Resources.Icon;

        #region source

        _ = new Label()
        {
            Text = "Source",
            Location = new(10, 10),
            Width = 80,
            Parent = this,
        };

        this.tb_source = new()
        {
            Location = new(90, 10 - 2),
            Width = 390,
            Parent = this,
        };
        this.tb_source.PathChanged += SetDestination;

        var srcBrowse = new BrowseButton<OpenFileDialog>()
        {
            Text = "...",
            FileName = GetSourceDefaultFileName,
            Filter = "All supported files|*.csv;*.ufs|Comma-separated values files|*.csv|Ultrafast Systems Data files|*.ufs|All files|*.*",
            Target = this.tb_source,
            Location = new(490, 10 - 2),
            Size = new(30, 25),
            Parent = this
        };
        srcBrowse.FileSelected += SetDestination;

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
            Width = 110,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(140, 100),
            Size = new(40, 20),
            Parent = this,
        };

        this.nud_timeStart = new()
        {
            Location = new(180, 100),
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
            Location = new(270, 100),
            Size = new Size(25, 20),
            Parent = this,
        };

        this.num_timeEnd = new()
        {
            Location = new(295, 100),
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
            Width = 110,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(140, 140),
            Size = new(40, 20),
            Parent = this,
        };

        this.nud_wlStart = new()
        {
            Location = new(180, 140),
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
            Location = new(270, 140),
            Size = new(25, 20),
            Parent = this,
        };

        this.nud_wlEnd = new()
        {
            Location = new(295, 140),
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
            Location = new(395, 115),
            Size = new(70, 30),
            Enabled = false,
            Parent = this,
        };
        this.trim.Click += TrimRange;

        _ = new Label()
        {
            Text = "Metadata",
            Location = new(30, 180),
            Width = 70,
            Parent = this,
        };

        this.tb_metadata = new()
        {
            Location = new(100, 180),
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
            Width = 80,
            Parent = this,
        };

        this.tb_destination = new()
        {
            Location = new(90, 320 - 2),
            Width = 390,
            Parent = this,
        };

        _ = new BrowseButton<SaveFileDialog>()
        {
            Text = "...",
            FileName = GetDestinationDefaultFileName,
            Filter = "Ultrafast Systems Data|*.ufs|Comma-separated values|*.csv|All files|*.*",
            FilterIndex = GetDestinationDefaultFilterIndex,
            Target = this.tb_destination,
            Location = new(490, 320 - 2),
            Size = new(30, 25),
            Parent = this
        };

        this.save = new Button()
        {
            Text = "Save",
            Location = new(240, 350),
            Size = new(70, 30),
            Enabled = false,
            Parent = this,
        };
        this.save.Click += SaveData;
        this.tb_destination.TextChanged += (s, e)
            => this.save.Enabled = !string.IsNullOrWhiteSpace(this.tb_destination.Text) && this.data is not null;

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

        this.nud_timeStart.Minimum = this.num_timeEnd.Minimum = tMin;
        this.nud_timeStart.Maximum = this.num_timeEnd.Maximum = tMax;
        this.nud_wlStart.Minimum = this.nud_wlEnd.Minimum = wlMin;
        this.nud_wlStart.Maximum = this.nud_wlEnd.Maximum = wlMax;

        this.nud_timeStart.Value = tMin;
        this.num_timeEnd.Value = tMax;
        this.nud_wlStart.Value = wlMin;
        this.nud_wlEnd.Value = wlMax;

        this.tb_metadata.Text = this.data.Metadata;

        this.nud_timeStart.Enabled = this.num_timeEnd.Enabled = true;
        this.nud_wlStart.Enabled = this.nud_wlEnd.Enabled = true;
        this.trim.Enabled = true;
        this.tb_metadata.Enabled = true;
        this.save.Enabled = this.tb_destination.TextLength > 0;
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

        this.data.Metadata = this.tb_metadata.Text;
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

    private string GetSourceDefaultFileName()
    {
        var src = this.tb_source.Text;
        if (string.IsNullOrWhiteSpace(src)) return string.Empty;
        return Path.GetFileName(src);
    } // private string GetSourceDefaultFileName ()

    private string GetDestinationDefaultFileName()
    {
        var dst = this.tb_destination.Text;
        if (!string.IsNullOrWhiteSpace(dst)) Path.GetFileName(dst);
        
        var src = this.tb_source.Text;
        if (string.IsNullOrWhiteSpace(src)) return string.Empty;
        var ext = GetDestinationDefaultFilterIndex() switch
        {
            1 => ".ufs",
            2 => ".csv",
            _ => string.Empty
        };
        return Path.GetFileNameWithoutExtension(src) + ext;
    } // private string GetDestinationDefaultFileName ()

    private int GetDestinationDefaultFilterIndex()
    {
        var dst = this.tb_destination.Text;
        if (!string.IsNullOrWhiteSpace(dst))
        {
            var dstExt = Path.GetExtension(dst).ToUpper();
            if (dstExt == ".UFS") return 1;
            if (dstExt == ".CSV") return 2;
        }

        var src = this.tb_source.Text;
        if (!string.IsNullOrWhiteSpace(src))
        {
            var srcExt = Path.GetExtension(src).ToUpper();
            if (srcExt == ".UFS") return 2;
            if (srcExt == ".CSV") return 1;
        }

        return 1;
    } // private int GetDestinationDefaultFilterIndex ()

    private void SetDestination(object? sender, EventArgs e)
    {
        if (this.tb_destination.TextLength > 0) return;

        var src = this.tb_source.Text;
        if (string.IsNullOrWhiteSpace(src)) return;

        var folder = Path.GetDirectoryName(src);
        if (folder is null) return;
        var filename = Path.GetFileNameWithoutExtension(src);
        var ext = GetDestinationDefaultFilterIndex() switch
        {
            1 => ".ufs",
            2 => ".csv",
            _ => string.Empty
        };
        var fullpath = Path.Combine(folder, filename + ext);
        this.tb_destination.Text = fullpath;
    } // private void SetDestination ()
} // internal sealed class MainWindow : Form

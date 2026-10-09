namespace MarkdownContextViewer;

partial class OperationProgressForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _enableCancelTimer?.Dispose();
            _enableCancelTimer = null;
            if (!_cts.IsCancellationRequested)
            {
                try { _cts.Cancel(); } catch { /* ignore */ }
            }

            _cts.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        panelHost = new Panel();
        lblStatus = new Label();
        progressBar = new ProgressBar();
        btnCancel = new Button();
        panelHost.SuspendLayout();
        SuspendLayout();
        // 
        // panelHost
        // 
        panelHost.Controls.Add(btnCancel);
        panelHost.Controls.Add(progressBar);
        panelHost.Controls.Add(lblStatus);
        panelHost.Dock = DockStyle.Fill;
        panelHost.Location = new Point(0, 0);
        panelHost.Name = "panelHost";
        panelHost.Padding = new Padding(12, 4, 12, 8);
        panelHost.Size = new Size(420, 130);
        panelHost.TabIndex = 0;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Top;
        lblStatus.Location = new Point(12, 4);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(8, 10, 8, 0);
        lblStatus.Size = new Size(396, 48);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "正在处理…";
        // 
        // progressBar
        // 
        progressBar.Dock = DockStyle.Top;
        progressBar.Location = new Point(12, 52);
        progressBar.Maximum = 100;
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(396, 24);
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.TabIndex = 1;
        // 
        // btnCancel
        // 
        btnCancel.Dock = DockStyle.Bottom;
        btnCancel.Enabled = false;
        btnCancel.Location = new Point(12, 88);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(396, 34);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "取消等待";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;
        // 
        // OperationProgressForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(420, 130);
        ControlBox = false;
        Controls.Add(panelHost);
        Font = new Font("Microsoft YaHei UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "OperationProgressForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "操作进度";
        panelHost.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel panelHost = null!;
    private Label lblStatus = null!;
    private ProgressBar progressBar = null!;
    private Button btnCancel = null!;
}

namespace MarkdownContextViewer;

/// <summary>
/// 短时后台操作的进度窗：显示状态、进度条，超时后可取消等待。
/// </summary>
internal sealed class OperationProgressForm : Form
{
    private readonly Label _lblStatus = new()
    {
        AutoSize = false,
        Dock = DockStyle.Top,
        Height = 48,
        Padding = new Padding(8, 10, 8, 0),
        Text = "正在处理…"
    };

    private readonly ProgressBar _bar = new()
    {
        Dock = DockStyle.Top,
        Height = 24,
        Style = ProgressBarStyle.Continuous,
        Minimum = 0,
        Maximum = 100
    };

    private readonly Button _btnCancel = new()
    {
        Text = "取消等待",
        Dock = DockStyle.Bottom,
        Height = 34,
        Enabled = false
    };

    private readonly CancellationTokenSource _cts = new();
    private readonly int _enableCancelAfterMs;
    private System.Windows.Forms.Timer? _enableCancelTimer;
    private bool _completed;

    public CancellationToken Token => _cts.Token;

    public OperationProgressForm(string title, string initialStatus, int enableCancelAfterMs = 3000)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ClientSize = new Size(420, 130);
        Font = new Font("Microsoft YaHei UI", 9f);
        _enableCancelAfterMs = Math.Max(0, enableCancelAfterMs);
        _lblStatus.Text = initialStatus;
        _btnCancel.Click += (_, _) =>
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _lblStatus.Text = "正在取消…";
                _btnCancel.Enabled = false;
            }
        };

        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 4, 12, 8) };
        host.Controls.Add(_btnCancel);
        host.Controls.Add(_bar);
        host.Controls.Add(_lblStatus);
        Controls.Add(host);
    }

    public void Report(int percent, string message)
    {
        if (IsDisposed)
        {
            return;
        }

        void Apply()
        {
            if (percent >= 0)
            {
                _bar.Value = Math.Clamp(percent, 0, 100);
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                _lblStatus.Text = message;
            }
        }

        if (InvokeRequired)
        {
            try { BeginInvoke(Apply); } catch (ObjectDisposedException) { }
            return;
        }

        Apply();
    }

    public async Task RunAsync(Action<IProgress<PreviewRegistrationProgress>, CancellationToken> work)
    {
        var progress = new Progress<PreviewRegistrationProgress>(p => Report(p.Percent, p.Message));
        if (_enableCancelAfterMs > 0)
        {
            _enableCancelTimer = new System.Windows.Forms.Timer { Interval = _enableCancelAfterMs };
            _enableCancelTimer.Tick += (_, _) =>
            {
                _enableCancelTimer.Stop();
                if (!_completed)
                {
                    _btnCancel.Enabled = true;
                }
            };
            _enableCancelTimer.Start();
        }

        Exception? error = null;
        var canceled = false;
        try
        {
            await Task.Run(() => work(progress, _cts.Token), CancellationToken.None);
            if (!_cts.IsCancellationRequested)
            {
                Report(100, "完成");
            }
            else
            {
                canceled = true;
                Report(_bar.Value, "已取消");
            }
        }
        catch (OperationCanceledException)
        {
            canceled = true;
            Report(_bar.Value, "已取消");
        }
        catch (Exception ex)
        {
            error = ex;
            Report(_bar.Value, "失败：" + ex.Message);
        }
        finally
        {
            _completed = true;
            _enableCancelTimer?.Stop();
            _btnCancel.Enabled = false;
            ControlBox = true;
            try
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(Close);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        if (canceled)
        {
            throw new OperationCanceledException();
        }

        if (error is not null)
        {
            throw error;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _enableCancelTimer?.Dispose();
        _cts.Dispose();
        base.OnFormClosed(e);
    }
}

namespace MarkdownContextViewer;

/// <summary>
/// 短时后台操作的进度窗：显示状态、进度条，超时后可取消等待。
/// </summary>
internal sealed partial class OperationProgressForm : Form
{
    private readonly CancellationTokenSource _cts = new();
    private int _enableCancelAfterMs = 3000;
    private System.Windows.Forms.Timer? _enableCancelTimer;
    private bool _completed;

    public CancellationToken Token => _cts.Token;

    public OperationProgressForm()
    {
        InitializeComponent();
    }

    public OperationProgressForm(string title, string initialStatus, int enableCancelAfterMs = 3000)
        : this()
    {
        Text = title;
        lblStatus.Text = initialStatus;
        _enableCancelAfterMs = Math.Max(0, enableCancelAfterMs);
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        if (_cts.IsCancellationRequested)
        {
            return;
        }

        _cts.Cancel();
        lblStatus.Text = "正在取消…";
        btnCancel.Enabled = false;
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
                progressBar.Value = Math.Clamp(percent, 0, 100);
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                lblStatus.Text = message;
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
                    btnCancel.Enabled = true;
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
                Report(progressBar.Value, "已取消");
            }
        }
        catch (OperationCanceledException)
        {
            canceled = true;
            Report(progressBar.Value, "已取消");
        }
        catch (Exception ex)
        {
            error = ex;
            Report(progressBar.Value, "失败：" + ex.Message);
        }
        finally
        {
            _completed = true;
            _enableCancelTimer?.Stop();
            btnCancel.Enabled = false;
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

}

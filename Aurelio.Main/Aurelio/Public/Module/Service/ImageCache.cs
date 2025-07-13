using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Aurelio.Public.Module.Service;

/// <summary>
///     图片加载服务，使用线程池处理图片加载
/// </summary>
public static class ImageCache
{
    private static readonly ThreadPool _imageThreadPool = new ThreadPool(4); // 最多同时4个线程
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingTasks = new();
    private static readonly object _lockObj = new object();

    /// <summary>
    ///     异步加载图片
    /// </summary>
    public static Task<Bitmap?> LoadImageAsync(string filePath, int targetHeight = 135)
    {
        if (!File.Exists(filePath))
            return Task.FromResult<Bitmap?>(null);

        return _imageThreadPool.QueueTask(() =>
        {
            try
            {
                using var fileStream = File.OpenRead(filePath);
                return Bitmap.DecodeToHeight(fileStream, targetHeight);
            }
            catch
            {
                return null;
            }
        });
    }

    /// <summary>
    ///     取消加载任务
    /// </summary>
    public static void CancelLoading(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        // 线程安全地移除并获取CTS
        CancellationTokenSource? cts = null;
        _pendingTasks.TryRemove(key, out cts);

        // 如果成功获取到CTS，安全地取消和释放
        if (cts != null)
        {
            try
            {
                // 仅当尚未取消时才取消
                if (!cts.IsCancellationRequested && !cts.Token.CanBeCanceled)
                {
                    cts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                // 忽略已处置对象的异常
            }
            catch (Exception ex)
            {
                // 记录其他异常但不传播
                System.Diagnostics.Debug.WriteLine($"Error cancelling task {key}: {ex.Message}");
            }
            finally
            {
                try
                {
                    cts.Dispose();
                }
                catch
                {
                    // 忽略处置时的异常
                }
            }
        }
    }

    /// <summary>
    ///     请求延迟加载图片，返回一个可以取消的令牌
    /// </summary>
    public static CancellationTokenSource RequestDelayedLoad(string key, int delayMs, Func<Task> loadAction)
    {
        // 取消之前的请求（如果有）
        CancelLoading(key);

        // 创建新的取消令牌
        var cts = new CancellationTokenSource();

        // 安全地添加到字典
        _pendingTasks.TryAdd(key, cts);

        // 启动延迟任务
        Task.Delay(delayMs, cts.Token).ContinueWith(async t =>
        {
            // 只在未取消时执行操作
            if (!t.IsCanceled)
            {
                try
                {
                    await loadAction();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in delayed task {key}: {ex.Message}");
                }
                finally
                {
                    // 完成后自动从字典中移除
                    CancellationTokenSource? removedCts;
                    _pendingTasks.TryRemove(key, out removedCts);
                }
            }
        }, TaskContinuationOptions.NotOnCanceled);

        return cts;
    }
    
    /// <summary>
    /// 清理所有挂起的任务
    /// </summary>
    public static void ClearPendingTasks()
    {
        // 获取所有键的副本
        var keys = _pendingTasks.Keys.ToArray();
        
        // 逐个取消任务
        foreach (var key in keys)
        {
            CancelLoading(key);
        }
    }
}

/// <summary>
///     简单的线程池实现
/// </summary>
public class ThreadPool
{
    private readonly SemaphoreSlim _semaphore;
    
    public ThreadPool(int maxConcurrency)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }
    
    public async Task<T> QueueTask<T>(Func<T> workItem)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await Task.Run(workItem);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
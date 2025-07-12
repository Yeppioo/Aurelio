using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Aurelio.Public.Module.Service;

/// <summary>
///     简单的图片缓存服务，用于优化截图加载性能
/// </summary>
public static class ImageCache
{
    private const int MaxCacheSize = 200; // 最大缓存数量
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();
    private static readonly ConcurrentDictionary<string, Task<Bitmap>> _loadingTasks = new();

    /// <summary>
    ///     异步获取图片，如果缓存中存在则直接返回，否则异步加载
    /// </summary>
    public static async Task<Bitmap?> GetImageAsync(string filePath, int targetHeight = 135)
    {
        if (!File.Exists(filePath))
            return null;

        var cacheKey = $"{filePath}_{targetHeight}";

        // 检查缓存
        if (_cache.TryGetValue(cacheKey, out var cachedBitmap))
            return cachedBitmap;

        // 检查是否正在加载
        if (_loadingTasks.TryGetValue(cacheKey, out var loadingTask))
            return await loadingTask;

        // 创建加载任务
        var task = LoadImageAsync(filePath, targetHeight, cacheKey);
        _loadingTasks[cacheKey] = task;

        try
        {
            var result = await task;
            return result;
        }
        finally
        {
            _loadingTasks.TryRemove(cacheKey, out _);
        }
    }

    private static async Task<Bitmap> LoadImageAsync(string filePath, int targetHeight, string cacheKey)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var fileStream = File.OpenRead(filePath);
                var bitmap = Bitmap.DecodeToHeight(fileStream, targetHeight);

                // 添加到缓存
                _cache[cacheKey] = bitmap;

                // 简单的LRU清理：如果缓存过大，移除一些旧项
                if (_cache.Count > MaxCacheSize)
                {
                    var keysToRemove = _cache.Keys.Take(_cache.Count - MaxCacheSize + 50).ToList();
                    foreach (var key in keysToRemove)
                        if (_cache.TryRemove(key, out var oldBitmap))
                            oldBitmap?.Dispose();
                }

                return bitmap;
            }
            catch
            {
                // 如果加载失败，返回空的占位符
                return CreatePlaceholderBitmap(targetHeight);
            }
        });
    }

    /// <summary>
    ///     创建占位符图片
    /// </summary>
    private static Bitmap CreatePlaceholderBitmap(int height)
    {
        // 返回 null，让 Image 控件显示空白
        return null;
    }

    /// <summary>
    ///     清理缓存
    /// </summary>
    public static void ClearCache()
    {
        foreach (var bitmap in _cache.Values) bitmap?.Dispose();
        _cache.Clear();
        _loadingTasks.Clear();
    }

    /// <summary>
    ///     预加载指定路径的图片
    /// </summary>
    public static void PreloadImages(IEnumerable<string> filePaths, int targetHeight = 135)
    {
        Task.Run(async () =>
        {
            var semaphore = new SemaphoreSlim(4, 4); // 限制并发数
            var tasks = filePaths.Select(async path =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await GetImageAsync(path, targetHeight);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        });
    }
}
namespace WorkerServiceDemo.Services
{
    public interface IDatabaseBackupService
    {
        Task<bool> BackUpDataBaseAsync();
    }
}

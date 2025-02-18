using Microsoft.Extensions.Options;
using TelegramAutomate.Abstract;
using TelegramAutomate.Abstract.Jobs;
using TelegramAutomate.Commands;

namespace TelegramAutomate.Jobs
{
    internal class CleanBlobDrive : AbstractJobService
    {
        private IEnumerable<IBlobUploadService> _blobServices { get; init; }

        public CleanBlobDrive(IScheduleConfig<CleanBlobDrive> options, IEnumerable<IBlobUploadService> blobServices) : base(options.CronExpression, options.TimeZoneInfo)
        {
            _blobServices = blobServices;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            var driveCommand = _blobServices.First(s => (s as BlobDriveCommand) is not null) as BlobDriveCommand;
            if (driveCommand is null)
            {
                return;
            }

            var folderId = driveCommand.EnsureFolder();
            if (string.IsNullOrEmpty(folderId))
            {
                return;
            }

            var thresholdDate = DateTime.UtcNow.AddDays(-5);
            var files = driveCommand.GetAllFilesInFolder(folderId);
            var oldFiles = files
                .Where(file => file.CreatedTimeDateTimeOffset < thresholdDate)
                .ToList();

            foreach (var file in oldFiles)
            {
                await driveCommand.DeleteFile(file.Id);
            }

            return;
        }
    }
}

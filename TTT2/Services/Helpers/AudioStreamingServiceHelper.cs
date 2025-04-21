using System.Text.RegularExpressions;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;

namespace TTT2.Services.Helpers
{
    public partial class AudioStreamingServiceHelper(IAudioServiceHelper audioServiceHelper) : IAudioStreamingServiceHelper
    {
        
        private readonly string _uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        [GeneratedRegex(@"bytes=(\d*)-(\d*)")]
        private static partial Regex MyRegex();
        
        public ServiceResult<(string physicalPath, long fileSize)> ValidateAndBuildPath(Guid audioId, Guid userId)
        {
            // 1. Verify the file belongs to this user
            var validateFileOwnershipResult = audioServiceHelper.ValidateAudioFileWithUserAsync(audioId, userId).GetAwaiter().GetResult();
            if (validateFileOwnershipResult.IsFailure)
                return validateFileOwnershipResult.ToFailureResult<(string, long)>();

            // 2. Build path and verify existence
            var folder = Path.Combine(_uploadsRoot, userId.ToString());
            var path   = Path.Combine(folder, $"{audioId}.mp3");
            if (!File.Exists(path))
                return ServiceResult<(string, long)>.Failure(MessageKey.Error_NotFound);

            var size = new FileInfo(path).Length;
            return ServiceResult<(string, long)>.SuccessResult((path, size));
        }

        public ServiceResult<AudioStreamResponseDTO> BuildFullStream(string physicalPath, long fileSize)
        {
            var dto = new AudioStreamResponseDTO
            {
                FileStream    = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true),
                ContentType   = "audio/mpeg",
                ContentLength = fileSize,
                StatusCode    = StatusCodes.Status200OK,
                Headers       = new AudioStreamHeadersDTO()
            };
            return ServiceResult<AudioStreamResponseDTO>.SuccessResult(dto);
        }

        public ServiceResult<AudioStreamResponseDTO> BuildPartialStream(
            Guid audioId,
            string physicalPath,
            long totalSize,
            string rangeHeader)
        {
            // Parse "bytes=start-end"
            var m = MyRegex().Match(rangeHeader);
            if (!m.Success)
                return BuildFullStream(physicalPath, totalSize);

            var start = string.IsNullOrEmpty(m.Groups[1].Value)
                ? 0
                : long.Parse(m.Groups[1].Value);
            var end = string.IsNullOrEmpty(m.Groups[2].Value)
                ? totalSize - 1
                : long.Parse(m.Groups[2].Value);

            if (start >= totalSize)
            {
                // 416: Range Not Satisfiable
                var err = new AudioStreamResponseDTO
                {
                    FileStream    = Stream.Null,
                    ContentType   = "audio/mpeg",
                    ContentLength = 0,
                    StatusCode    = StatusCodes.Status416RangeNotSatisfiable,
                    Headers = new AudioStreamHeadersDTO
                    {
                        AcceptRanges = "bytes",
                        ContentRange = $"bytes */{totalSize}"
                    }
                };
                return ServiceResult<AudioStreamResponseDTO>.Failure(MessageKey.Error_StreamRangeNotSatisfiable);
            }

            end = Math.Min(end, totalSize - 1);
            var length = end - start + 1;

            var dto = new AudioStreamResponseDTO
            {
                FileStream    = new PartialStream(physicalPath, start, length),
                ContentType   = "audio/mpeg",
                ContentLength = length,
                StatusCode    = StatusCodes.Status206PartialContent,
                Headers = new AudioStreamHeadersDTO
                {
                    AcceptRanges = "bytes",
                    ContentRange = $"bytes {start}-{end}/{totalSize}"
                }
            };
            return ServiceResult<AudioStreamResponseDTO>.SuccessResult(dto);
        }

        #region -- PartialStream Helper Class --

        private class PartialStream : Stream
        {
            private readonly FileStream _fs;
            private readonly long _length;
            private long _position;

            public PartialStream(string path, long start, long length)
            {
                _fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                _fs.Seek(start, SeekOrigin.Begin);
                _length = length;
            }

            public override bool CanRead  => true;
            public override bool CanSeek  => false;
            public override bool CanWrite => false;
            public override long Length   => _length;
            public override long Position { get => _position; set => throw new NotSupportedException(); }
            public override void Flush()  => _fs.Flush();

            public override int Read(byte[] buffer, int offset, int count)
            {
                var remaining = _length - _position;
                if (remaining <= 0) return 0;
                var toRead = (int)Math.Min(count, remaining);
                var read   = _fs.Read(buffer, offset, toRead);
                _position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value)               => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing) _fs.Dispose();
                base.Dispose(disposing);
            }
        }
        
        #endregion
    }
}
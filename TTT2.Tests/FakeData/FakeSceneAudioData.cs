using System.Collections.Concurrent;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Tests.FakeData;

public class FakeSceneAudioData : ISceneAudioData
{
    // Using a dictionary to store scene audio files with a composite key.
    private readonly ConcurrentDictionary<string, SceneAudioFile> _sceneAudioFiles = new();

    // Utility function to get a unique key for a scene audio file.
    private static string GetKey(SceneAudioFile file) => 
        $"{file.SceneId}_{file.AudioFileId}_{file.AudioType}";

    public Task<DataResult<SceneAudioFile>> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
    {
        var key = GetKey(sceneAudioFile);
        return Task.FromResult(!_sceneAudioFiles.TryAdd(key, sceneAudioFile) ? DataResult<SceneAudioFile>.AlreadyExists() : DataResult<SceneAudioFile>.Success(sceneAudioFile));
    }

    public Task<DataResult<bool>> RemoveSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
    {
        var key = GetKey(sceneAudioFile);
        return Task.FromResult(_sceneAudioFiles.TryRemove(key, out var removed) ? DataResult<bool>.Success(true) : DataResult<bool>.NotFound());
    }

    public Task<DataResult<bool>> RemoveAllSceneAudioFilesAsync(Guid sceneId)
    {
        return Task.FromResult(DataResult<bool>.Success(true));
    }

    public Task<DataResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(Guid sceneId)
    {
        var list = _sceneAudioFiles.Values.Where(x => x.SceneId == sceneId).ToList();
        return Task.FromResult(list.Count != 0 ? DataResult<List<SceneAudioFile>>.Success(list) : DataResult<List<SceneAudioFile>>.NotFound());
    }
}
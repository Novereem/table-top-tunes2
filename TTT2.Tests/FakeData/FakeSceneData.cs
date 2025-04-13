using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Tests.FakeData;

public class FakeSceneData : ISceneData
{
    // In-memory store keyed by Scene.Id.
    private readonly ConcurrentDictionary<Guid, Scene> _scenes = new();

    public Task<DataResult<Scene>> CreateSceneAsync(Scene scene)
    {
        scene.CreatedAt = DateTime.UtcNow;
        _scenes[scene.Id] = scene;
        return Task.FromResult(DataResult<Scene>.Success(scene));
    }

    public Task<DataResult<Scene>> GetSceneBySceneIdAsync(Guid sceneId)
    {
        if (_scenes.TryGetValue(sceneId, out var scene))
        {
            return Task.FromResult(DataResult<Scene>.Success(scene));
        }
        return Task.FromResult(DataResult<Scene>.NotFound());
    }

    public Task<DataResult<List<Scene>>> GetScenesByUserIdAsync(Guid userId)
    {
        var scenes = _scenes.Values.Where(s => s.UserId == userId).ToList();
        return scenes.Any()
            ? Task.FromResult(DataResult<List<Scene>>.Success(scenes))
            : Task.FromResult(DataResult<List<Scene>>.NotFound());
    }

    public Task<DataResult<Scene>> UpdateSceneAsync(Scene scene)
    {
        if (_scenes.TryGetValue(scene.Id, out var existingScene))
        {
            if (existingScene.UserId != scene.UserId)
            {
                return Task.FromResult(DataResult<Scene>.NotFound());
            }

            existingScene.Name = scene.Name;

            return Task.FromResult(DataResult<Scene>.Success(existingScene));
        }
        return Task.FromResult(DataResult<Scene>.NotFound());
    }

    public Task<DataResult<bool>> DeleteSceneAsync(Guid sceneId, Guid userId)
    {
        if (_scenes.TryGetValue(sceneId, out var scene))
        {
            if (scene.UserId != userId)
            {
                return Task.FromResult(DataResult<bool>.NotFound());
            }

            _scenes.TryRemove(sceneId, out _);
            return Task.FromResult(DataResult<bool>.Success(true));
        }
        return Task.FromResult(DataResult<bool>.NotFound());
    }

    public Task<DataResult<bool>> SceneBelongsToUserAsync(Guid sceneId, Guid userId)
    {
        if (_scenes.TryGetValue(sceneId, out var scene) && scene.UserId == userId)
        {
            return Task.FromResult(DataResult<bool>.Success(true));
        }
        return Task.FromResult(DataResult<bool>.NotFound());
    }

    // Optional helper to clear the store between tests.
    public void Clear() => _scenes.Clear();
}
﻿using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Services
{
    public interface ISceneService
    {
        Task<HttpServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<SceneGetResponseDTO>> GetSceneByIdAsync(SceneGetDTO sceneGetDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<List<SceneListItemDTO>>> GetScenesListByUserIdAsync(ClaimsPrincipal user);
        Task<HttpServiceResult<SceneUpdateResponseDTO>> UpdateSceneAsync(SceneUpdateDTO sceneUpdateDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<bool>> DeleteSceneAsync(SceneRemoveDTO sceneRemoveDTO, ClaimsPrincipal user);
    }
}

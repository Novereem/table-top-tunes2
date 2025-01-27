﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
    public enum MessageKey
    {
        /// Errors
        //Standard
        Error_InternalServerError,
        Error_InvalidInput,

        //Authentication
        Error_PasswordTooShort,
        Error_EmailTaken,
        Error_InvalidEmail,
        Error_JWTNullOrEmpty,
        Error_InvalidCredentials,
        Error_Unauthorized,

        //SceneAudio
        Error_SceneAudioAlreadyAdded,
        
        /// Successes
        //Standard
        Success_OperationCompleted,
        Success_DataRetrieved,
        
        //Authentication
        Success_Register,
        Success_Login,

        //Scenes
        Success_SceneCreation,
        
        //Audio
        Success_AudioCreation,
        
        //SceneAudio
        Success_SceneAudioAssignment,
        Success_SceneAudioRemoval,
        Success_SceneAudioFilesRetrieval,
    }
}

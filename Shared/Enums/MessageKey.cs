using System;
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
        Error_NotFound,
        Error_InternalServerErrorService,
        Error_InternalServerErrorData,
        Error_MalwareOrVirusDetected,

        //Authentication
        Error_PasswordTooShort,
        Error_EmailTaken,
        Error_InvalidEmail,
        Error_JWTNullOrEmpty,
        Error_InvalidCredentials,
        Error_Unauthorized,
        Error_InvalidOldPassword,
        Error_UsernameTaken,

        //Audio
        Error_InvalidAudioFile,
        Error_InvalidAudioFileType,
        Error_FileTooLarge,
        Error_ExceedsStorageQuota,
        Error_UnableToUploadAudioFile,
        Error_UnableToSaveAudioFileMetaData,
        
        //SceneAudio
        Error_SceneAudioAlreadyAdded,
        
        /// Successes
        //Standard
        Success_OperationCompleted,
        Success_DataRetrieved,
        
        //Authentication
        Success_Register,
        Success_Login,
        Success_UpdatedUser,

        //Scenes
        Success_SceneCreation,
        Success_SceneUpdate,
        Success_SceneRemoval,
        
        //Audio
        Success_AudioCreation,
        Success_AudioRemoval,
        
        //SceneAudio
        Success_SceneAudioAssignment,
        Success_SceneAudioRemoval,
        Success_AllSceneAudiosRemoval,
        Success_SceneAudioFilesRetrieval,
        
        //AudioStreaming
        Error_StreamingAudioFileNotFound,
        Error_StreamRangeNotSatisfiable,
    }
}

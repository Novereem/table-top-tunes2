using Shared.Enums;
using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Statics
{
    public static class MessageRepository
    {
        private static readonly Dictionary<MessageKey, MessageInfo> Messages = new()
    {
        /// Error
        //Standard
        { MessageKey.Error_InternalServerError, new MessageInfo("A general unexpected error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.InternalServerError) },
        { MessageKey.Error_InvalidInput, new MessageInfo("Invalid input provided.", "Your input is invalid. Please check and try again.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_NotFound, new MessageInfo("Target not found.", "Target was not found, please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InternalServerErrorService, new MessageInfo("An unexpected service error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InternalServerErrorData, new MessageInfo("An unexpected data error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_MalwareOrVirusDetected, new MessageInfo("Malware or virus detected.", "Malware detected.", MessageType.Error, HttpStatusCode.Forbidden) },
        
        //Authentication
        { MessageKey.Error_PasswordTooShort, new MessageInfo("Password too short when registering.", "Password is too short, the password has to be atleast 5 characters long.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_EmailTaken, new MessageInfo("Email taken.", "This email has already been taken, please try again with a different email.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InvalidEmail, new MessageInfo("Invalid email", "Please provide a valid email.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_JWTNullOrEmpty, new MessageInfo("JWT secret key is null.", "Something went wrong. Please try again.", MessageType.Error, HttpStatusCode.InternalServerError) },
        { MessageKey.Error_InvalidCredentials, new MessageInfo("Invalid credentials", "Wrong username and or password, please try again.", MessageType.Error, HttpStatusCode.Unauthorized) },
        { MessageKey.Error_Unauthorized, new MessageInfo("Not authorized", "Something went wrong, please try to re-log back in.", MessageType.Error, HttpStatusCode.Unauthorized) },
        { MessageKey.Error_InvalidOldPassword, new MessageInfo("Old password doesn't match.", "Wrong password, please try again.", MessageType.Error, HttpStatusCode.Forbidden) },
        { MessageKey.Error_UsernameTaken, new MessageInfo("Username taken", "This username is already taken, please try a different username.", MessageType.Error, HttpStatusCode.Unauthorized) },
        
        //Audio
        { MessageKey.Error_InvalidAudioFile, new MessageInfo("Invalid audio file.", "The audio file is invalid, please try another file.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InvalidAudioFileType, new MessageInfo("Invalid audio file type.", "Only MP3 files are allowed, please try again with a different file type.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_FileTooLarge, new MessageInfo("File size too large", "The maximum file size is 5MB, please try a smaller file.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_ExceedsStorageQuota, new MessageInfo("File size too large", "The maximum file size is 5MB, please try a smaller file.", MessageType.Error, HttpStatusCode.Forbidden) },
        { MessageKey.Error_UnableToUploadAudioFile, new MessageInfo("Unable to upload file.", "Unable to upload the file, please try again later.", MessageType.Error, HttpStatusCode.InternalServerError) },
        { MessageKey.Error_UnableToSaveAudioFileMetaData, new MessageInfo("Unable to save audiofile metadata.", "Unable to upload the file, please try again later.", MessageType.Error, HttpStatusCode.InternalServerError) },
        
        //SceneAudio
        { MessageKey.Error_SceneAudioAlreadyAdded, new MessageInfo("Audio already added to scene", "This audio has already been added to the scene.", MessageType.Error, HttpStatusCode.Conflict) },
        
        /// Success
        //Standard
        { MessageKey.Success_OperationCompleted, new MessageInfo("Operation completed successfully!", "Operation successful.", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_DataRetrieved, new MessageInfo("Data retrieved successfully!", "Data has been retrieved.", MessageType.Success, HttpStatusCode.OK) },

        //Authentication
        { MessageKey.Success_Login, new MessageInfo("Login successful", "Logged in successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_Register, new MessageInfo("Register successful", "Registered successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_UpdatedUser, new MessageInfo("Updated user successfully", "Updated user successfully!", MessageType.Success, HttpStatusCode.OK) },

        //Scenes
        { MessageKey.Success_SceneCreation, new MessageInfo("Created scene succesfully", "Created the scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneUpdate, new MessageInfo("Updated scene succesfully", "Updated the scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneRemoval, new MessageInfo("Removed scene succesfully", "Removed the scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        
        //Audio
        { MessageKey.Success_AudioCreation, new MessageInfo("Created audio succesfully", "Created the audio successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_AudioRemoval, new MessageInfo("Removed audio succesfully", "Removed the audio successfully!", MessageType.Success, HttpStatusCode.OK) },
        
        //SceneAudio
        { MessageKey.Success_SceneAudioAssignment, new MessageInfo("Audio assigned to scene succesfully", "Audio assigned to scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneAudioRemoval, new MessageInfo("Removed audio from scene succesfully", "Removed audio from scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_AllSceneAudiosRemoval, new MessageInfo("Removed all audio from scene succesfully", "Removed all audio from scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneAudioFilesRetrieval, new MessageInfo("Retrieved audio files from scene succesfully", "Retrieved audio files from scene successfully!", MessageType.Success, HttpStatusCode.OK) },
    };

        public static MessageInfo GetMessage(MessageKey key)
        {
            return Messages.TryGetValue(key, out var message)
                ? message
                : throw new KeyNotFoundException($"Message with key '{key}' not found.");
        }
    }
}

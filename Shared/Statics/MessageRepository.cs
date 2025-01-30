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
        { MessageKey.Error_InternalServerError, new MessageInfo("E001", "A general unexpected error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.InternalServerError) },
        { MessageKey.Error_InvalidInput, new MessageInfo("E002", "Invalid input provided.", "Your input is invalid. Please check and try again.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_NotFound, new MessageInfo("E003", "Target not found.", "Target was not found, please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InternalServerErrorService, new MessageInfo("E004", "An unexpected service error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InternalServerErrorData, new MessageInfo("E005", "An unexpected data error occurred.", "Something went wrong. Please try again later.", MessageType.Error, HttpStatusCode.BadRequest) },
        
        //Authentication
        { MessageKey.Error_PasswordTooShort, new MessageInfo("E006", "Password too short when registering.", "Password is too short, the password has to be atleast 5 characters long.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_EmailTaken, new MessageInfo("E007", "Email taken.", "This email has already been taken, please try again with a different email.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_InvalidEmail, new MessageInfo("E008", "Invalid email", "Please provide a valid email.", MessageType.Error, HttpStatusCode.BadRequest) },
        { MessageKey.Error_JWTNullOrEmpty, new MessageInfo("E009", "JWT secret key is null.", "Something went wrong. Please try again.", MessageType.Error, HttpStatusCode.InternalServerError) },
        { MessageKey.Error_InvalidCredentials, new MessageInfo("E010", "Invalid credentials", "Wrong username and or password, please try again.", MessageType.Error, HttpStatusCode.Unauthorized) },
        { MessageKey.Error_Unauthorized, new MessageInfo("E011", "Not authorized", "Something went wrong, please try to re-log back in.", MessageType.Error, HttpStatusCode.Unauthorized) },
        
        //SceneAudio
        { MessageKey.Error_SceneAudioAlreadyAdded, new MessageInfo("E012", "Audio already added to scene", "This audio has already been added to the scene.", MessageType.Error, HttpStatusCode.Conflict) },
        
        /// Success
        //Standard
        { MessageKey.Success_OperationCompleted, new MessageInfo("S001", "Operation completed successfully!", "Operation successful.", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_DataRetrieved, new MessageInfo("S002", "Data retrieved successfully!", "Data has been retrieved.", MessageType.Success, HttpStatusCode.OK) },

        //Authentication
        { MessageKey.Success_Login, new MessageInfo("S003", "Login successful", "Logged in successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_Register, new MessageInfo("S004", "Register successful", "Registered successfully!", MessageType.Success, HttpStatusCode.OK) },

        //Scenes
        { MessageKey.Success_SceneCreation, new MessageInfo("S005", "Created scene succesfully", "Created the scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        
        //Audio
        { MessageKey.Success_AudioCreation, new MessageInfo("S006", "Created audio succesfully", "Created the audio successfully!", MessageType.Success, HttpStatusCode.OK) },
        
        //SceneAudio
        { MessageKey.Success_SceneAudioAssignment, new MessageInfo("S007", "Audio assigned to scene succesfully", "Audio assigned to scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneAudioRemoval, new MessageInfo("S008", "Removed audio from scene succesfully", "Removed audio from scene successfully!", MessageType.Success, HttpStatusCode.OK) },
        { MessageKey.Success_SceneAudioFilesRetrieval, new MessageInfo("S009", "Retrieved audio files from scene succesfully", "Retrieved audio files from scene successfully!", MessageType.Success, HttpStatusCode.OK) },
    };

        public static MessageInfo GetMessage(MessageKey key)
        {
            return Messages.TryGetValue(key, out var message)
                ? message
                : throw new KeyNotFoundException($"Message with key '{key}' not found.");
        }
    }
}

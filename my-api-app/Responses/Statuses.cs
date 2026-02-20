using static System.Net.WebRequestMethods;

namespace my_api_app.Responses
{
    public class Statuses
    {
        //================================ Success related statuses ================================

        public static readonly ApiStatus Success =
            new() { HttpCode = 200, StatusCode = "SUCCESS", Message = "Success! Request processed successfully" };

        public static readonly ApiStatus Created =
            new() { HttpCode = 201, StatusCode = "CREATED", Message = "Success! Resource created successfully" };



        //================================ Client error related statuses 4XX ================================

        public static readonly ApiStatus BadRequest =
            new() { HttpCode = 400, StatusCode = "BAD_REQUEST", Message = "Bad Request! The request is invalid" };

        public static readonly ApiStatus ValidationFailed =
            new() { HttpCode = 400, StatusCode = "VALIDATION_FAILED", Message = "Validation Failed! Request validation failed" };

        public static readonly ApiStatus Unauthorized =
            new() { HttpCode = 401, StatusCode = "UNAUTHORIZED", Message = "Unauthorized! Access is denied" };

        public static readonly ApiStatus Forbidden =
            new() { HttpCode = 403, StatusCode = "FORBIDDEN", Message = "Forbidden! Access denied" };

        public static readonly ApiStatus TooManyRequests =
            new() { HttpCode = 429, StatusCode = "TOO_MANY_REQUESTS", Message = "Too many requests! Please try again later" };



        //================================ Not found related statuses ================================
        public static readonly ApiStatus ResourceNotFound =
            new() { HttpCode = 404, StatusCode = "RESOURCE_NOT_FOUND", Message = "Not Found! Resource not found" };

        public static readonly ApiStatus StaticImageNotFound =
            new() { HttpCode = 404, StatusCode = "STATIC_IMAGE_NOT_FOUND", Message = "Not Found! Static image not found" };

        public static readonly ApiStatus StaticHtmlNotFound =
            new() { HttpCode = 404, StatusCode = "STATIC_HTML_NOT_FOUND", Message = "Not Found! Static HTML not found" };



        //================================ Token related statuses ================================

        public static readonly ApiStatus TokenExpired =
            new() { HttpCode = 401, StatusCode = "TOKEN_EXPIRED", Message = "Unauthorized! JWT token has expired" };

        public static readonly ApiStatus InvalidToken =
            new() { HttpCode = 401, StatusCode = "INVALID_TOKEN", Message = "Unauthorized! Invalid JWT token" };

        public static readonly ApiStatus InvalidPasswordResetToken =
            new() { HttpCode = 401, StatusCode = "INVALID_PASSWORD_RESET_TOKEN", Message = "Unauthorized! Invalid password reset token" };




        //================================ User related statuses ================================

        public static readonly ApiStatus UserNotFound =
            new() { HttpCode = 404, StatusCode = "USER_NOT_FOUND", Message = "Not Found! User not found" };

        public static readonly ApiStatus PendingUserNotFound =
            new() { HttpCode = 404, StatusCode = "PENDING_USER_NOT_FOUND", Message = "Not Found! Pending user not found" };

        public static readonly ApiStatus UserAlreadyExists =
            new() { HttpCode = 409, StatusCode = "USER_ALREADY_EXISTS", Message = "Conflict! User already exists" };

        public static readonly ApiStatus InvalidCredentials =
            new() { HttpCode = 401, StatusCode = "INVALID_CREDENTIALS", Message = "Unauthorized! Invalid login credentials" };



        //================================ OTP related statuses ================================

        public static readonly ApiStatus OtpSent =
            new() { HttpCode = 200, StatusCode = "OTP_SENT", Message = "Success! OTP sent successfully" };

        public static readonly ApiStatus OtpVerified =
            new() { HttpCode = 200, StatusCode = "OTP_VERIFIED", Message = "Success! OTP verified successfully" };

        public static readonly ApiStatus InvalidOtp =
            new() { HttpCode = 401, StatusCode = "INVALID_OTP", Message = "Invalid OTP! Provided OTP is invalid" };

        public static readonly ApiStatus OtpExpired =
            new() { HttpCode = 401, StatusCode = "OTP_EXPIRED", Message = "OTP Expired! OTP has expired" };

        public static readonly ApiStatus OtpAlreadyUsed =
            new() { HttpCode = 401, StatusCode = "OTP_ALREADY_USED", Message = "Invalid OTP! OTP has already been used" };

        public static readonly ApiStatus UnsupportedOtpPurpose =
            new() { HttpCode = 401, StatusCode = "UNSUPORTED_OTP_PURPOSE", Message = "Bad Request! OTP purpose is invalid." };



        //================================ Server Error related statuses 5XX ================================

        public static readonly ApiStatus InternalServerError =
            new() { HttpCode = 500, StatusCode = "INTERNAL_SERVER_ERROR", Message = "Server Error! An unexpected server error occurred" };

        public static readonly ApiStatus ServiceUnavailable =
            new() { HttpCode = 503, StatusCode = "SERVICE_UNAVAILABLE", Message = "Service temporarily unavailable" };

        public static readonly ApiStatus SmtpServiceUnavailable =
            new() { HttpCode = 503, StatusCode = "SMTP_SERVICE_UNAVAILABLE", Message = "SMTP service is temporarily unavailable" };
    }
}

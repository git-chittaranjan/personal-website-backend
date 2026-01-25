namespace my_api_app.Responses
{
    public class Statuses
    {
        public static readonly ApiStatus Success =
            new() { HttpCode = 200, Code = "SUCCESS", Message = "Success! Request processed successfully" };

        public static readonly ApiStatus Created =
            new() { HttpCode = 201, Code = "CREATED", Message = "Success! Resource created successfully" };

        public static readonly ApiStatus OtpSent =
            new() { HttpCode = 200, Code = "OTP_SENT", Message = "Success! OTP sent successfully" };

        public static readonly ApiStatus OtpVerified =
            new() { HttpCode = 200, Code = "OTP_VERIFIED", Message = "Success! OTP verified successfully" };

        public static readonly ApiStatus BadRequest =
            new() { HttpCode = 400, Code = "BAD_REQUEST", Message = "Bad Request! The request is invalid" };

        public static readonly ApiStatus ValidationFailed =
            new() { HttpCode = 400, Code = "VALIDATION_FAILED", Message = "Validation Failed! Request validation failed" };

        public static readonly ApiStatus Unauthorized =
            new() { HttpCode = 401, Code = "UNAUTHORIZED", Message = "Unauthorized! Access is denied" };

        public static readonly ApiStatus TokenExpired =
            new() { HttpCode = 401, Code = "TOKEN_EXPIRED", Message = "Unauthorized! JWT token has expired" };

        public static readonly ApiStatus InvalidToken =
            new() { HttpCode = 401, Code = "INVALID_TOKEN", Message = "Unauthorized! Invalid JWT token" };

        public static readonly ApiStatus Forbidden =
            new() { HttpCode = 403, Code = "FORBIDDEN", Message = "Forbidden! Access denied" };

        public static readonly ApiStatus UserNotFound =
            new() { HttpCode = 404, Code = "USER_NOT_FOUND", Message = "Not Found! User not found" };

        public static readonly ApiStatus InvalidOtp =
            new() { HttpCode = 401, Code = "INVALID_OTP", Message = "Invalid OTP! Provided OTP is invalid" };

        public static readonly ApiStatus OtpExpired =
            new() { HttpCode = 401, Code = "OTP_EXPIRED", Message = "OTP Expired! OTP has expired" };

        public static readonly ApiStatus OtpAlreadyUsed =
            new() { HttpCode = 401, Code = "OTP_ALREADY_USED", Message = "Invalid OTP! OTP has already been used" };

        public static readonly ApiStatus UserAlreadyExists =
            new() { HttpCode = 409, Code = "USER_ALREADY_EXISTS", Message = "Conflict! User already exists" };

        public static readonly ApiStatus InvalidCredentials =
            new() { HttpCode = 401, Code = "INVALID_CREDENTIALS", Message = "Unauthorized! Invalid login credentials" };

        public static readonly ApiStatus InternalError =
            new() { HttpCode = 500, Code = "INTERNAL_SERVER_ERROR", Message = "Server Error! An unexpected server error occurred" };

        public static readonly ApiStatus TooManyRequests =
            new() { HttpCode = 429, Code = "TOO_MANY_REQUESTS", Message = "Too many requests! Please try again later" };

        public static readonly ApiStatus ServiceUnavailable =
            new() { HttpCode = 503, Code = "SERVICE_UNAVAILABLE", Message = "Service temporarily unavailable" };
    }
}

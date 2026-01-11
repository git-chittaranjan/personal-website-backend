
A secure User Registration/Login application, built in ASP.NET Core Web API, using ADO.NET with JWT authentication, refresh token rotation, OTP verification and FluentValidation.

## Tech Stack
- .NET: ASP.NET Core Web API (.NET 10)
- Language: C#
- Database: SQL Server
- Data Access: ADO.NET (SqlConnection, SqlCommand)
- Authentication: JWT (Access Token + Refresh Token)
- Validation: FluentValidation
- Password Security: PBKDF2
- Email: SMTP Server (OTP delivery)
- Hosting: Azure App Aervice, Key-Vault


## Packages
- FluentValidation - Version="12.1.1"
- FluentValidation.AspNetCore" Version="11.3.1"
- Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.1"
- Microsoft.Data.SqlClient" Version="6.1.3"


## Authentication Flow
- User Registration
  - Validates input using FluentValidation
  - Hashes password using PBKDF2
  - Stores user with UNIQUEIDENTIFIER (GUID) as UserID
  - Sends OTP via email
- Login
  - Validates email + password
  - Verifies OTP
  - Issues:
    - Access Token (JWT – short lived)
    - Refresh Token (stored in DB)
- Authenticated Requests
  - Client sends JWT in header:
  - Authorization: Bearer <access_token>
  - JWT middleware validates token
  - Builds ClaimsPrincipal
  - Accessible via User in controllers


## Register cURL
curl --location 'https://personal-website-backend-btbdemf2b6cxfehf.southindia-01.azurewebsites.net/api/auth/register' \
--header 'Content-Type: application/json' \
--header 'Cookie: ARRAffinity=79e06db539acb57119e709978d2cf1da299e8341753d6f6345007fcab3f69bc5; ARRAffinitySameSite=79e06db539acb57119e709978d2cf1da299e8341753d6f6345007fcab3f69bc5' \
--data-raw '{
  "name": "Nayan Saha",
  "gender": "Male",
  "email": "nayan.saha@gmail.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}'


using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace EmployeeLeaveManagement.Helpers
{
    public static class SessionHelper
    {
    
        private const string UserIdKey = "UserId";
        private const string UserNameKey = "UserName";
        private const string UserEmailKey = "UserEmail";
        private const string UserRoleKey = "UserRole";

       
        public static void SetUserSession(ISession session, int userId, string userName, string email, string role)
        {
            session.SetInt32(UserIdKey, userId);
            session.SetString(UserNameKey, userName);
            session.SetString(UserEmailKey, email);
            session.SetString(UserRoleKey, role);
        }

        
        public static int? GetUserId(ISession session)
        {
            return session.GetInt32(UserIdKey);
        }

       
        public static string? GetUserName(ISession session)
        {
            return session.GetString(UserNameKey);
        }

        public static string? GetUserEmail(ISession session)
        {
            return session.GetString(UserEmailKey);
        }

       
        public static string? GetUserRole(ISession session)
        {
            return session.GetString(UserRoleKey);
        }

      
        public static bool IsLoggedIn(ISession session)
        {
            return session.GetInt32(UserIdKey).HasValue;
        }

      
        public static bool IsAdmin(ISession session)
        {
            var role = session.GetString(UserRoleKey);
            return role != null && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        
        public static bool IsEmployee(ISession session)
        {
            var role = session.GetString(UserRoleKey);
            return role != null && role.Equals("Employee", StringComparison.OrdinalIgnoreCase);
        }

      
        public static void ClearSession(ISession session)
        {
            session.Clear();
        }

        
        public static void SetObjectAsJson(ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        
        public static T? GetObjectFromJson<T>(ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
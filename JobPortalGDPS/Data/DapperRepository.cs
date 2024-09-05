using Dapper;
using JobPortalGDPS.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JobPortalGDPS.Data
{
    public class DapperRepository
    {
        private readonly string _connectionString;

        public DapperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Create and return a new database connection
        private IDbConnection GetConnection() => new SqlConnection(_connectionString);

        // Test the database connection
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Try opening the connection
                    return true; // Connection successful
                }
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework in production)
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false; // Connection failed
            }
        }

        // Check if a user exists by email
        public async Task<bool> UserExistsAsync(string email)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                return await connection.ExecuteScalarAsync<bool>(query, new { Email = email });
            }
        }

        // Verify user credentials for login
        public async Task<User> VerifyUserAsync(string email, string passwordHash)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email, PasswordHash = passwordHash });
            }
        }

        // Get all jobs
        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Jobs";
                return await connection.QueryAsync<Job>(query);
            }
        }

        // Get a job by ID
        public async Task<Job> GetJobByIdAsync(int jobId)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Jobs WHERE JobId = @JobId";
                return await connection.QueryFirstOrDefaultAsync<Job>(query, new { JobId = jobId });
            }
        }

        // Add a new application
        public async Task AddApplicationAsync(Application application)
        {
            var sql = @"
                    INSERT INTO Applications (UserId, JobId, ResumeFile, CreatedAt)
                    VALUES (@UserId, @JobId, @ResumeFile, @CreatedAt)";

            using (var connection = GetConnection())
            {
                await connection.ExecuteAsync(sql, new
                {
                    application.UserId,
                    application.JobId,
                    application.ResumeFile, // Binary data
                    application.CreatedAt
                });
            }
        }

        // Check if an application already exists for the user and job
        public async Task<bool> ApplicationExistsAsync(int userId, int jobId)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Applications
                WHERE UserId = @UserId AND JobId = @JobId";

            using (var connection = GetConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, JobId = jobId });
                return count > 0;
            }
        }

        // Get a list of applied jobs with details for a specific user
        public async Task<IEnumerable<AppliedJobViewModel>> GetAppliedJobsWithDetailsAsync(int userId)
        {
            var sql = @"
                SELECT a.ApplicationId, j.Title AS JobTitle, a.CreatedAt, a.ResumeFile
                FROM Applications a
                JOIN Jobs j ON a.JobId = j.JobId
                WHERE a.UserId = @UserId";

            using (var connection = GetConnection())
            {
                return await connection.QueryAsync<AppliedJobViewModel>(sql, new { UserId = userId });
            }
        }


        // Get an application by its ID
        public async Task<Application> GetApplicationByIdAsync(int applicationId)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Applications WHERE ApplicationId = @ApplicationId";
                return await connection.QueryFirstOrDefaultAsync<Application>(query, new { ApplicationId = applicationId });
            }
        }

        // Delete an application
        public async Task DeleteApplicationAsync(int applicationId)
        {
            using (var connection = GetConnection())
            {
                var query = "DELETE FROM Applications WHERE ApplicationId = @ApplicationId";
                await connection.ExecuteAsync(query, new { ApplicationId = applicationId });
            }
        }

        // Add a new user
        public async Task AddUserAsync(User user)
        {
            using (var connection = GetConnection())
            {
                var query = "INSERT INTO Users (Email, PasswordHash, FirstName, LastName, PhoneNumber) VALUES (@Email, @PasswordHash, @FirstName, @LastName, @PhoneNumber)";
                await connection.ExecuteAsync(query, user);
            }
        }

        // Get a user by email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Users WHERE Email = @Email";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
            }
        }

        // Get a user by ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var connection = GetConnection())
            {
                var query = "SELECT * FROM Users WHERE UserId = @UserId";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { UserId = userId });
            }
        }

        // Update user information
        public async Task UpdateUserAsync(User user)
        {
            using (var connection = GetConnection())
            {
                var query = @"
                    UPDATE Users 
                    SET Email = @Email, 
                        FirstName = @FirstName, 
                        LastName = @LastName, 
                        PhoneNumber = @PhoneNumber,
                        PasswordHash = @PasswordHash
                    WHERE UserId = @UserId";

                await connection.ExecuteAsync(query, new
                {
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.PasswordHash,
                    user.UserId
                });
            }
        }
    }
}

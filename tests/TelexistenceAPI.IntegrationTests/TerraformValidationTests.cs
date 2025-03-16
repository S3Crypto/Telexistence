using System.Diagnostics;
using Xunit;

namespace TelexistenceAPI.IntegrationTests.Infrastructure
{
    // Mark the class as skipped since Terraform tests are infrastructure-specific
    [Trait("Category", "TerraformTests")]
    public class TerraformValidationTests
    {
        // Try both possible paths to the terraform directory
        private string FindTerraformDir()
        {
            // Start from the current directory and try to find the terraform dir
            var currentDir = Directory.GetCurrentDirectory();

            // Try different possible locations
            var possiblePaths = new[]
            {
                // Direct path in case we're at project root
                Path.Combine(currentDir, "terraform"),
                // Path if we're in the test directory
                Path.Combine(currentDir, "..", "..", "..", "..", "terraform"),
                // Alternative path to try
                Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "terraform"))
            };

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            // If we couldn't find the directory, return a non-existent path
            // This will cause the test to skip
            return "";
        }

        [Fact(Skip = "Terraform tests require Terraform CLI to be installed")]
        public void Terraform_Validate_ShouldSucceed()
        {
            // Find terraform directory
            var terraformDir = FindTerraformDir();
            if (string.IsNullOrEmpty(terraformDir) || !Directory.Exists(terraformDir))
            {
                Assert.True(true, $"Skipping test: Terraform directory not found");
                return;
            }

            // Skip if terraform is not installed
            if (!IsTerraformInstalled())
            {
                Assert.True(true, "Skipping test: Terraform is not installed on this machine.");
                return;
            }

            // Initialize terraform
            var initResult = RunTerraformCommand(terraformDir, "init", "-backend=false");
            Assert.True(initResult.ExitCode == 0, $"Terraform init failed: {initResult.Output}");

            // Validate terraform configuration
            var validateResult = RunTerraformCommand(terraformDir, "validate");
            Assert.True(
                validateResult.ExitCode == 0,
                $"Terraform validation failed: {validateResult.Output}"
            );
        }

        [Fact(Skip = "Terraform tests require Terraform CLI to be installed")]
        public void Terraform_Plan_ShouldSucceed()
        {
            // Find terraform directory
            var terraformDir = FindTerraformDir();
            if (string.IsNullOrEmpty(terraformDir) || !Directory.Exists(terraformDir))
            {
                Assert.True(true, $"Skipping test: Terraform directory not found");
                return;
            }

            // Skip if terraform is not installed
            if (!IsTerraformInstalled())
            {
                Assert.True(true, "Skipping test: Terraform is not installed on this machine.");
                return;
            }

            // Set environment variables for the test
            var environmentVariables = new Dictionary<string, string>
            {
                { "TF_VAR_resource_group_name", "test-rg" },
                { "TF_VAR_location", "westus2" },
                { "TF_VAR_app_service_plan_name", "test-plan" },
                { "TF_VAR_app_service_name", "test-api" },
                { "TF_VAR_mongodb_connection_string", "mongodb://localhost:27017" },
                { "TF_VAR_mongodb_database_name", "TestDB" },
                { "TF_VAR_jwt_key", "TestSecretKey123" },
                { "TF_VAR_jwt_issuer", "TestIssuer" },
                { "TF_VAR_jwt_audience", "TestAudience" },
                { "TF_VAR_allowed_origins", "[]" }
            };

            // Initialize terraform
            var initResult = RunTerraformCommand(
                terraformDir,
                "init",
                "-backend=false",
                environmentVariables
            );
            Assert.True(initResult.ExitCode == 0, $"Terraform init failed: {initResult.Output}");

            // Run terraform plan
            var planResult = RunTerraformCommand(
                terraformDir,
                "plan",
                "-detailed-exitcode",
                environmentVariables
            );

            // Exit code 0 means no changes, 2 means changes would be made but that's expected in a test
            Assert.True(
                planResult.ExitCode == 0 || planResult.ExitCode == 2,
                $"Terraform plan failed with exit code {planResult.ExitCode}: {planResult.Output}"
            );
        }

        private (int ExitCode, string Output) RunTerraformCommand(
            string terraformDir,
            string command,
            string args = "",
            Dictionary<string, string>? environmentVariables = null
        )
        {
            try
            {
                // Print directory for debugging
                Console.WriteLine($"Terraform directory: {terraformDir}");
                Console.WriteLine($"Directory exists: {Directory.Exists(terraformDir)}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "terraform",
                    Arguments = $"{command} {args}",
                    WorkingDirectory = terraformDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Add environment variables if provided
                if (environmentVariables != null)
                {
                    foreach (var variable in environmentVariables)
                    {
                        startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                    }
                }

                var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                output += process.StandardError.ReadToEnd();
                process.WaitForExit();

                return (process.ExitCode, output);
            }
            catch (Exception ex)
            {
                return (-1, $"Error executing terraform command: {ex.Message}");
            }
        }

        private bool IsTerraformInstalled()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "terraform",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

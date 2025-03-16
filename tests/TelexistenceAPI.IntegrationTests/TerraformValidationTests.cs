using System.Diagnostics;
using Xunit;

namespace TelexistenceAPI.IntegrationTests.Infrastructure
{
    public class TerraformValidationTests
    {
        private readonly string _terraformDir = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "terraform")
        );

        [Fact]
        public void Terraform_Validate_ShouldSucceed()
        {
            // Skip if terraform is not installed
            if (!IsTerraformInstalled())
            {
                Skip.If(true, "Terraform is not installed on this machine.");
                return;
            }

            // Initialize terraform
            var initResult = RunTerraformCommand("init", "-backend=false");
            Assert.True(initResult.ExitCode == 0, $"Terraform init failed: {initResult.Output}");

            // Validate terraform configuration
            var validateResult = RunTerraformCommand("validate");
            Assert.True(
                validateResult.ExitCode == 0,
                $"Terraform validation failed: {validateResult.Output}"
            );
        }

        [Fact]
        public void Terraform_Plan_ShouldSucceed()
        {
            // Skip if terraform is not installed
            if (!IsTerraformInstalled())
            {
                Skip.If(true, "Terraform is not installed on this machine.");
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
            var initResult = RunTerraformCommand("init", "-backend=false", environmentVariables);
            Assert.True(initResult.ExitCode == 0, $"Terraform init failed: {initResult.Output}");

            // Run terraform plan
            var planResult = RunTerraformCommand(
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
            string command,
            string args = "",
            Dictionary<string, string>? environmentVariables = null
        )
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "terraform",
                Arguments = $"{command} {args}",
                WorkingDirectory = _terraformDir,
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
